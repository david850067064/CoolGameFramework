using UnityEngine;

namespace CoolGameFramework.Utilities
{
    /// <summary>
    /// 数学工具类
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// 限制值在范围内
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// 限制值在0-1之间
        /// </summary>
        public static float Clamp01(float value)
        {
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// 线性插值
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return Mathf.Lerp(a, b, t);
        }

        /// <summary>
        /// 平滑插值
        /// </summary>
        public static float SmoothStep(float a, float b, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(a, b, t);
        }

        /// <summary>
        /// 计算两点距离
        /// </summary>
        public static float Distance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        /// <summary>
        /// 计算两点距离（2D）
        /// </summary>
        public static float Distance2D(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static float DegreeToRadian(float degree)
        {
            return degree * Mathf.Deg2Rad;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public static float RadianToDegree(float radian)
        {
            return radian * Mathf.Rad2Deg;
        }

        /// <summary>
        /// 随机范围
        /// </summary>
        public static float RandomRange(float min, float max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// 随机整数范围
        /// </summary>
        public static int RandomRangeInt(int min, int max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// 随机布尔值
        /// </summary>
        public static bool RandomBool()
        {
            return Random.value > 0.5f;
        }

        /// <summary>
        /// 百分比概率
        /// </summary>
        public static bool RandomProbability(float probability)
        {
            return Random.value <= probability;
        }

        /// <summary>
        /// 向量归一化
        /// </summary>
        public static Vector3 Normalize(Vector3 vector)
        {
            return vector.normalized;
        }

        /// <summary>
        /// 计算方向向量
        /// </summary>
        public static Vector3 Direction(Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        /// <summary>
        /// 四舍五入
        /// </summary>
        public static int Round(float value)
        {
            return Mathf.RoundToInt(value);
        }

        /// <summary>
        /// 向上取整
        /// </summary>
        public static int Ceil(float value)
        {
            return Mathf.CeilToInt(value);
        }

        /// <summary>
        /// 向下取整
        /// </summary>
        public static int Floor(float value)
        {
            return Mathf.FloorToInt(value);
        }

        /// <summary>
        /// 绝对值
        /// </summary>
        public static float Abs(float value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// 符号
        /// </summary>
        public static float Sign(float value)
        {
            return Mathf.Sign(value);
        }
    }
}
