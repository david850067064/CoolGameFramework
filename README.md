# CoolGameFramework

一个功能完善、易于扩展的 Unity 游戏开发框架。

## 安装

### 方法1：通过 Unity Package Manager 安装（推荐）

1. 打开 Unity 编辑器
2. 打开 Package Manager（Window > Package Manager）
3. 点击左上角的 "+" 按钮
4. 选择 "Add package from git URL..."
5. 输入以下 URL：
   ```
   https://github.com/david850067064/CoolGameFramework.git?path=/Assets/CoolGameFramework
   ```
6. 点击 "Add" 按钮

### 方法2：通过 manifest.json 安装

1. 打开你的 Unity 项目的 `Packages/manifest.json` 文件
2. 在 `dependencies` 中添加：
   ```json
   {
     "dependencies": {
       "com.coolgameframework.core": "https://github.com/david850067064/CoolGameFramework.git?path=/Assets/CoolGameFramework",
       ...
     }
   }
   ```
3. 保存文件，Unity 会自动安装

### 方法3：手动安装

1. 下载或克隆本仓库
2. 将 `Assets/CoolGameFramework` 文件夹复制到你的项目的 `Assets` 或 `Packages` 目录

## 特性

- **模块化设计** - 所有功能模块独立，易于扩展和维护
- **完整的管理器系统** - 包含游戏开发所需的所有核心管理器
- **热更新支持** - 支持 HybridCLR 热更新方案
- **资源管理** - 支持 Resources、AssetBundle 等多种加载方式
- **UI管理** - 完善的UI层级管理和生命周期控制
- **事件系统** - 类型安全的事件订阅/发布机制
- **网络通信** - 支持 TCP/HTTP 等多种网络协议
- **本地化支持** - 多语言切换系统
- **对象池** - 高效的对象复用机制
- **状态机** - 通用FSM和流程管理
- **丰富的工具类** - 文件、加密、时间、数学等实用工具

## 框架结构

```
CoolGameFramework/
├── Core/                   # 核心层
│   ├── Base/              # 基础类
│   │   ├── Singleton.cs
│   │   ├── MonoSingleton.cs
│   │   └── ModuleBase.cs
│   ├── Interface/         # 接口定义
│   │   └── IModule.cs
│   ├── ModuleManager.cs   # 模块管理器
│   └── GameEntry.cs       # 框架入口
├── Modules/               # 模块层
│   ├── Event/            # 事件系统
│   ├── Resource/         # 资源管理
│   ├── ObjectPool/       # 对象池
│   ├── UI/               # UI管理
│   ├── Scene/            # 场景管理
│   ├── Audio/            # 音频管理
│   ├── Data/             # 数据管理
│   ├── Network/          # 网络管理
│   ├── Input/            # 输入管理
│   ├── Timer/            # 定时器
│   ├── Procedure/        # 流程管理
│   ├── FSM/              # 状态机
│   ├── Localization/     # 本地化
│   ├── SDK/              # SDK集成
│   ├── Download/         # 下载管理
│   ├── Tween/            # 动画系统
│   ├── Log/              # 日志系统
│   └── HotUpdate/        # 热更新
├── Utilities/            # 工具层
│   ├── PathUtil.cs
│   ├── FileUtil.cs
│   ├── JsonUtil.cs
│   ├── EncryptUtil.cs
│   ├── TimeUtil.cs
│   └── MathUtil.cs
├── Editor/               # 编辑器工具
│   ├── AssetBundle/      # AB包工具
│   ├── UI/               # UI工具
│   ├── Scene/            # 场景工具
│   ├── Tools/            # 其他工具
│   └── Settings/         # 设置窗口
└── Examples/             # 示例代码
```

## 快速开始

### 1. 安装框架

按照上述安装方法之一安装框架到你的 Unity 项目。

### 2. 初始化框架

在场景中创建一个空物体，添加 `GameEntry` 组件，框架会自动初始化所有模块。

