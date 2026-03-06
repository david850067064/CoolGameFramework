## Context

`DataManager.cs` 在正式 Save 模块引入之前，在文件末尾附带了一个基于 `PlayerPrefs` 的简易 `SaveManager` 静态类，作为临时存档工具。新的 `Modules/Save/SaveManager.cs` 实现了功能完整的模块化存档系统后，两个同名类在同一命名空间共存，Unity 编译器报 CS0101。

## Goals / Non-Goals

**Goals:**
- 从 `DataManager.cs` 中删除旧版 `SaveManager` 静态类，消除 CS0101 编译错误
- `DataManager` 的配置管理功能完全保留不变

**Non-Goals:**
- 不为旧版 PlayerPrefs API 提供向后兼容适配层
- 不修改 `Modules/Save/SaveManager.cs` 的任何逻辑

## Decisions

### 决策：直接删除，不做迁移适配

**选择**：直接从 `DataManager.cs` 中删除旧 `SaveManager` 类，不提供兼容层。

**替代方案**：保留旧类并改名（如 `LegacySaveManager`）或添加 `[Obsolete]` 标记。

**原因**：旧版 `SaveManager` 是框架内部的临时实现，README 示例已全部使用新的 `GameEntry.Save` API，不存在已知的外部调用方。添加兼容层会增加维护负担且意义不大。

## Risks / Trade-offs

- **[风险] 使用旧 API 的用户代码编译失败**：调用了 `SaveManager.SaveInt()`、`SaveManager.LoadString()` 等静态方法的业务代码会出现编译错误。→ 缓解：在 CHANGELOG 中说明迁移路径（改用 `GameEntry.Save.*`）。
- **[权衡] 这是 BREAKING CHANGE**：框架仍处于早期阶段，版本号应升至下一个 minor 版本以示区分。
