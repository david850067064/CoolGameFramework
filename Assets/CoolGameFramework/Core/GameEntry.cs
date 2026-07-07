using UnityEngine;
using CoolGameFramework.Modules;

namespace CoolGameFramework.Core
{
    /// <summary>
    /// 游戏框架入口
    /// </summary>
    public class GameEntry : MonoSingleton<GameEntry>
    {
        [Header("框架设置")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        // 模块快捷访问
        public static EventManager Event { get; private set; }
        public static ResourceManager Resource { get; private set; }
        public static ObjectPoolManager ObjectPool { get; private set; }
        public static UIManager UI { get; private set; }
        public static SceneManager Scene { get; private set; }
        public static AudioManager Audio { get; private set; }
        public static DataManager Data { get; private set; }
        public static NetworkManager Network { get; private set; }
        public static InputManager Input { get; private set; }
        public static TimerManager Timer { get; private set; }
        public static ProcedureManager Procedure { get; private set; }
        public static FSMManager FSM { get; private set; }
        public static LocalizationManager Localization { get; private set; }
        public static SDKManager SDK { get; private set; }
        public static DownloadManager Download { get; private set; }
        public static LogManager Log { get; private set; }
        public static HotUpdateManager HotUpdate { get; private set; }
        public static TweenManager Tween { get; private set; }
        public static SaveManager Save { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeFramework();
        }

        private void InitializeFramework()
        {
            Debug.Log("=== CoolGameFramework Initializing ===");

            // 按优先级注册模块
            Log = ModuleManager.Instance.RegisterModule<LogManager>();
            Event = ModuleManager.Instance.RegisterModule<EventManager>();
            Timer = ModuleManager.Instance.RegisterModule<TimerManager>();
            Input = ModuleManager.Instance.RegisterModule<InputManager>();
            Resource = ModuleManager.Instance.RegisterModule<ResourceManager>();
            Save = ModuleManager.Instance.RegisterModule<SaveManager>();
            ObjectPool = ModuleManager.Instance.RegisterModule<ObjectPoolManager>();
            Data = ModuleManager.Instance.RegisterModule<DataManager>();
            Audio = ModuleManager.Instance.RegisterModule<AudioManager>();
            Scene = ModuleManager.Instance.RegisterModule<SceneManager>();
            UI = ModuleManager.Instance.RegisterModule<UIManager>();
            Network = ModuleManager.Instance.RegisterModule<NetworkManager>();
            FSM = ModuleManager.Instance.RegisterModule<FSMManager>();
            Procedure = ModuleManager.Instance.RegisterModule<ProcedureManager>();
            Localization = ModuleManager.Instance.RegisterModule<LocalizationManager>();
            SDK = ModuleManager.Instance.RegisterModule<SDKManager>();
            Download = ModuleManager.Instance.RegisterModule<DownloadManager>();
            Tween = ModuleManager.Instance.RegisterModule<TweenManager>();
            HotUpdate = ModuleManager.Instance.RegisterModule<HotUpdateManager>();

            Debug.Log("=== CoolGameFramework Initialized Successfully ===");
        }

        private void Update()
        {
            ModuleManager.Instance.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            ModuleManager.Instance.FixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            ModuleManager.Instance.Dispose();
        }

        private void OnApplicationQuit()
        {
            ModuleManager.Instance.Dispose();
        }
    }
}
