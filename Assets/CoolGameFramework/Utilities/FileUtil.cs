using System.IO;
using System.Text;
using UnityEngine;

namespace CoolGameFramework.Utilities
{
    /// <summary>
    /// 文件工具类
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// 读取文本文件
        /// </summary>
        public static string ReadText(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// 写入文本文件
        /// </summary>
        public static void WriteText(string path, string content)
        {
            PathUtil.EnsureDirectory(path);
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        /// <summary>
        /// 读取字节数组
        /// </summary>
        public static byte[] ReadBytes(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return null;
            }

            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// 写入字节数组
        /// </summary>
        public static void WriteBytes(string path, byte[] bytes)
        {
            PathUtil.EnsureDirectory(path);
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        public static void CopyFile(string sourcePath, string destPath, bool overwrite = true)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"Source file not found: {sourcePath}");
                return;
            }

            PathUtil.EnsureDirectory(destPath);
            File.Copy(sourcePath, destPath, overwrite);
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        public static void MoveFile(string sourcePath, string destPath)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"Source file not found: {sourcePath}");
                return;
            }

            PathUtil.EnsureDirectory(destPath);
            File.Move(sourcePath, destPath);
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        public static long GetFileSize(string path)
        {
            if (!File.Exists(path))
                return 0;

            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Length;
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        public static void DeleteDirectory(string path, bool recursive = true)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }

        /// <summary>
        /// 获取目录下所有文件
        /// </summary>
        public static string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(path))
                return new string[0];

            return Directory.GetFiles(path, searchPattern, searchOption);
        }

        /// <summary>
        /// 获取目录下所有子目录
        /// </summary>
        public static string[] GetDirectories(string path)
        {
            if (!Directory.Exists(path))
                return new string[0];

            return Directory.GetDirectories(path);
        }
    }
}
