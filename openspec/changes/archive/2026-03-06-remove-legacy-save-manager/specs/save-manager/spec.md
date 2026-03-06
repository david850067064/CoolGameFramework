## REMOVED Requirements

### Requirement: 基于 PlayerPrefs 的静态存档工具
**Reason**: 已被 `Modules/Save/SaveManager.cs` 中功能完整的模块化存档系统取代；两者同名导致 CS0101 编译错误。
**Migration**: 将所有 `SaveManager.Save<T>(key, data)`、`SaveManager.Load<T>(key)`、`SaveManager.SaveInt()`、`SaveManager.LoadString()` 等静态调用替换为通过 `GameEntry.Save` 访问的模块化 API：
- `SaveManager.SaveInt("key", val)` → `GameEntry.Save.Save(slotIndex, data)`
- `SaveManager.LoadInt("key", 0)` → `GameEntry.Save.Load<T>(slotIndex)`

## MODIFIED Requirements

### Requirement: DataManager 职责范围
DataManager SHALL 仅负责运行时配置表（Config）数据的加载、缓存与查询，不包含任何持久化存档功能。

#### Scenario: 加载配置表
- **WHEN** 调用 `DataManager.LoadConfig<T>(configName)`
- **THEN** 系统从 Resources 加载对应 JSON 配置并缓存，返回反序列化对象

#### Scenario: 查询已缓存配置
- **WHEN** 调用 `DataManager.GetConfig<T>(configName)` 且配置已加载
- **THEN** 系统返回缓存对象，不重复 IO 操作
