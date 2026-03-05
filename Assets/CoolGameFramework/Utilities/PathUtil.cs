using System.IO;
using UnityEngine;

namespace CoolGameFramework.Utilities
{
    /// <summary>
    /// 路径工具类
    /// </summary>
    public static class PathUtil
    {
        /// <summary>
        /// 获取持久化数据路径
        /// </summary>
        public static string GetPersistentDataPath(string fileName = "")
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        /// <summary>
        /// 获取StreamingAssets路径
        /// </summary>
        public static string GetStreamingAssetsPath(string fileName = "")
        {
            return Path.Combine(Application.streamingAssetsPath, fileName);
        }

        /// <summary>
        /// 获取临时缓存路径
        /// </summary>
        public static string GetTemporaryCachePath(string fileName = "")
        {
            return Path.Combine(Application.temporaryCachePath, fileName);
        }

        /// <summary>
        /// 确保目录存在
        /// </summary>
        public static void EnsureDirectory(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 组合路径
        /// </summary>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            string result = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                result = Path.Combine(result, paths[i]);
            }
            return result;
        }

        /// <summary>
        /// 获取文件名（不含扩展名）
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// 获取扩展名
        /// </summary>
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }
    }
}
