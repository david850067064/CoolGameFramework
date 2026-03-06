## Why

`DataManager.cs` 包含一个旧版 `SaveManager` 静态工具类（基于 `PlayerPrefs`），它是在正式 Save 模块开发之前的临时占位实现。新增的 `Modules/Save/SaveManager.cs` 提供了功能完整的模块化存档系统后，两者在同一命名空间 `CoolGameFramework.Modules` 中同时存在，引发 `CS0101` 重复定义编译错误，阻止框架正常编译。

## What Changes

- 从 `Modules/Data/DataManager.cs` 中删除旧版 `SaveManager` 静态类（第 72-179 行）
- 保留 `DataManager` 类本身不变
- `Modules/Save/SaveManager.cs` 继续作为唯一的 `SaveManager` 实现

## Capabilities

### New Capabilities

（无新功能，仅删除冗余代码）

### Modified Capabilities

- `save-manager`: 移除基于 `PlayerPrefs` 的旧版静态 `SaveManager`，消除与模块化 `SaveManager` 的命名冲突

## Impact

- `Assets/CoolGameFramework/Modules/Data/DataManager.cs`：删除旧 `SaveManager` 类（约 107 行）
- 使用旧版 `SaveManager` 静态 API（`SaveManager.SaveInt()`、`SaveManager.LoadString()` 等）的业务代码需迁移至 `GameEntry.Save`（**BREAKING**）
