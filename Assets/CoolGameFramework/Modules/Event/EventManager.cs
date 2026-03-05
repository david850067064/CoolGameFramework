using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager : Core.ModuleBase
    {
        public override int Priority => 1;

        private readonly Dictionary<Type, Delegate> _eventDict = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventDict.TryGetValue(eventType, out Delegate existingDelegate))
            {
                _eventDict[eventType] = Delegate.Combine(existingDelegate, handler);
            }
            else
            {
                _eventDict[eventType] = handler;
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventDict.TryGetValue(eventType, out Delegate existingDelegate))
            {
                Delegate newDelegate = Delegate.Remove(existingDelegate, handler);
                if (newDelegate == null)
                {
                    _eventDict.Remove(eventType);
                }
                else
                {
                    _eventDict[eventType] = newDelegate;
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventDict.TryGetValue(eventType, out Delegate existingDelegate))
            {
                (existingDelegate as Action<T>)?.Invoke(eventData);
            }
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void Clear()
        {
            _eventDict.Clear();
        }

        public override void OnDestroy()
        {
            Clear();
        }
    }
}
