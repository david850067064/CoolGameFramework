# Cool Game Framework

A complete and extensible Unity game development framework.

## Installation

### Install via Git URL

1. Open Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/david850067064/CoolGameFramework.git?path=/Assets/CoolGameFramework`
5. Click "Add"

### Manual Installation

1. Download the repository
2. Copy the `Assets/CoolGameFramework` folder to your Unity project's `Packages` folder

## Quick Start

1. Create an empty GameObject in your scene
2. Add the `GameEntry` component to it
3. The framework will automatically initialize all modules

```csharp
using CoolGameFramework.Core;

// Access modules
GameEntry.Event.Subscribe<YourEvent>(OnYourEvent);
GameEntry.UI.OpenUI<YourUI>("YourUIName");
GameEntry.Audio.PlayMusic("BackgroundMusic");
```

## Features

- 17 Core Modules (Event, Resource, UI, Audio, Network, etc.)
- Object Pooling System
- Tween Animation System
- Hot Update Support (HybridCLR ready)
- 6 Utility Classes
- 5 Editor Tools
- Complete Documentation

## Documentation

For full documentation, visit: https://github.com/david850067064/CoolGameFramework

## License

MIT License - see LICENSE file for details
