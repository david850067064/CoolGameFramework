using System;
using System.Collections.Generic;
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

        public override void OnInit()
        {
            Application.logMessageReceived += OnLogMessageReceived;
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
            _enableFileLog = enabled;
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
        }

        public void LogDebug(string message) => Log(message, LogLevel.Debug);
        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogError(string message) => Log(message, LogLevel.Error);

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!_enableFileLog) return;
            // TODO: 写入文件
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

        public override void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            _logCache.Clear();
        }
    }
}
