using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// Tween动画管理器
    /// </summary>
    public class TweenManager : Core.ModuleBase
    {
        public override int Priority => 16;

        private readonly List<TweenBase> _tweens = new List<TweenBase>();
        private readonly List<TweenBase> _tweensToRemove = new List<TweenBase>();

        /// <summary>
        /// 移动动画
        /// </summary>
        public TweenPosition MoveTo(Transform target, Vector3 endValue, float duration)
        {
            TweenPosition tween = new TweenPosition(target, endValue, duration);
            _tweens.Add(tween);
            return tween;
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        public TweenScale ScaleTo(Transform target, Vector3 endValue, float duration)
        {
            TweenScale tween = new TweenScale(target, endValue, duration);
            _tweens.Add(tween);
            return tween;
        }

        /// <summary>
        /// 旋转动画
        /// </summary>
        public TweenRotation RotateTo(Transform target, Vector3 endValue, float duration)
        {
            TweenRotation tween = new TweenRotation(target, endValue, duration);
            _tweens.Add(tween);
            return tween;
        }

        /// <summary>
        /// 透明度动画
        /// </summary>
        public TweenAlpha FadeTo(CanvasGroup target, float endValue, float duration)
        {
            TweenAlpha tween = new TweenAlpha(target, endValue, duration);
            _tweens.Add(tween);
            return tween;
        }

        /// <summary>
        /// 数值动画
        /// </summary>
        public TweenValue ValueTo(float startValue, float endValue, float duration, Action<float> onUpdate)
        {
            TweenValue tween = new TweenValue(startValue, endValue, duration, onUpdate);
            _tweens.Add(tween);
            return tween;
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        public void Stop(TweenBase tween)
        {
            if (tween != null && !_tweensToRemove.Contains(tween))
            {
                _tweensToRemove.Add(tween);
            }
        }

        /// <summary>
        /// 停止目标的所有动画
        /// </summary>
        public void StopAll(Transform target)
        {
            foreach (var tween in _tweens)
            {
                if (tween.Target == target)
                {
                    _tweensToRemove.Add(tween);
                }
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _tweens.Count; i++)
            {
                TweenBase tween = _tweens[i];
                if (tween.IsPlaying)
                {
                    tween.Update(deltaTime);
                    if (tween.IsCompleted)
                    {
                        _tweensToRemove.Add(tween);
                    }
                }
            }

            if (_tweensToRemove.Count > 0)
            {
                foreach (var tween in _tweensToRemove)
                {
                    _tweens.Remove(tween);
                }
                _tweensToRemove.Clear();
            }
        }

        public override void OnDestroy()
        {
            _tweens.Clear();
            _tweensToRemove.Clear();
        }
    }

    /// <summary>
    /// Tween基类
    /// </summary>
    public abstract class TweenBase
    {
        public Transform Target { get; protected set; }
        public float Duration { get; protected set; }
        public float ElapsedTime { get; protected set; }
        public bool IsPlaying { get; protected set; }
        public bool IsCompleted => ElapsedTime >= Duration;

        protected Action _onComplete;
        protected Func<float, float> _easeFunc = EaseLinear;

        public TweenBase(float duration)
        {
            Duration = duration;
            IsPlaying = true;
            ElapsedTime = 0f;
        }

        public virtual void Update(float deltaTime)
        {
            if (!IsPlaying) return;

            ElapsedTime += deltaTime;
            float t = Mathf.Clamp01(ElapsedTime / Duration);
            float easedT = _easeFunc(t);

            UpdateValue(easedT);

            if (IsCompleted)
            {
                IsPlaying = false;
                _onComplete?.Invoke();
            }
        }

        protected abstract void UpdateValue(float t);

        public TweenBase SetEase(Func<float, float> easeFunc)
        {
            _easeFunc = easeFunc;
            return this;
        }

        public TweenBase OnComplete(Action callback)
        {
            _onComplete = callback;
            return this;
        }

        // 缓动函数
        public static float EaseLinear(float t) => t;
        public static float EaseInQuad(float t) => t * t;
        public static float EaseOutQuad(float t) => t * (2 - t);
        public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        public static float EaseInCubic(float t) => t * t * t;
        public static float EaseOutCubic(float t) => (--t) * t * t + 1;
    }

    /// <summary>
    /// 位置动画
    /// </summary>
    public class TweenPosition : TweenBase
    {
        private Vector3 _startValue;
        private Vector3 _endValue;

        public TweenPosition(Transform target, Vector3 endValue, float duration) : base(duration)
        {
            Target = target;
            _startValue = target.position;
            _endValue = endValue;
        }

        protected override void UpdateValue(float t)
        {
            if (Target != null)
            {
                Target.position = Vector3.Lerp(_startValue, _endValue, t);
            }
        }
    }

    /// <summary>
    /// 缩放动画
    /// </summary>
    public class TweenScale : TweenBase
    {
        private Vector3 _startValue;
        private Vector3 _endValue;

        public TweenScale(Transform target, Vector3 endValue, float duration) : base(duration)
        {
            Target = target;
            _startValue = target.localScale;
            _endValue = endValue;
        }

        protected override void UpdateValue(float t)
        {
            if (Target != null)
            {
                Target.localScale = Vector3.Lerp(_startValue, _endValue, t);
            }
        }
    }

    /// <summary>
    /// 旋转动画
    /// </summary>
    public class TweenRotation : TweenBase
    {
        private Vector3 _startValue;
        private Vector3 _endValue;

        public TweenRotation(Transform target, Vector3 endValue, float duration) : base(duration)
        {
            Target = target;
            _startValue = target.eulerAngles;
            _endValue = endValue;
        }

        protected override void UpdateValue(float t)
        {
            if (Target != null)
            {
                Target.eulerAngles = Vector3.Lerp(_startValue, _endValue, t);
            }
        }
    }

    /// <summary>
    /// 透明度动画
    /// </summary>
    public class TweenAlpha : TweenBase
    {
        private CanvasGroup _canvasGroup;
        private float _startValue;
        private float _endValue;

        public TweenAlpha(CanvasGroup target, float endValue, float duration) : base(duration)
        {
            _canvasGroup = target;
            _startValue = target.alpha;
            _endValue = endValue;
        }

        protected override void UpdateValue(float t)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(_startValue, _endValue, t);
            }
        }
    }

    /// <summary>
    /// 数值动画
    /// </summary>
    public class TweenValue : TweenBase
    {
        private float _startValue;
        private float _endValue;
        private Action<float> _onUpdate;

        public TweenValue(float startValue, float endValue, float duration, Action<float> onUpdate) : base(duration)
        {
            _startValue = startValue;
            _endValue = endValue;
            _onUpdate = onUpdate;
        }

        protected override void UpdateValue(float t)
        {
            float value = Mathf.Lerp(_startValue, _endValue, t);
            _onUpdate?.Invoke(value);
        }
    }
}
