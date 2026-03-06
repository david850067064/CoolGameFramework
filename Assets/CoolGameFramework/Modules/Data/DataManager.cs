using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 数据管理器
    /// </summary>
    public class DataManager : Core.ModuleBase
    {
        public override int Priority => 8;

        private readonly Dictionary<string, object> _configData = new Dictionary<string, object>();

        /// <summary>
        /// 加载配置数据
        /// </summary>
        public T LoadConfig<T>(string configName) where T : class
        {
            if (_configData.TryGetValue(configName, out object data))
            {
                return data as T;
            }

            // 从Resources加载配置
            TextAsset textAsset = Core.GameEntry.Resource.Load<TextAsset>($"Config/{configName}");
            if (textAsset != null)
            {
                T config = JsonUtility.FromJson<T>(textAsset.text);
                _configData[configName] = config;
                return config;
            }

            Debug.LogError($"Failed to load config: {configName}");
            return null;
        }

        /// <summary>
        /// 获取配置数据
        /// </summary>
        public T GetConfig<T>(string configName) where T : class
        {
            if (_configData.TryGetValue(configName, out object data))
            {
                return data as T;
            }
            return null;
        }

        /// <summary>
        /// 保存配置数据
        /// </summary>
        public void SaveConfig<T>(string configName, T config) where T : class
        {
            _configData[configName] = config;
        }

        /// <summary>
        /// 清空配置缓存
        /// </summary>
        public void ClearCache()
        {
            _configData.Clear();
        }

        public override void OnDestroy()
        {
            ClearCache();
        }
    }
}
