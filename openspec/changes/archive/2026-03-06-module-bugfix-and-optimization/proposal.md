## Why

对 CoolGameFramework 各模块进行全面审查后，发现多个模块存在功能性 Bug、线程安全漏洞和性能问题，影响框架在生产环境的稳定性。这些问题涵盖 NetworkManager 的竞态条件、ResourceManager 的异步加载失效、TimerManager 的精度丢失、ObjectPoolManager 的无边界增长等高优先级缺陷，需要集中修复以提升框架整体可靠性。

## What Changes

- **NetworkManager**：修复异步回调中的竞态条件（lock 保护共享状态），增加消息分发接口，添加连接超时和心跳检测
- **ResourceManager**：修复 AssetBundle/Addressable 模式下异步加载未实现的 Bug，添加资源引用计数，修复 Unload 无法安全卸载资源的问题
- **TimerManager**：修复循环定时器精度丢失（余数保留），将 `List.RemoveAt(0)` 改为标记删除以降低 GC 压力
- **ObjectPoolManager**：为泛型 `ObjectPool<T>` 添加 `maxSize` 上限，防止无限增长；统一 Pool 数据结构
- **UIManager**：修复 `CloseUI` 时 Stack 与 Dictionary 不同步的问题（中间层关闭不会正确出栈）
- **LogManager**：将日志缓存从 `List<T>` 改为循环队列，消除 `RemoveAt(0)` 的 O(n) 开销
- **AudioManager**：修复音效 Source 覆盖逻辑，添加音效优先级机制

## Capabilities

### New Capabilities

无新增用户可见能力（均为 Bug 修复和内部优化）

### Modified Capabilities

- `save-manager`：无需修改（已独立实现）

## Impact

- **修改文件**：
  - `Modules/Network/NetworkManager.cs`
  - `Modules/Resource/ResourceManager.cs`
  - `Modules/Timer/TimerManager.cs`
  - `Modules/ObjectPool/ObjectPoolManager.cs`
  - `Modules/UI/UIManager.cs`
  - `Modules/Log/LogManager.cs`
  - `Modules/Audio/AudioManager.cs`
- **API 兼容性**：所有修改均向下兼容，不改变公共接口签名
- **无 Breaking Change**