```csharp
// 框架会自动初始化，你也可以通过代码访问各个模块
using CoolGameFramework.Core;

// 访问模块
GameEntry.Event.Subscribe<YourEvent>(OnYourEvent);
GameEntry.UI.OpenUI<YourUI>("YourUIName");
GameEntry.Audio.PlayMusic("BackgroundMusic");
```

### 3. 使用示例

#### 事件系统
```csharp
// 定义事件
public struct PlayerDiedEvent
{
    public int PlayerId;
    public string Reason;
}

// 订阅事件
GameEntry.Event.Subscribe<PlayerDiedEvent>(OnPlayerDied);

// 发布事件
GameEntry.Event.Publish(new PlayerDiedEvent
{
    PlayerId = 1,
    Reason = "Fall"
});

// 取消订阅
GameEntry.Event.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
```

#### UI管理
```csharp
// 打开UI
var mainUI = GameEntry.UI.OpenUI<MainUI>("MainUI", UILayer.Normal);

// 关闭UI
GameEntry.UI.CloseUI("MainUI");

// 获取UI
var ui = GameEntry.UI.GetUI<MainUI>("MainUI");
```

#### 资源加载
```csharp
// 同步加载
var prefab = GameEntry.Resource.Load<GameObject>("Prefabs/Player");

// 异步加载
GameEntry.Resource.LoadAsync<GameObject>("Prefabs/Enemy", (asset) =>
{
    if (asset != null)
    {
        Instantiate(asset);
    }
});

// 实例化
var instance = GameEntry.Resource.Instantiate("Prefabs/Bullet");
```

#### 音频播放
```csharp
// 播放背景音乐
GameEntry.Audio.PlayMusic("BGM_Main");

// 播放音效
GameEntry.Audio.PlaySFX("SFX_Shoot");

// 设置音量
GameEntry.Audio.SetMusicVolume(0.8f);
GameEntry.Audio.SetSFXVolume(0.6f);
```

#### 场景管理
```csharp
// 同步加载场景
GameEntry.Scene.LoadScene("GameScene");

// 异步加载场景
GameEntry.Scene.LoadSceneAsync("GameScene",
    progress => Debug.Log($"Loading: {progress * 100}%"),
    () => Debug.Log("Scene loaded!"));
```

#### 定时器
```csharp
// 延迟执行
GameEntry.Timer.DelayCall(2f, () => Debug.Log("2 seconds later"));

// 循环执行
var timer = GameEntry.Timer.LoopCall(1f, () => Debug.Log("Every second"));

// 停止定时器
timer.Stop();
```

#### Tween动画
```csharp
// 移动动画
GameEntry.Tween.MoveTo(transform, new Vector3(5, 0, 0), 2f)
    .SetEase(TweenBase.EaseInOutQuad)
    .OnComplete(() => Debug.Log("Move completed!"));

// 缩放动画
GameEntry.Tween.ScaleTo(transform, Vector3.one * 2f, 1f);

// 旋转动画
GameEntry.Tween.RotateTo(transform, new Vector3(0, 180, 0), 1.5f);

// 透明度动画
GameEntry.Tween.FadeTo(canvasGroup, 0f, 1f);

// 自定义数值动画
GameEntry.Tween.ValueTo(0f, 100f, 2f, (value) =>
{
    Debug.Log($"Current value: {value}");
});
```

#### 对象池
```csharp
// 创建GameObject对象池
GameEntry.ObjectPool.CreateGameObjectPool("Bullet", bulletPrefab, 10, 100);

// 从对象池获取
GameObject bullet = GameEntry.ObjectPool.Spawn("Bullet", position, rotation);

// 回收到对象池
GameEntry.ObjectPool.Despawn("Bullet", bullet);

// 创建普通对象池
var pool = GameEntry.ObjectPool.CreateObjectPool<MyClass>(
    () => new MyClass(),
    obj => obj.OnGet(),
    obj => obj.OnRelease(),
    10
);

// 使用普通对象池
MyClass obj = pool.Get();
pool.Release(obj);
```

