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

    /// <summary>
    /// 存档管理器
    /// </summary>
    public class SaveManager
    {
        /// <summary>
        /// 保存数据
        /// </summary>
        public static void Save<T>(string key, T data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public static T Load<T>(string key, T defaultValue = default)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<T>(json);
            }
            return defaultValue;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public static void Delete(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 检查数据是否存在
        /// </summary>
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 保存整数
        /// </summary>
        public static void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载整数
        /// </summary>
        public static int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <summary>
        /// 保存浮点数
        /// </summary>
        public static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载浮点数
        /// </summary>
        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        public static void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载字符串
        /// </summary>
        public static string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
    }
}
