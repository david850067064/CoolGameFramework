# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2024-03-06

### Added
- Initial release of CoolGameFramework
- Core module system with lifecycle management
- 16 core modules:
  - EventManager - Type-safe event system
  - ResourceManager - Resource loading (Resources/AssetBundle)
  - ObjectPoolManager - GameObject and object pooling
  - UIManager - Multi-layer UI management
  - SceneManager - Scene loading and management
  - AudioManager - Music and sound effects
  - DataManager - Configuration data management
  - NetworkManager - TCP/HTTP networking
  - InputManager - Unified input handling
  - TimerManager - Timer and delay execution
  - ProcedureManager - Game flow management
  - FSMManager - Finite state machine
  - LocalizationManager - Multi-language support
  - SDKManager - Third-party SDK integration
  - DownloadManager - File download with queue
  - TweenManager - Animation tweening system
  - HotUpdateManager - Hot update support (HybridCLR ready)
- 6 utility classes:
  - PathUtil - Path operations
  - FileUtil - File I/O operations
  - JsonUtil - JSON serialization
  - EncryptUtil - Encryption (MD5/AES/Base64)
  - TimeUtil - Time utilities
  - MathUtil - Math utilities
- Editor tools:
  - AssetBundle Builder - Build and manage AssetBundles
  - UI Binder - Automatic UI component binding
  - Scene Switcher - Quick scene switching
  - Resource Checker - Find missing scripts and unused assets
  - Framework Settings - Configure framework settings
- Complete documentation and examples
- MIT License

### Features
- Modular architecture for easy extension
- Singleton pattern support (Singleton & MonoSingleton)
- Priority-based module initialization
- Automatic lifecycle management
- Type-safe event system
- Flexible resource loading modes
- Multi-layer UI system
- Object pooling for performance
- Tween animation with easing functions
- Hot update infrastructure
- Cross-platform support

### Documentation
- Comprehensive README with usage examples
- Code comments in Chinese and English
- Example scripts demonstrating all features