#### 定时器
```csharp
// 延迟执行
GameEntry.Timer.DelayCall(2f, () => Debug.Log("2 seconds later"));

// 循环执行
var timer = GameEntry.Timer.LoopCall(1f, () => Debug.Log("Every second"));

// 停止定时器
timer.Stop();
```

#### 网络请求
```csharp
// HTTP GET
HttpManager.Get("https://api.example.com/data",
    response => Debug.Log(response),
    error => Debug.LogError(error));

// HTTP POST
HttpManager.Post("https://api.example.com/login",
    "{\"username\":\"test\",\"password\":\"123456\"}",
    response => Debug.Log(response),
    error => Debug.LogError(error));
```

#### 数据存储
```csharp
// 保存数据
SaveManager.SaveInt("PlayerLevel", 10);
SaveManager.SaveString("PlayerName", "Hero");

// 加载数据
int level = SaveManager.LoadInt("PlayerLevel", 1);
string name = SaveManager.LoadString("PlayerName", "Unknown");

// 保存对象
SaveManager.Save("PlayerData", playerData);
var data = SaveManager.Load<PlayerData>("PlayerData");
```

#### 本地化
```csharp
// 获取本地化文本
string text = GameEntry.Localization.GetText("UI_Start");

// 切换语言
GameEntry.Localization.ChangeLanguage(SystemLanguage.Chinese);
```

## 模块说明

### 核心模块

- **ModuleManager** - 管理所有模块的生命周期
- **GameEntry** - 框架入口，提供全局访问点

### 功能模块

- **EventManager** - 事件系统，支持类型安全的事件订阅/发布
- **ResourceManager** - 资源管理，支持多种加载方式
- **ObjectPoolManager** - 对象池管理，支持GameObject和普通对象池
- **UIManager** - UI管理，支持多层级UI系统
- **SceneManager** - 场景管理，支持同步/异步加载
- **AudioManager** - 音频管理，支持音乐和音效
- **DataManager** - 数据管理，配置表管理
- **NetworkManager** - 网络管理，TCP/HTTP通信
- **InputManager** - 输入管理，统一输入接口
- **TimerManager** - 定时器管理
- **ProcedureManager** - 流程管理
- **FSMManager** - 状态机管理
- **LocalizationManager** - 本地化管理
- **SDKManager** - SDK集成管理
- **DownloadManager** - 下载管理
- **TweenManager** - 动画补间系统
- **LogManager** - 日志管理
- **HotUpdateManager** - 热更新管理

## 工具类

- **PathUtil** - 路径工具
- **FileUtil** - 文件操作
- **JsonUtil** - JSON序列化
- **EncryptUtil** - 加密解密
- **TimeUtil** - 时间工具
- **MathUtil** - 数学工具

## 编辑器工具

- **AssetBundle Builder** - AssetBundle打包工具
- **UI Binder** - UI组件自动绑定工具
- **Scene Switcher** - 场景快速切换工具
- **Resource Checker** - 资源检查工具（查找丢失脚本、未使用资源）
- **Framework Settings** - 框架设置窗口

### 使用编辑器工具

在Unity编辑器菜单栏中找到 `CoolGameFramework` 菜单：

- `CoolGameFramework/Framework Settings` - 打开框架设置
- `CoolGameFramework/AssetBundle/Build Window` - 打开AB包打包窗口
- `CoolGameFramework/Scene Switcher` - 打开场景切换工具
- `CoolGameFramework/Resource Checker` - 打开资源检查工具
- `GameObject/CoolGameFramework/Add UI Binder` - 为选中的GameObject添加UI绑定器

## 系统要求

- Unity 2020.3 或更高版本
- .NET Standard 2.1

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！

## 联系方式

如有问题或建议，请提交 Issue。
