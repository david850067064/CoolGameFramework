## 1. 数据模型与配置

- [x] 1.1 创建 `SaveConfig.cs`：定义 `MaxSlots`、`EnableEncryption`、`EncryptionKey`、`EnableCompression`、`CurrentVersion` 等配置项
- [x] 1.2 创建 `SaveMeta.cs`：定义槽位元数据结构（`SlotIndex`、`CreateTime`、`LastSaveTime`、`PlayTime`、`DataVersion`）
- [x] 1.3 创建 `SaveSlot.cs`：封装单个槽位的文件路径计算逻辑（`save.dat` 和 `meta.json` 路径）

## 2. 核心存档模块

- [x] 2.1 创建 `SaveManager.cs`，继承 `ModuleBase`，Priority 设为 4（在 ResourceManager 之后）
- [x] 2.2 实现 `OnInit()`：读取 `SaveConfig`，初始化存档根目录（`Application.persistentDataPath/Saves/`）
- [x] 2.3 实现同步 `Save<T>(int slotIndex, T data)`：JSON 序列化 → 可选压缩 → 可选加密 → 原子写入（先写 `.tmp` 再 `File.Move`）
- [x] 2.4 实现同步 `Load<T>(int slotIndex)`：读取文件 → 可选解密 → 可选解压 → JSON 反序列化 → 返回对象（槽位不存在返回 null）
- [x] 2.5 实现 `DeleteSlot(int slotIndex)`：删除槽位目录下所有文件
- [x] 2.6 实现 `GetSlotMeta(int slotIndex)`：读取明文 `meta.json` 并返回 `SaveMeta`
- [x] 2.7 实现 `GetAllSlotMetas()`：遍历所有槽位，返回 `SaveMeta[]`（空槽位填 null）
- [x] 2.8 实现 `CanLoad(int slotIndex)`：检查文件存在性及版本兼容性，返回 bool

## 3. 异步读写

- [x] 3.1 实现 `Task SaveAsync<T>(int slotIndex, T data)`：在 `Task.Run` 中调用同步 Save 逻辑
- [x] 3.2 实现 `Task<T> LoadAsync<T>(int slotIndex)`：在 `Task.Run` 中调用同步 Load 逻辑
- [x] 3.3 实现 `OnDestroy()`：等待所有未完成的异步写入任务结束（维护 pending task 列表）

## 4. 版本迁移

- [x] 4.1 定义 `SaveVersionException`：携带 `fromVersion`、`toVersion` 信息
- [x] 4.2 实现 `RegisterMigration(int fromVersion, int toVersion, Func<string, string> migrateFunc)`：注册迁移回调字典
- [x] 4.3 在 `Load<T>` 中加入版本检测逻辑：读取元数据版本 → 与 `CurrentVersion` 比对 → 依序触发迁移回调 → 无回调时抛出 `SaveVersionException`

## 5. 元数据自动更新

- [x] 5.1 在 `Save<T>` 成功后自动写入 `meta.json`（更新 `LastSaveTime`；首次创建时设置 `CreateTime`）
- [x] 5.2 提供 `UpdatePlayTime(int slotIndex, float deltaSeconds)` 方法，供业务层每帧或定期累加游戏时长

## 6. 框架集成

- [x] 6.1 在 `GameEntry.cs` 添加 `public static SaveManager Save { get; private set; }`
- [x] 6.2 在 `GameEntry.InitializeFramework()` 中注册 `Save = ModuleManager.Instance.RegisterModule<SaveManager>()`（在 Resource 之后）

## 7. 验证与测试

- [x] 7.1 编写手动测试场景脚本 `SaveSystemTest.cs`：测试多槽位存取、加密开关、删除、元数据读取
- [x] 7.2 测试版本迁移路径：旧版本存档 → 注册迁移回调 → 成功加载并转换数据
- [x] 7.3 测试异常路径：密钥为空时启用加密、读取不存在槽位、版本不兼容无迁移回调
- [x] 7.4 测试原子写入：模拟写入中断场景，确认旧存档不损坏
