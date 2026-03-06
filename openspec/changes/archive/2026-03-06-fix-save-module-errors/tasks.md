## 1. 修正 generate_meta_files.py

- [x] 1.1 将脚本中 `framework_path` 的硬编码路径替换为相对于脚本自身位置的动态路径（`os.path.dirname(os.path.abspath(__file__))`）

## 2. 生成缺失的 .meta 文件

- [x] 2.1 在项目根目录执行 `python generate_meta_files.py`，为 `Modules/Save/`（文件夹 meta + 4 个 .cs meta）和 `Examples/`（文件夹 meta + SaveSystemTest.cs meta）生成 `.meta` 文件
- [x] 2.2 确认生成的 `.meta` 文件格式正确（fileFormatVersion: 2，MonoImporter for .cs，folderAsset: yes for 文件夹）

## 3. 提交到 git

- [x] 3.1 将所有新生成的 `.meta` 文件（共 7 个）及修改后的 `generate_meta_files.py` 添加到 git staging
- [x] 3.2 提交 commit，消息说明修复缺失 meta 文件的问题

## 4. 验证

- [x] 4.1 在一个新的 Unity 项目中通过 git URL 重新安装框架，确认 Console 中无 "has no meta file" 警告
- [x] 4.2 确认 Console 中无 `CS0311`（SaveManager 无法转换为 IModule）编译错误
- [x] 4.3 确认 Console 中无 Burst 找不到 `CoolGameFramework.Editor` 的级联错误
