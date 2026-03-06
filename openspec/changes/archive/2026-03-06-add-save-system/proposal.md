## Why

CoolGameFramework 目前缺少统一的存档管理模块，业务层需要各自实现数据持久化逻辑，导致代码重复、存档格式不统一、加密/压缩等能力难以复用。现在框架已具备稳定的模块体系，是引入标准化存档系统的合适时机。

## What Changes

- 新增 `SaveManager` 模块，集成到框架模块体系，通过 `GameEntry.Save` 全局访问
- 支持多存档槽位（Slot），每个槽位独立管理一套存档数据
- 支持对象序列化（JSON）、可选 AES 加密、可选 GZIP 压缩
- 提供同步与异步两套 API
- 支持存档元数据（创建时间、游戏时长、版本号、自定义缩略图路径）
- 提供存档迁移机制，处理版本升级时的数据兼容

## Capabilities

### New Capabilities

- `save-manager`: 存档管理器核心模块，负责槽位管理、序列化、文件读写、加密压缩及存档元数据维护

### Modified Capabilities

（无现有 spec 需要修改）

## Impact

- **新增文件**：`Modules/Save/SaveManager.cs`、`Modules/Save/SaveSlot.cs`、`Modules/Save/SaveData.cs`、`Modules/Save/SaveConfig.cs`
- **修改文件**：`Core/GameEntry.cs`（添加 `Save` 属性）
- **依赖**：`Utilities/JsonUtil.cs`、`Utilities/EncryptUtil.cs`、`Utilities/FileUtil.cs`（已有）
- **无 Breaking Change**：纯新增模块，不影响现有接口
