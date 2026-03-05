using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 定时器管理器
    /// </summary>
    public class TimerManager : Core.ModuleBase
    {
        public override int Priority => 2;

        private readonly List<Timer> _timers = new List<Timer>();
        private readonly List<Timer> _timersToRemove = new List<Timer>();

        /// <summary>
        /// 创建定时器
        /// </summary>
        public Timer CreateTimer(float duration, Action onComplete, bool loop = false)
        {
            Timer timer = new Timer(duration, onComplete, loop);
            _timers.Add(timer);
            return timer;
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        public Timer DelayCall(float delay, Action callback)
        {
            return CreateTimer(delay, callback, false);
        }

        /// <summary>
        /// 循环执行
        /// </summary>
        public Timer LoopCall(float interval, Action callback)
        {
            return CreateTimer(interval, callback, true);
        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        public void RemoveTimer(Timer timer)
        {
            if (timer != null && !_timersToRemove.Contains(timer))
            {
                _timersToRemove.Add(timer);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            // 更新所有定时器
            for (int i = 0; i < _timers.Count; i++)
            {
                Timer timer = _timers[i];
                if (timer.IsActive)
                {
                    timer.Update(deltaTime);
                    if (timer.IsCompleted && !timer.IsLoop)
                    {
                        _timersToRemove.Add(timer);
                    }
                }
            }

            // 移除已完成的定时器
            if (_timersToRemove.Count > 0)
            {
                foreach (var timer in _timersToRemove)
                {
                    _timers.Remove(timer);
                }
                _timersToRemove.Clear();
            }
        }

        public override void OnDestroy()
        {
            _timers.Clear();
            _timersToRemove.Clear();
        }
    }

    /// <summary>
    /// 定时器类
    /// </summary>
    public class Timer
    {
        public float Duration { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsLoop { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsCompleted => ElapsedTime >= Duration;

        private Action _onComplete;

        public Timer(float duration, Action onComplete, bool loop)
        {
            Duration = duration;
            _onComplete = onComplete;
            IsLoop = loop;
            IsActive = true;
            ElapsedTime = 0f;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            ElapsedTime += deltaTime;
            if (ElapsedTime >= Duration)
            {
                _onComplete?.Invoke();
                if (IsLoop)
                {
                    ElapsedTime = 0f;
                }
                else
                {
                    IsActive = false;
                }
            }
        }

        public void Pause()
        {
            IsActive = false;
        }

        public void Resume()
        {
            IsActive = true;
        }

        public void Stop()
        {
            IsActive = false;
            ElapsedTime = 0f;
        }
    }
}
