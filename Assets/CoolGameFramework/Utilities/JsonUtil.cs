using UnityEngine;
using Newtonsoft.Json;

namespace CoolGameFramework.Utilities
{
    /// <summary>
    /// JSON工具类 — 使用 Newtonsoft.Json 替代 JsonUtility
    /// 支持 Dictionary、多态、自定义 Converter，解决 JsonUtility 序列化限制
    /// 依赖：com.unity.nuget.newtonsoft-json（Unity Package Manager）
    /// </summary>
    public static class JsonUtil
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None
        };

        private static readonly JsonSerializerSettings _prettySettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// 对象转JSON
        /// </summary>
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            return JsonConvert.SerializeObject(obj, prettyPrint ? _prettySettings : _settings);
        }

        /// <summary>
        /// JSON转对象
        /// </summary>
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// 覆盖对象（兼容旧接口）
        /// </summary>
        public static void FromJsonOverwrite(string json, object objectToOverwrite)
        {
            if (string.IsNullOrEmpty(json)) return;
            JsonConvert.PopulateObject(json, objectToOverwrite, _settings);
        }

        /// <summary>
        /// 保存JSON到文件
        /// </summary>
        public static void SaveToFile(string path, object obj, bool prettyPrint = true)
        {
            string json = ToJson(obj, prettyPrint);
            FileUtil.WriteText(path, json);
        }

        /// <summary>
        /// 从文件加载JSON
        /// </summary>
        public static T LoadFromFile<T>(string path)
        {
            string json = FileUtil.ReadText(path);
            if (string.IsNullOrEmpty(json))
                return default;
            return FromJson<T>(json);
        }
    }
}
