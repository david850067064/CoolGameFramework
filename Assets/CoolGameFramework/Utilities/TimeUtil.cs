using System;
using UnityEngine;

namespace CoolGameFramework.Utilities
{
    /// <summary>
    /// 时间工具类
    /// </summary>
    public static class TimeUtil
    {
        /// <summary>
        /// 获取当前时间戳（秒）
        /// </summary>
        public static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        public static long GetTimestampMilliseconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 时间戳转DateTime
        /// </summary>
        public static DateTime TimestampToDateTime(long timestamp)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return startTime.AddSeconds(timestamp).ToLocalTime();
        }

        /// <summary>
        /// DateTime转时间戳
        /// </summary>
        public static long DateTimeToTimestamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        public static string FormatTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);

            if (hours > 0)
                return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, secs);
            else
                return string.Format("{0:00}:{1:00}", minutes, secs);
        }

        /// <summary>
        /// 格式化日期时间
        /// </summary>
        public static string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        /// 获取今天开始时间
        /// </summary>
        public static DateTime GetTodayStart()
        {
            return DateTime.Today;
        }

        /// <summary>
        /// 获取今天结束时间
        /// </summary>
        public static DateTime GetTodayEnd()
        {
            return DateTime.Today.AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// 计算时间差
        /// </summary>
        public static TimeSpan GetTimeSpan(DateTime start, DateTime end)
        {
            return end - start;
        }

        /// <summary>
        /// 是否是同一天
        /// </summary>
        public static bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day;
        }
    }
}
