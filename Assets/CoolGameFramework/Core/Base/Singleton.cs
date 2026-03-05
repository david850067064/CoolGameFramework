using System;

namespace CoolGameFramework.Core
{
    /// <summary>
    /// 单例基类（非MonoBehaviour）
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void Dispose()
        {
            _instance = null;
        }
    }
}
