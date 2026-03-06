## ADDED Requirements

### Requirement: Save 模块 meta 文件完整性
框架 git 仓库 SHALL 为 `Modules/Save/` 目录及其所有 C# 文件，以及 `Examples/SaveSystemTest.cs` 及其所在目录提供合法的 Unity `.meta` 文件，使通过 git URL 安装的用户能够正常编译框架。

#### Scenario: Save 模块文件均有 meta
- **WHEN** 用户通过 Package Manager git URL 安装框架
- **THEN** Unity 不产生"has no meta file"警告，Save 模块所有 `.cs` 文件被正常编译

#### Scenario: 框架无编译错误
- **WHEN** 用户安装框架后打开 Unity 编辑器
- **THEN** Console 中不出现 `CS0311` 或与 `SaveManager` 相关的编译错误

#### Scenario: generate_meta_files.py 可在任意路径执行
- **WHEN** 开发者在任意工作目录执行 `python generate_meta_files.py`
- **THEN** 脚本自动定位到 `Assets/CoolGameFramework` 目录并为所有缺失 meta 的文件生成 `.meta`，不需要修改脚本内的硬编码路径
