using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : Core.ModuleBase
    {
        public override int Priority => 4;

        private readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();

        public enum LoadMode
        {
            Resources,      // Resources加载
            AssetBundle,    // AssetBundle加载
            Addressable     // Addressable加载（待实现）
        }

        private LoadMode _currentLoadMode = LoadMode.Resources;

        /// <summary>
        /// 设置加载模式
        /// </summary>
        public void SetLoadMode(LoadMode mode)
        {
            _currentLoadMode = mode;
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T Load<T>(string path) where T : Object
        {
            if (_assetCache.TryGetValue(path, out Object cachedAsset))
            {
                return cachedAsset as T;
            }

            T asset = null;
            switch (_currentLoadMode)
            {
                case LoadMode.Resources:
                    asset = Resources.Load<T>(path);
                    break;
                case LoadMode.AssetBundle:
                    asset = LoadFromAssetBundle<T>(path);
                    break;
            }

            if (asset != null)
            {
                _assetCache[path] = asset;
            }

            return asset;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            if (_assetCache.TryGetValue(path, out Object cachedAsset))
            {
                onComplete?.Invoke(cachedAsset as T);
                return;
            }

            Core.GameEntry.Instance.StartCoroutine(LoadAsyncCoroutine(path, onComplete));
        }

        private IEnumerator LoadAsyncCoroutine<T>(string path, Action<T> onComplete) where T : Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;

            T asset = request.asset as T;
            if (asset != null)
            {
                _assetCache[path] = asset;
            }

            onComplete?.Invoke(asset);
        }

        /// <summary>
        /// 实例化GameObject
        /// </summary>
        public GameObject Instantiate(string path, Transform parent = null)
        {
            GameObject prefab = Load<GameObject>(path);
            if (prefab != null)
            {
                return Object.Instantiate(prefab, parent);
            }
            return null;
        }

        /// <summary>
        /// 异步实例化GameObject
        /// </summary>
        public void InstantiateAsync(string path, Action<GameObject> onComplete, Transform parent = null)
        {
            LoadAsync<GameObject>(path, (prefab) =>
            {
                GameObject instance = prefab != null ? Object.Instantiate(prefab, parent) : null;
                onComplete?.Invoke(instance);
            });
        }

        /// <summary>
        /// 从AssetBundle加载
        /// </summary>
        private T LoadFromAssetBundle<T>(string path) where T : Object
        {
            // 解析路径：bundleName/assetName
            string[] parts = path.Split('/');
            if (parts.Length < 2)
            {
                Debug.LogError($"Invalid AssetBundle path format: {path}. Expected format: bundleName/assetName");
                return null;
            }

            string bundleName = parts[0];
            string assetName = parts[1];

            AssetBundle bundle = LoadAssetBundle(bundleName);
            if (bundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle: {bundleName}");
                return null;
            }

            return bundle.LoadAsset<T>(assetName);
        }

        /// <summary>
        /// 异步从AssetBundle加载
        /// </summary>
        private IEnumerator LoadFromAssetBundleAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            string[] parts = path.Split('/');
            if (parts.Length < 2)
            {
                Debug.LogError($"Invalid AssetBundle path format: {path}");
                onComplete?.Invoke(null);
                yield break;
            }

            string bundleName = parts[0];
            string assetName = parts[1];

            AssetBundle bundle = LoadAssetBundle(bundleName);
            if (bundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle: {bundleName}");
                onComplete?.Invoke(null);
                yield break;
            }

            AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
            yield return request;

            onComplete?.Invoke(request.asset as T);
        }

        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        public AssetBundle LoadAssetBundle(string bundleName)
        {
            if (_loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                return bundle;
            }

            string path = System.IO.Path.Combine(Application.streamingAssetsPath, bundleName);
            bundle = AssetBundle.LoadFromFile(path);
            if (bundle != null)
            {
                _loadedBundles[bundleName] = bundle;
            }

            return bundle;
        }

        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        public void UnloadAssetBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (_loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                bundle.Unload(unloadAllLoadedObjects);
                _loadedBundles.Remove(bundleName);
            }
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload(string path)
        {
            if (_assetCache.TryGetValue(path, out Object asset))
            {
                if (asset != null && !(asset is GameObject))
                {
                    Resources.UnloadAsset(asset);
                }
                _assetCache.Remove(path);
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void ClearCache()
        {
            _assetCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        public override void OnDestroy()
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle?.Unload(true);
            }
            _loadedBundles.Clear();
            _assetCache.Clear();
        }
    }
}
