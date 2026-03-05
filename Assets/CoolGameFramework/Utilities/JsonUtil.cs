using UnityEngine;

namespace CoolGameFramework.Utilities
{
    /// <summary>
    /// JSON工具类
    /// </summary>
    public static class JsonUtil
    {
        /// <summary>
        /// 对象转JSON
        /// </summary>
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        /// <summary>
        /// JSON转对象
        /// </summary>
        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// 覆盖对象
        /// </summary>
        public static void FromJsonOverwrite(string json, object objectToOverwrite)
        {
            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
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
                return default(T);

            return FromJson<T>(json);
        }
    }
}
