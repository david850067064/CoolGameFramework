## Why

The Save 模块（`Modules/Save/`）及示例文件（`Examples/SaveSystemTest.cs`）在提交到 git 仓库时缺少了对应的 `.meta` 文件。Unity 依赖 `.meta` 文件为每个资产分配稳定的 GUID；缺少 `.meta` 会导致 Unity 忽略这些文件，进而引发编译错误，使通过 Package Manager（git URL）安装该框架的用户无法正常使用。

## What Changes

- 为 `Modules/Save/` 目录及其 4 个 C# 文件生成并提交缺失的 `.meta` 文件
- 为 `Examples/` 目录及 `Examples/SaveSystemTest.cs` 生成并提交缺失的 `.meta` 文件
- 修正 `generate_meta_files.py` 脚本中硬编码的路径，使其可复用
- 修复由于 `SaveManager.cs` 被忽略而导致的 `CS0311` 编译错误（`SaveManager` 无法满足 `IModule` 约束）

## Capabilities

### New Capabilities

- `save-module-meta`: 为 Save 模块及示例文件补充完整的 `.meta` 文件，确保 Unity 正确识别并编译这些资产

### Modified Capabilities

- `save-manager`: Save 模块的行为不变，但修复其 `.meta` 缺失问题，使其可被正常编译和注册到 ModuleManager

## Impact

- `Assets/CoolGameFramework/Modules/Save/`（新增 5 个 `.meta` 文件：文件夹 + 4 个 `.cs`）
- `Assets/CoolGameFramework/Examples/`（新增 2 个 `.meta` 文件：文件夹 + `SaveSystemTest.cs`）
- `generate_meta_files.py`（修正硬编码路径为相对路径，提升可维护性）
- 下游影响：修复后 `GameEntry.cs:57` 的 `CS0311` 编译错误将自动消失，Burst 编译器找不到 `CoolGameFramework.Editor` 的级联错误也随之消除
