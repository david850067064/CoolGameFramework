## 1. NetworkManager — 线程安全修复

- [x] 1.1 在 `NetworkManager` 类中添加 `private readonly object _lockObj = new object()`
- [x] 1.2 用 `lock(_lockObj)` 包裹 `ConnectCallback` 中对 `_isConnected`、`_socket` 的赋值
- [x] 1.3 用 `lock(_lockObj)` 包裹 `ReceiveCallback` 中对 `_socket` 的访问及 `BeginReceive` 调用
- [x] 1.4 用 `lock(_lockObj)` 包裹 `Disconnect` 方法体，确保断开时不与接收回调冲突

## 2. ResourceManager — 异步加载修复

- [x] 2.1 在 `LoadAsyncCoroutine` 中添加 AssetBundle 分支：解析 `bundleName/assetName`，发起 `AssetBundleRequest` 异步加载
- [x] 2.2 修复 AssetBundle 路径解析：改用 `IndexOf('/')` 只分割第一个斜杠，避免路径中含多个 `/` 时越界
- [x] 2.3 为 Addressable 模式添加警告日志并 fallback 到同步加载，结果通过 `callback` 返回

## 3. TimerManager — 精度和性能修复

- [x] 3.1 循环定时器触发后将 `ElapsedTime = 0` 改为 `ElapsedTime -= Duration`（余数保留）
- [x] 3.2 将 `Update` 中的即时移除（`_timers.Remove`）改为标记删除：先标记 `IsFinished = true`，帧末统一清理
- [x] 3.3 确认 `_timersToRemove` 列表在清理后被 `Clear()`，避免重复删除

## 4. ObjectPoolManager — 泛型池内存边界

- [x] 4.1 为 `ObjectPool<T>` 构造函数添加可选参数 `int maxSize = 0`，存储为 `_maxSize`
- [x] 4.2 在 `Release(T obj)` 方法中：若 `_maxSize > 0 && _pool.Count >= _maxSize`，则调用 `_onDestroy?.Invoke(obj)` 并返回，不入栈

## 5. UIManager — Stack 精确关闭修复

- [x] 5.1 将 `_uiStack` 类型从 `Stack<UIBase>` 改为 `List<UIBase>`
- [x] 5.2 `OpenUI` 中将 `_uiStack.Push(ui)` 改为 `_uiStack.Add(ui)`
- [x] 5.3 `CloseUI` 中将 `_uiStack.Peek()` 条件判断改为 `_uiStack.Remove(ui)`（精确删除）
- [x] 5.4 `CloseAllUI` 中将 `_uiStack.Clear()` 保持不变（已正确）

## 6. LogManager — 循环队列优化

- [x] 6.1 将 `_logCache` 从 `List<string>` 改为 `string[] _cacheBuffer` + `int _head`、`int _tail`、`int _count` 三个指针
- [x] 6.2 实现 `AddToCache(string log)`：写入 `_cacheBuffer[_tail]`，更新 `_tail = (_tail + 1) % _maxCacheSize`；若满则同步推进 `_head`
- [x] 6.3 实现 `GetCachedLogs()`：按时间顺序从 `_head` 到 `_tail` 遍历数组，返回有序列表
- [x] 6.4 删除原 `List<string>` 及 `RemoveAt(0)` 相关代码

## 7. AudioManager — 音效优先级修复

- [x] 7.1 为 `SFXSource` 内部记录结构添加 `int Priority` 字段（或使用并行 `int[]` 数组记录各 Source 当前优先级）
- [x] 7.2 修改 `PlaySFX(string name, int priority = 0)` 签名，保持向下兼容（默认 priority = 0）
- [x] 7.3 修改 `GetAvailableSFXSource`：若无空闲 Source，查找优先级最低的正在播放的 Source；若其优先级 < 新请求优先级则返回该 Source，否则返回 null（跳过）
- [x] 7.4 在 `PlaySFX` 中：若返回 Source 不为 null 则停止其当前音效，播放新音效并记录新优先级
