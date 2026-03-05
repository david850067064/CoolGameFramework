using System;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// SDK管理器
    /// </summary>
    public class SDKManager : Core.ModuleBase
    {
        public override int Priority => 14;

        private ISDKInterface _currentSDK;

        public override void OnInit()
        {
            InitializeSDK();
        }

        /// <summary>
        /// 初始化SDK
        /// </summary>
        private void InitializeSDK()
        {
#if UNITY_ANDROID
            _currentSDK = new AndroidSDK();
#elif UNITY_IOS
            _currentSDK = new iOSSDK();
#else
            _currentSDK = new DefaultSDK();
#endif
            _currentSDK.Initialize();
        }

        /// <summary>
        /// 登录
        /// </summary>
        public void Login(Action<bool> onComplete)
        {
            _currentSDK?.Login(onComplete);
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            _currentSDK?.Logout();
        }

        /// <summary>
        /// 支付
        /// </summary>
        public void Pay(string productId, float amount, Action<bool> onComplete)
        {
            _currentSDK?.Pay(productId, amount, onComplete);
        }

        /// <summary>
        /// 分享
        /// </summary>
        public void Share(string content, Action<bool> onComplete)
        {
            _currentSDK?.Share(content, onComplete);
        }

        /// <summary>
        /// 显示广告
        /// </summary>
        public void ShowAd(Action<bool> onComplete)
        {
            _currentSDK?.ShowAd(onComplete);
        }

        public override void OnDestroy()
        {
            _currentSDK?.Dispose();
        }
    }

    /// <summary>
    /// SDK接口
    /// </summary>
    public interface ISDKInterface
    {
        void Initialize();
        void Login(Action<bool> onComplete);
        void Logout();
        void Pay(string productId, float amount, Action<bool> onComplete);
        void Share(string content, Action<bool> onComplete);
        void ShowAd(Action<bool> onComplete);
        void Dispose();
    }

    /// <summary>
    /// 默认SDK实现
    /// </summary>
    public class DefaultSDK : ISDKInterface
    {
        public void Initialize()
        {
            Debug.Log("DefaultSDK Initialized");
        }

        public void Login(Action<bool> onComplete)
        {
            Debug.Log("DefaultSDK Login");
            onComplete?.Invoke(true);
        }

        public void Logout()
        {
            Debug.Log("DefaultSDK Logout");
        }

        public void Pay(string productId, float amount, Action<bool> onComplete)
        {
            Debug.Log($"DefaultSDK Pay: {productId}, Amount: {amount}");
            onComplete?.Invoke(true);
        }

        public void Share(string content, Action<bool> onComplete)
        {
            Debug.Log($"DefaultSDK Share: {content}");
            onComplete?.Invoke(true);
        }

        public void ShowAd(Action<bool> onComplete)
        {
            Debug.Log("DefaultSDK ShowAd");
            onComplete?.Invoke(true);
        }

        public void Dispose()
        {
            Debug.Log("DefaultSDK Disposed");
        }
    }

    /// <summary>
    /// Android SDK实现
    /// </summary>
    public class AndroidSDK : DefaultSDK
    {
        public new void Initialize()
        {
            Debug.Log("AndroidSDK Initialized");
            // TODO: Android特定初始化
        }
    }

    /// <summary>
    /// iOS SDK实现
    /// </summary>
    public class iOSSDK : DefaultSDK
    {
        public new void Initialize()
        {
            Debug.Log("iOSSDK Initialized");
            // TODO: iOS特定初始化
        }
    }
}
