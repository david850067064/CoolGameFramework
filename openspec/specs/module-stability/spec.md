### Requirement: NetworkManager 线程安全
NetworkManager 的异步回调（连接、接收、断开）SHALL 使用互斥锁保护共享状态，确保多线程环境下 `_isConnected`、`_socket` 等字段的读写安全。

#### Scenario: 并发连接回调不产生竞态
- **WHEN** 多个异步回调同时触发
- **THEN** 共享状态在锁保护下串行修改，不出现脏读或重复操作

#### Scenario: 断开连接时状态一致
- **WHEN** 调用 `Disconnect()` 时另一线程正在执行 `ReceiveCallback`
- **THEN** 不抛出 `ObjectDisposedException`，状态最终一致

---

### Requirement: ResourceManager 异步加载正确性
ResourceManager 的 `LoadAsync<T>` SHALL 在所有已实现的加载模式（Resources、AssetBundle）下均能正确完成异步加载并回调结果。

#### Scenario: AssetBundle 模式下异步加载成功
- **WHEN** 加载模式为 AssetBundle，调用 `LoadAsync<GameObject>("bundleName/assetName", callback)`
- **THEN** 系统发起 AssetBundle 异步请求，资源加载完成后触发 callback

#### Scenario: 未实现模式下给出警告而非静默失败
- **WHEN** 加载模式为 Addressable，调用 `LoadAsync`
- **THEN** 系统打印警告日志并回调 null，不静默挂起

---

### Requirement: TimerManager 循环精度
循环定时器触发后 SHALL 保留超出周期的时间余数，以防止高频定时器因帧率波动产生累计偏差。

#### Scenario: 高频循环定时器精度
- **WHEN** 循环定时器周期为 0.1s，某帧 deltaTime = 0.15s 导致定时器触发
- **THEN** 下一周期从余数 0.05s 开始计时，而非从 0 重新开始

#### Scenario: 标记删除不影响当帧其他定时器
- **WHEN** 定时器在回调中调用 `Stop()`
- **THEN** 当帧其余定时器正常 Update，不因列表修改产生越界或跳过

---

### Requirement: ObjectPool<T> 内存边界
泛型 `ObjectPool<T>` SHALL 支持配置最大容量，超出上限时 Release 的对象执行销毁回调而非入池，防止内存无限增长。

#### Scenario: 超出 maxSize 时不入池
- **WHEN** Pool 当前 Count 已达 maxSize，调用 `Release(obj)`
- **THEN** 系统调用 `onDestroy(obj)` 并丢弃该对象，池大小不超过 maxSize

#### Scenario: maxSize 为 0 时不限制
- **WHEN** 创建 Pool 时不传入 maxSize（或传入 0）
- **THEN** 对象无上限地入池，保持向下兼容

---

### Requirement: UIManager 精确关闭
UIManager 的 `CloseUI` SHALL 精确从 UI 历史栈中移除目标 UI，不依赖栈顶假设，支持关闭任意层级的 UI。

#### Scenario: 关闭中间层 UI
- **WHEN** 同时打开了 UI_A（底层）、UI_B（中层）、UI_C（顶层），调用 `CloseUI("UI_B")`
- **THEN** 历史栈变为 [UI_A, UI_C]，UI_A 和 UI_C 不受影响

#### Scenario: 关闭后栈与字典状态一致
- **WHEN** 调用任意 `CloseUI`
- **THEN** `_openedUIs` 字典和历史栈中对应 UI 均被移除，两者始终同步

---

### Requirement: LogManager 高效缓存
LogManager 的日志缓存 SHALL 使用 O(1) 时间复杂度的数据结构，避免在高频日志写入场景下产生 GC 压力。

#### Scenario: 缓存写入性能
- **WHEN** 日志缓存已满（达到 maxCacheSize），写入新日志
- **THEN** 最旧的日志被覆盖，操作时间复杂度为 O(1)，不触发数组拷贝

#### Scenario: 缓存读取完整性
- **WHEN** 调用 `GetCachedLogs()` 获取缓存日志
- **THEN** 返回按时间顺序排列的日志列表，最多 maxCacheSize 条

---

### Requirement: AudioManager 音效优先级
AudioManager 的音效播放 SHALL 支持优先级参数，当所有 Source 均在使用时替换优先级最低的正在播放的音效，而非静默失败或随机覆盖。

#### Scenario: 高优先级音效抢占低优先级 Source
- **WHEN** 所有 SFX Source 均在播放，调用 `PlaySFX("impact", priority: 10)`
- **THEN** 系统找到当前优先级最低的 Source，停止其音效并播放新音效

#### Scenario: 同等优先级时不抢占
- **WHEN** 所有 SFX Source 均在播放相同优先级音效，调用 `PlaySFX("ambient", priority: 0)`
- **THEN** 系统记录警告但不强制抢占，新音效等待或跳过
