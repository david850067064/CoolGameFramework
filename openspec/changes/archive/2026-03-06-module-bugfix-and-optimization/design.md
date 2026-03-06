## Context

对框架 17 个模块的全面审查发现了 7 个关键模块存在功能性 Bug、竞态条件和性能问题。这些问题在模块独立运行时可能不易察觉，但在生产环境（多并发、长时间运行、大量资源）下会导致不可预期的行为。本次修复涉及 7 个模块，均为内部实现改动，不改变公共 API。

## Goals / Non-Goals

**Goals:**
- 消除 NetworkManager 异步回调中的竞态条件
- 修复 ResourceManager 在 AssetBundle 模式下异步加载无效的 Bug
- 修复 TimerManager 循环定时器精度丢失问题
- 为 ObjectPool<T> 添加 maxSize 防止内存无限增长
- 修复 UIManager CloseUI 时 Stack 与 Dictionary 不同步
- 将 LogManager 日志缓存改为 O(1) 循环队列
- 修复 AudioManager 音效 Source 覆盖逻辑

**Non-Goals:**
- 实现 Addressable 完整集成（HotUpdate 范畴）
- HybridCLR DLL 加载（需要单独引入依赖）
- SDK 平台接入（需要真实 SDK）
- 添加新的公共 API（本次仅修复现有行为）

## Decisions

### 决策 1：NetworkManager — 使用 `lock` 而非 `Interlocked`

**选择**：在 `ConnectCallback`、`ReceiveCallback`、`Disconnect` 等异步回调中使用 `lock(_lockObj)` 保护共享状态（`_isConnected`、`_socket`）。

**理由**：
- `Interlocked` 只适合单个原子变量，多个字段需要原子性修改时 `lock` 更简单安全
- Unity 不推荐 `volatile`，`lock` 是 C# 中最清晰的临界区表达方式

**备选**：`ConcurrentDictionary` + `Interlocked` —— 过于复杂，不适合少量状态字段。

---

### 决策 2：ResourceManager 异步加载 — 分模式实现

**选择**：在 `LoadAsyncCoroutine` 中对三种模式分别实现：Resources 用 `Resources.LoadAsync`，AssetBundle 用 `AssetBundleRequest`，Addressable 暂时 fallback 到同步加载并打印警告。

**理由**：Addressable 完整集成需要引入 `com.unity.addressables` 包，超出本次范围；先补全 AssetBundle 路径，Addressable 留警告提示。

---

### 决策 3：TimerManager — 余数保留

**选择**：循环定时器触发后执行 `ElapsedTime -= Duration`（余数保留）而非 `ElapsedTime = 0`（重置）。

**理由**：余数保留可避免高频定时器因帧率波动造成的累计偏差，这是定时器系统的标准做法（Unity Animator、DOTween 均采用此方式）。

---

### 决策 4：ObjectPool<T> — maxSize 默认值

**选择**：为 `ObjectPool<T>` 构造函数添加 `maxSize` 参数，默认值 `0`（0 表示不限制，保持向后兼容）。Release 时若当前 Count >= maxSize 则直接调用 `onDestroy` 而不入栈。

**理由**：默认不限制保持向后兼容；有需要的业务层可显式传入上限。

---

### 决策 5：UIManager Stack 修复 — 改用 List 替代 Stack

**选择**：将 `_uiStack` 从 `Stack<UIBase>` 改为 `List<UIBase>`，CloseUI 时使用 `List.Remove(ui)` 精确删除对应项。

**理由**：`Stack` 只支持 Pop 栈顶，无法删除中间元素；`List` 支持任意位置删除，并且可以通过 `list[^1]` 访问最顶层 UI，语义等价。

---

### 决策 6：LogManager — 使用数组实现循环队列

**选择**：用固定大小数组 + 头尾指针实现 O(1) 循环队列替代 `List<string>` + `RemoveAt(0)`。

**理由**：`List.RemoveAt(0)` 为 O(n)，在日志频繁写入时 GC 压力大。数组循环队列无 GC，读写均为 O(1)。

---

### 决策 7：AudioManager — 优先级机制

**选择**：为每个 SFX 播放请求增加 `priority` 参数（默认 0），当所有 Source 都在使用时，替换优先级最低的正在播放的 Source，而非直接使用第一个。

**理由**：游戏中重要音效（UI 点击、受击反馈）不应被低优先级环境音替换，优先级机制是标准音频系统的基础功能。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|---|---|
| NetworkManager lock 增加主线程等待 | lock 范围极小（仅变量赋值），阻塞时间 < 1μs，可忽略 |
| TimerManager 余数保留改变现有行为 | 仅影响循环定时器的触发时机误差，不影响总次数，视为 bugfix |
| UIManager List 替代 Stack 性能略低 | UI 数量通常 < 20，List.Remove 的 O(n) 在此规模无影响 |
| ObjectPool maxSize 默认 0 | 向下兼容，不影响现有代码 |

## Migration Plan

所有修改均为内部实现，公共 API 签名不变，无需业务层迁移。`ObjectPool<T>` 构造函数新增可选参数，旧调用代码无需修改。
