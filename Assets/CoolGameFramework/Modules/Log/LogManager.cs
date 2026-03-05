using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public class LogManager : Core.ModuleBase
    {
        public override int Priority => 0;

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        private bool _enableLog = true;
        private bool _enableFileLog = false;
        private LogLevel _minLogLevel = LogLevel.Debug;
        private readonly List<string> _logCache = new List<string>();
        private const int MaxCacheSize = 1000;
        private StreamWriter _logWriter;
        private string _logFilePath;

        public override void OnInit()
        {
            Application.logMessageReceived += OnLogMessageReceived;

            if (_enableFileLog)
            {
                InitializeFileLog();
            }
        }

        /// <summary>
        /// 初始化文件日志
        /// </summary>
        private void InitializeFileLog()
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(logDirectory, $"Log_{timestamp}.txt");

            try
            {
                _logWriter = new StreamWriter(_logFilePath, true, Encoding.UTF8);
                _logWriter.AutoFlush = true;

                WriteToFile($"=== Log Started at {DateTime.Now} ===");
                WriteToFile($"Unity Version: {Application.unityVersion}");
                WriteToFile($"Platform: {Application.platform}");
                WriteToFile($"Device: {SystemInfo.deviceModel}");
                WriteToFile("=====================================\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize file log: {e.Message}");
                _enableFileLog = false;
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        private void WriteToFile(string message)
        {
            if (_logWriter != null)
            {
                try
                {
                    _logWriter.WriteLine(message);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to write log to file: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 设置日志开关
        /// </summary>
        public void SetLogEnabled(bool enabled)
        {
            _enableLog = enabled;
        }

        /// <summary>
        /// 设置文件日志
        /// </summary>
        public void SetFileLogEnabled(bool enabled)
        {
            if (_enableFileLog == enabled) return;

            _enableFileLog = enabled;

            if (_enableFileLog)
            {
                InitializeFileLog();
            }
            else
            {
                CloseFileLog();
            }
        }

        /// <summary>
        /// 设置最小日志级别
        /// </summary>
        public void SetMinLogLevel(LogLevel level)
        {
            _minLogLevel = level;
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!_enableLog || level < _minLogLevel) return;

            string logMessage = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(logMessage);
                    break;
            }

            CacheLog(logMessage);

            if (_enableFileLog)
            {
                WriteToFile(logMessage);
            }
        }

        public void LogDebug(string message) => Log(message, LogLevel.Debug);
        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogError(string message) => Log(message, LogLevel.Error);

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!_enableFileLog) return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] [{type}] {condition}";

            if (type == LogType.Error || type == LogType.Exception)
            {
                logMessage += $"\n{stackTrace}";
            }

            WriteToFile(logMessage);
        }

        private void CacheLog(string log)
        {
            _logCache.Add(log);
            if (_logCache.Count > MaxCacheSize)
            {
                _logCache.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取日志缓存
        /// </summary>
        public List<string> GetLogCache()
        {
            return new List<string>(_logCache);
        }

        /// <summary>
        /// 关闭文件日志
        /// </summary>
        private void CloseFileLog()
        {
            if (_logWriter != null)
            {
                try
                {
                    WriteToFile($"\n=== Log Ended at {DateTime.Now} ===");
                    _logWriter.Close();
                    _logWriter.Dispose();
                    _logWriter = null;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to close log file: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 打开日志文件夹
        /// </summary>
        public void OpenLogFolder()
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            if (Directory.Exists(logDirectory))
            {
                Application.OpenURL("file://" + logDirectory);
            }
        }

        /// <summary>
        /// 清理旧日志
        /// </summary>
        public void ClearOldLogs(int keepDays = 7)
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(logDirectory)) return;

            try
            {
                string[] logFiles = Directory.GetFiles(logDirectory, "*.txt");
                DateTime threshold = DateTime.Now.AddDays(-keepDays);

                foreach (string file in logFiles)
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                    if (fileInfo.CreationTime < threshold)
                    {
                        File.Delete(file);
                        Debug.Log($"Deleted old log file: {fileInfo.Name}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear old logs: {e.Message}");
            }
        }

        public override void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            CloseFileLog();
            _logCache.Clear();
        }
    }
}
