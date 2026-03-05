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
        private AndroidJavaObject _currentActivity;
        private AndroidJavaObject _unityPlayer;

        public new void Initialize()
        {
            Debug.Log("AndroidSDK Initialized");

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // 获取Unity Activity
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // 初始化Android特定SDK
                // 例如：初始化支付SDK、广告SDK等
                // InitializePaymentSDK();
                // InitializeAdSDK();

                Debug.Log("Android SDK components initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Android SDK: {e.Message}");
            }
#endif
        }

        public new void Pay(string productId, float amount, Action<bool> onComplete)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // 调用Android原生支付
                // _currentActivity.Call("startPayment", productId, amount);
                Debug.Log($"Android Pay: {productId}, Amount: {amount}");
                onComplete?.Invoke(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Android payment failed: {e.Message}");
                onComplete?.Invoke(false);
            }
#else
            base.Pay(productId, amount, onComplete);
#endif
        }

        public new void Share(string content, Action<bool> onComplete)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // 调用Android原生分享
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", content);

                AndroidJavaObject chooser = new AndroidJavaClass("android.content.Intent")
                    .CallStatic<AndroidJavaObject>("createChooser", intent, "Share");
                _currentActivity.Call("startActivity", chooser);

                Debug.Log($"Android Share: {content}");
                onComplete?.Invoke(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Android share failed: {e.Message}");
                onComplete?.Invoke(false);
            }
#else
            base.Share(content, onComplete);
#endif
        }
    }

    /// <summary>
    /// iOS SDK实现
    /// </summary>
    public class iOSSDK : DefaultSDK
    {
#if UNITY_IOS && !UNITY_EDITOR
        // iOS原生方法声明
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _InitializeIOSSDK();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _IOSPay(string productId, float amount);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _IOSShare(string content);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _IOSShowAd();
#endif

        public new void Initialize()
        {
            Debug.Log("iOSSDK Initialized");

#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                // 调用iOS原生初始化
                _InitializeIOSSDK();

                // 初始化iOS特定SDK
                // 例如：初始化StoreKit、GameCenter等
                // InitializeStoreKit();
                // InitializeGameCenter();

                Debug.Log("iOS SDK components initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize iOS SDK: {e.Message}");
            }
#endif
        }

        public new void Pay(string productId, float amount, Action<bool> onComplete)
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                // 调用iOS原生支付（StoreKit）
                _IOSPay(productId, amount);
                Debug.Log($"iOS Pay: {productId}, Amount: {amount}");
                onComplete?.Invoke(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"iOS payment failed: {e.Message}");
                onComplete?.Invoke(false);
            }
#else
            base.Pay(productId, amount, onComplete);
#endif
        }

        public new void Share(string content, Action<bool> onComplete)
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                // 调用iOS原生分享（UIActivityViewController）
                _IOSShare(content);
                Debug.Log($"iOS Share: {content}");
                onComplete?.Invoke(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"iOS share failed: {e.Message}");
                onComplete?.Invoke(false);
            }
#else
            base.Share(content, onComplete);
#endif
        }

        public new void ShowAd(Action<bool> onComplete)
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                // 调用iOS原生广告
                _IOSShowAd();
                Debug.Log("iOS ShowAd");
                onComplete?.Invoke(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"iOS show ad failed: {e.Message}");
                onComplete?.Invoke(false);
            }
#else
            base.ShowAd(onComplete);
#endif
        }
    }
}
