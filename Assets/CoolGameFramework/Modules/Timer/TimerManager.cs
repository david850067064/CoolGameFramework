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

        // ─── FrameTimer 扩展 ────────────────────────────────────────
        private readonly List<FrameTimer> _frameTimers = new List<FrameTimer>();
        private readonly List<FrameTimer> _frameTimersToRemove = new List<FrameTimer>();

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

        // ─── FrameTimer 接口 ────────────────────────────────────────

        /// <summary>
        /// 注册帧计数回调（在 FixedUpdate 节拍下精确到帧，用于战斗窗口判定）
        /// </summary>
        /// <param name="frames">等待的帧数（基于 FixedUpdate）</param>
        /// <param name="onComplete">帧数到达后的回调</param>
        /// <returns>FrameTimer 句柄，可用于提前取消</returns>
        public FrameTimer RegisterFrameCallback(int frames, Action onComplete)
        {
            var ft = new FrameTimer(frames, onComplete, loop: false);
            _frameTimers.Add(ft);
            return ft;
        }

        /// <summary>
        /// 注册帧计数窗口：在 [startFrame, endFrame] 区间内每帧回调 onWindow，
        /// 离开窗口后回调 onWindowEnd。用于弹反/闪避窗口判定。
        /// </summary>
        public FrameTimer RegisterFrameWindow(int startFrame, int endFrame,
            Action onWindowEnter, Action onWindowActive, Action onWindowEnd)
        {
            var ft = new FrameTimer(startFrame, endFrame, onWindowEnter, onWindowActive, onWindowEnd);
            _frameTimers.Add(ft);
            return ft;
        }

        /// <summary>
        /// 移除帧计数定时器
        /// </summary>
        public void RemoveFrameTimer(FrameTimer ft)
        {
            if (ft != null && !_frameTimersToRemove.Contains(ft))
            {
                _frameTimersToRemove.Add(ft);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            // 更新浮点定时器
            for (int i = 0; i < _timers.Count; i++)
            {
                Timer timer = _timers[i];
                if (timer.IsActive)
                {
                    timer.Update(deltaTime);
                    if (timer.IsCompleted && !timer.IsLoop)
                    {
                        if (!_timersToRemove.Contains(timer))
                            _timersToRemove.Add(timer);
                    }
                }
            }

            if (_timersToRemove.Count > 0)
            {
                foreach (var timer in _timersToRemove)
                    _timers.Remove(timer);
                _timersToRemove.Clear();
            }
        }

        /// <summary>
        /// FrameTimer 在 FixedUpdate 节拍下推进（由 ModuleManager 调用 OnFixedUpdate）
        /// </summary>
        public override void OnFixedUpdate(float fixedDeltaTime)
        {
            for (int i = 0; i < _frameTimers.Count; i++)
            {
                FrameTimer ft = _frameTimers[i];
                if (ft.IsActive)
                {
                    ft.Tick();
                    if (ft.IsFinished)
                    {
                        if (!_frameTimersToRemove.Contains(ft))
                            _frameTimersToRemove.Add(ft);
                    }
                }
            }

            if (_frameTimersToRemove.Count > 0)
            {
                foreach (var ft in _frameTimersToRemove)
                    _frameTimers.Remove(ft);
                _frameTimersToRemove.Clear();
            }
        }

        public override void OnDestroy()
        {
            _timers.Clear();
            _timersToRemove.Clear();
            _frameTimers.Clear();
            _frameTimersToRemove.Clear();
        }
    }

    /// <summary>
    /// 浮点定时器类
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
                    ElapsedTime -= Duration;
                }
                else
                {
                    IsActive = false;
                }
            }
        }

        public void Pause() { IsActive = false; }
        public void Resume() { IsActive = true; }
        public void Stop() { IsActive = false; ElapsedTime = 0f; }
    }

    /// <summary>
    /// 帧精度计数器 — 基于 FixedUpdate，用于战斗窗口判定
    /// 不受帧率波动影响，FixedUpdate 固定 60 次/秒
    /// </summary>
    public class FrameTimer
    {
        public bool IsActive { get; private set; }
        public bool IsFinished { get; private set; }
        public int CurrentFrame { get; private set; }

        // 简单模式：等待 N 帧后回调
        private readonly int _targetFrame;
        private readonly Action _onComplete;

        // 窗口模式：[startFrame, endFrame] 区间回调
        private readonly bool _isWindowMode;
        private readonly int _windowStart;
        private readonly int _windowEnd;
        private readonly Action _onWindowEnter;
        private readonly Action _onWindowActive;
        private readonly Action _onWindowEnd;
        private bool _inWindow;

        /// <summary>简单模式：N帧后回调</summary>
        public FrameTimer(int frames, Action onComplete, bool loop = false)
        {
            _targetFrame = frames;
            _onComplete = onComplete;
            IsActive = true;
            IsFinished = false;
            CurrentFrame = 0;
            _isWindowMode = false;
        }

        /// <summary>窗口模式：startFrame 进入窗口，endFrame 离开窗口</summary>
        public FrameTimer(int startFrame, int endFrame,
            Action onWindowEnter, Action onWindowActive, Action onWindowEnd)
        {
            _isWindowMode = true;
            _windowStart = startFrame;
            _windowEnd = endFrame;
            _onWindowEnter = onWindowEnter;
            _onWindowActive = onWindowActive;
            _onWindowEnd = onWindowEnd;
            IsActive = true;
            IsFinished = false;
            CurrentFrame = 0;
        }

        public void Tick()
        {
            if (!IsActive || IsFinished) return;

            CurrentFrame++;

            if (_isWindowMode)
            {
                if (CurrentFrame == _windowStart)
                {
                    _inWindow = true;
                    _onWindowEnter?.Invoke();
                }

                if (_inWindow && CurrentFrame <= _windowEnd)
                {
                    _onWindowActive?.Invoke();
                }

                if (_inWindow && CurrentFrame > _windowEnd)
                {
                    _inWindow = false;
                    _onWindowEnd?.Invoke();
                    IsFinished = true;
                }
            }
            else
            {
                if (CurrentFrame >= _targetFrame)
                {
                    _onComplete?.Invoke();
                    IsFinished = true;
                }
            }
        }

        /// <summary>检查当前帧是否在窗口内（用于弹反判定查询）</summary>
        public bool IsInWindow() => _isWindowMode && _inWindow;

        public void Cancel() { IsActive = false; IsFinished = true; }
    }
}
