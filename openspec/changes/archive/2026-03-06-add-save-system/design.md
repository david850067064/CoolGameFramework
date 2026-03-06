## Context

CoolGameFramework 现有模块体系完善，但缺少统一的数据持久化方案。业务层通常直接使用 `PlayerPrefs` 或手写文件读写，导致：存档格式分散、无法统一加密、多存档槽位支持困难。框架已有 `JsonUtil`、`EncryptUtil`、`FileUtil` 工具类，可复用于存档底层实现。

## Goals / Non-Goals

**Goals:**
- 提供标准化 `SaveManager` 模块，统一接入 `GameEntry.Save`
- 支持多槽位（Slot）独立管理
- 支持 JSON 序列化 + 可选 AES 加密 + 可选 GZIP 压缩
- 提供存档元数据（时间戳、游戏时长、版本号）
- 支持同步与异步读写 API
- 提供版本迁移钩子

**Non-Goals:**
- 云存档同步（由 SDKManager 扩展负责）
- 二进制自定义序列化（不在本次范围）
- 存档冲突解决（多设备）
- Editor 可视化存档查看工具

## Decisions

### 决策 1：存档文件格式选 JSON 而非二进制

**选择**：JSON + 可选 AES-128 加密 + 可选 GZIP 压缩

**理由**：
- 框架已有 `JsonUtil` 和 `EncryptUtil`，复用成本低
- JSON 可读性强，便于调试和版本迁移
- 加密和压缩作为可选开关，不强制业务层

**备选方案**：MessagePack / ProtoBuf —— 性能更好但引入外部依赖，不符合框架轻量原则。

---

### 决策 2：槽位用文件夹隔离，每个槽位一个文件

**结构**：
```
{persistentDataPath}/Saves/
    slot_0/
        save.dat       # 存档数据（JSON/加密）
        meta.json      # 元数据（明文，方便读取缩略图、时间等）
    slot_1/
        save.dat
        meta.json
```

**理由**：
- 槽位之间相互独立，删除/覆盖操作简单
- meta.json 始终明文，UI 展示存档列表无需解密整个文件

---

### 决策 3：版本迁移使用回调钩子，不内置迁移引擎

业务层注册 `OnMigrate(int fromVersion, int toVersion, JObject rawData)` 回调，框架负责检测版本差异并触发回调，具体数据转换由业务层实现。

**理由**：框架无法预知业务数据结构，强行内置迁移逻辑反而增加复杂度。

---

### 决策 4：模块不继承 MonoBehaviour，异步用 C# Task

与其他模块保持一致（`ModuleBase` 为普通 C# 类），异步 IO 使用 `System.Threading.Tasks.Task`，避免引入 UniTask 依赖。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|---|---|
| JSON 序列化大型存档时 GC 压力 | 提供 `SaveAsync` 异步接口，避免主线程卡顿 |
| AES 密钥硬编码风险 | 密钥由业务层通过 `SaveConfig` 注入，框架不存储默认密钥 |
| 写入中断导致存档损坏 | 先写临时文件再原子替换（`File.Move` 覆盖） |
| 版本迁移未注册导致旧存档不可读 | 框架抛出明确异常并提供 `CanLoad(slot)` 预检查 API |

## Migration Plan

1. 纯新增模块，无需迁移现有代码
2. 业务层可选择性迁移 `PlayerPrefs` 数据：在首次 `OnMigrate` 回调中读取旧 `PlayerPrefs` 并写入新格式
3. `GameEntry.cs` 新增一行注册语句，不影响其他模块
