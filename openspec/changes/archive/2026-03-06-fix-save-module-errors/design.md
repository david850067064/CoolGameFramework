## Context

Unity 要求 Assets/ 和 Packages/ 目录下的每个文件与文件夹都有一个对应的 `.meta` 文件。`.meta` 文件存储 Unity 为该资产分配的 GUID，编辑器用 GUID 追踪资产引用。

Save 模块（`Modules/Save/`）和示例文件（`Examples/SaveSystemTest.cs`）在 git 提交时遗漏了 `.meta` 文件。当用户通过 git URL 安装包时，Unity 在 `Library/PackageCache/` 中缓存包内容，发现这些文件没有 `.meta` 后输出警告并将其标记为"immutable folder 中无 meta"，导致文件被排除在编译之外。

此后 `GameEntry.cs` 编译时找不到 `SaveManager` 类型（或类型无法满足 `IModule` 约束），产生 `CS0311` 错误，进而级联导致 Burst 编译器找不到 `CoolGameFramework.Editor` 程序集。

`generate_meta_files.py` 脚本已存在于项目根目录，但其路径被硬编码为开发者本机路径，无法直接复用。

## Goals / Non-Goals

**Goals:**
- 为所有缺失 `.meta` 的文件和文件夹生成合法的 Unity `.meta` 文件并提交
- 修正 `generate_meta_files.py` 的路径问题，使其可在任意检出路径运行
- 消除 `CS0311` 编译错误及级联的 Burst 错误

**Non-Goals:**
- 不修改 SaveManager 的任何业务逻辑
- 不调整框架的整体架构或其他模块
- 不解决 `packages.unity.com` 网络连接问题（属于用户网络/防火墙配置，与本框架无关）

## Decisions

### 决策 1：手动生成 `.meta` 文件后提交，而非依赖 Unity 编辑器自动生成

**选择**：直接运行 `generate_meta_files.py`（修正路径后）生成 `.meta` 文件，然后 git commit。

**替代方案**：依赖 Unity 编辑器在导入包时自动生成 `.meta`。

**原因**：Unity 对 `Library/PackageCache/`（immutable 文件夹）中的文件不会自动生成 `.meta`，必须由包的 git 仓库提供。手动生成并提交是 Unity 官方推荐的 git 包发布方式。

### 决策 2：修正 `generate_meta_files.py` 为相对路径

**选择**：将脚本中的 `framework_path` 改为相对于脚本文件自身位置（`os.path.dirname(__file__)`）自动推导，指向 `Assets/CoolGameFramework`。

**替代方案**：通过命令行参数传入路径。

**原因**：相对路径方案对所有贡献者零配置，降低再次遗漏的概率；同时保持命令简单（直接 `python generate_meta_files.py`）。

## Risks / Trade-offs

- **[风险] GUID 冲突**：新生成的 GUID 是随机的，与已有 GUID 不会冲突。→ 缓解：Python `uuid.uuid4().hex` 确保全局唯一。
- **[风险] 已缓存旧版本的用户**：已安装包的用户需要在 Unity 中 "Remove" 后重新 "Add" 包，或清除 `Library/PackageCache/` 后重启编辑器。→ 缓解：在 CHANGELOG 中注明。
- **[权衡] 不修改 SaveManager 代码**：`CS0311` 错误是 meta 缺失引起的编译失败的结果，SaveManager 本身继承自 `ModuleBase`（实现了 `IModule`）逻辑正确，无需修改代码。
