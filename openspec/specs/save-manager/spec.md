### Requirement: 存档槽位管理
系统 SHALL 支持多个独立存档槽位，每个槽位拥有独立的数据文件和元数据文件，槽位数量上限由 `SaveConfig.MaxSlots` 配置。

#### Scenario: 创建新存档
- **WHEN** 调用 `SaveManager.Save(slotIndex, data)`
- **THEN** 系统在对应槽位目录写入序列化后的数据文件和元数据文件

#### Scenario: 覆盖已有存档
- **WHEN** 对已存在的槽位再次调用 `Save(slotIndex, data)`
- **THEN** 系统原子性地覆盖旧文件（先写临时文件再替换），旧数据不残留

#### Scenario: 删除存档
- **WHEN** 调用 `SaveManager.DeleteSlot(slotIndex)`
- **THEN** 系统删除该槽位目录下所有文件，槽位变为空状态

#### Scenario: 查询所有槽位
- **WHEN** 调用 `SaveManager.GetAllSlotMetas()`
- **THEN** 系统返回所有槽位的元数据列表（空槽位返回 null 占位）

---

### Requirement: 存档读取
系统 SHALL 支持从指定槽位加载存档数据，并反序列化为业务层指定的类型。

#### Scenario: 读取已有存档
- **WHEN** 调用 `SaveManager.Load<T>(slotIndex)`
- **THEN** 系统读取该槽位数据文件并返回反序列化后的 `T` 对象

#### Scenario: 读取不存在的槽位
- **WHEN** 调用 `SaveManager.Load<T>(slotIndex)` 且槽位为空
- **THEN** 系统返回 `null`，不抛出异常

#### Scenario: 预检查存档可读性
- **WHEN** 调用 `SaveManager.CanLoad(slotIndex)`
- **THEN** 系统返回 `bool`，指示该槽位是否存在且版本兼容

---

### Requirement: 异步读写支持
系统 SHALL 提供异步 API，避免大型存档的 IO 操作阻塞主线程。

#### Scenario: 异步保存
- **WHEN** 调用 `await SaveManager.SaveAsync(slotIndex, data)`
- **THEN** 系统在后台线程完成文件写入，完成后在主线程回调（或返回 Task）

#### Scenario: 异步加载
- **WHEN** 调用 `await SaveManager.LoadAsync<T>(slotIndex)`
- **THEN** 系统在后台线程完成文件读取和反序列化，返回结果

---

### Requirement: 加密与压缩
系统 SHALL 支持可选的 AES 加密和 GZIP 压缩，配置通过 `SaveConfig` 注入。

#### Scenario: 启用加密保存
- **WHEN** `SaveConfig.EnableEncryption = true` 且提供有效密钥，调用 `Save()`
- **THEN** 写入的数据文件为 AES 加密内容，明文不可读

#### Scenario: 未配置密钥时启用加密
- **WHEN** `SaveConfig.EnableEncryption = true` 但 `SaveConfig.EncryptionKey` 为空
- **THEN** 系统抛出 `InvalidOperationException` 并提供明确错误信息

#### Scenario: 启用压缩保存
- **WHEN** `SaveConfig.EnableCompression = true`，调用 `Save()`
- **THEN** 写入前对 JSON 数据进行 GZIP 压缩，文件体积减小

---

### Requirement: 存档元数据
系统 SHALL 为每个存档槽位维护元数据，包含创建时间、最后保存时间、游戏时长、数据版本号。

#### Scenario: 自动更新元数据
- **WHEN** 调用 `Save(slotIndex, data)` 成功
- **THEN** 对应槽位的 `meta.json` 自动更新 `LastSaveTime` 和 `PlayTime`

#### Scenario: 读取元数据不需要解密
- **WHEN** 调用 `SaveManager.GetSlotMeta(slotIndex)`
- **THEN** 系统读取明文 `meta.json` 并返回，无需解密主数据文件

---

### Requirement: 版本迁移钩子
系统 SHALL 在加载存档时检测版本号差异，并调用业务层注册的迁移回调。

#### Scenario: 注册迁移回调
- **WHEN** 调用 `SaveManager.RegisterMigration(fromVersion, toVersion, migrateFunc)`
- **THEN** 系统记录该迁移路径

#### Scenario: 加载旧版本存档触发迁移
- **WHEN** 存档数据版本号低于当前版本，调用 `Load<T>(slotIndex)`
- **THEN** 系统依次触发已注册的迁移回调，迁移完成后返回最新格式数据

#### Scenario: 无迁移回调时加载旧版本存档
- **WHEN** 存档版本不兼容且未注册对应迁移回调，调用 `Load<T>(slotIndex)`
- **THEN** 系统抛出 `SaveVersionException` 并包含版本信息

---

### Requirement: 框架集成
系统 SHALL 将 `SaveManager` 注册为标准框架模块，通过 `GameEntry.Save` 全局访问。

#### Scenario: 框架启动时初始化存档模块
- **WHEN** `GameEntry.Awake()` 执行
- **THEN** `SaveManager` 按优先级自动完成初始化，`GameEntry.Save` 可用

#### Scenario: 框架关闭时清理存档模块
- **WHEN** `GameEntry.OnDestroy()` 执行
- **THEN** `SaveManager.OnDestroy()` 被调用，确保未完成的异步写入正常结束

---

### Requirement: DataManager 职责范围
DataManager SHALL 仅负责运行时配置表（Config）数据的加载、缓存与查询，不包含任何持久化存档功能。

#### Scenario: 加载配置表
- **WHEN** 调用 `DataManager.LoadConfig<T>(configName)`
- **THEN** 系统从 Resources 加载对应 JSON 配置并缓存，返回反序列化对象

#### Scenario: 查询已缓存配置
- **WHEN** 调用 `DataManager.GetConfig<T>(configName)` 且配置已加载
- **THEN** 系统返回缓存对象，不重复 IO 操作
