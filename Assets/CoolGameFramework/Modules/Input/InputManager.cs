using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 输入管理器
    /// </summary>
    public class InputManager : Core.ModuleBase
    {
        public override int Priority => 3;

        private readonly Dictionary<string, KeyCode> _keyBindings = new Dictionary<string, KeyCode>();
        private readonly Dictionary<string, Action> _keyDownActions = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action> _keyUpActions = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action> _keyHoldActions = new Dictionary<string, Action>();

        public override void OnInit()
        {
            // 默认按键绑定
            RegisterKey("Jump", KeyCode.Space);
            RegisterKey("Fire", KeyCode.Mouse0);
            RegisterKey("Interact", KeyCode.E);
            RegisterKey("Cancel", KeyCode.Escape);
        }

        /// <summary>
        /// 注册按键
        /// </summary>
        public void RegisterKey(string actionName, KeyCode keyCode)
        {
            if (_keyBindings.ContainsKey(actionName))
            {
                _keyBindings[actionName] = keyCode;
            }
            else
            {
                _keyBindings.Add(actionName, keyCode);
            }
        }

        /// <summary>
        /// 绑定按键按下事件
        /// </summary>
        public void BindKeyDown(string actionName, Action callback)
        {
            if (_keyDownActions.ContainsKey(actionName))
            {
                _keyDownActions[actionName] += callback;
            }
            else
            {
                _keyDownActions.Add(actionName, callback);
            }
        }

        /// <summary>
        /// 绑定按键抬起事件
        /// </summary>
        public void BindKeyUp(string actionName, Action callback)
        {
            if (_keyUpActions.ContainsKey(actionName))
            {
                _keyUpActions[actionName] += callback;
            }
            else
            {
                _keyUpActions.Add(actionName, callback);
            }
        }

        /// <summary>
        /// 绑定按键持续按下事件
        /// </summary>
        public void BindKeyHold(string actionName, Action callback)
        {
            if (_keyHoldActions.ContainsKey(actionName))
            {
                _keyHoldActions[actionName] += callback;
            }
            else
            {
                _keyHoldActions.Add(actionName, callback);
            }
        }

        /// <summary>
        /// 获取按键是否按下
        /// </summary>
        public bool GetKeyDown(string actionName)
        {
            if (_keyBindings.TryGetValue(actionName, out KeyCode keyCode))
            {
                return Input.GetKeyDown(keyCode);
            }
            return false;
        }

        /// <summary>
        /// 获取按键是否抬起
        /// </summary>
        public bool GetKeyUp(string actionName)
        {
            if (_keyBindings.TryGetValue(actionName, out KeyCode keyCode))
            {
                return Input.GetKeyUp(keyCode);
            }
            return false;
        }

        /// <summary>
        /// 获取按键是否持续按下
        /// </summary>
        public bool GetKey(string actionName)
        {
            if (_keyBindings.TryGetValue(actionName, out KeyCode keyCode))
            {
                return Input.GetKey(keyCode);
            }
            return false;
        }

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        public Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }

        /// <summary>
        /// 获取轴输入
        /// </summary>
        public float GetAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 检测按键按下
            foreach (var kvp in _keyDownActions)
            {
                if (GetKeyDown(kvp.Key))
                {
                    kvp.Value?.Invoke();
                }
            }

            // 检测按键抬起
            foreach (var kvp in _keyUpActions)
            {
                if (GetKeyUp(kvp.Key))
                {
                    kvp.Value?.Invoke();
                }
            }

            // 检测按键持续按下
            foreach (var kvp in _keyHoldActions)
            {
                if (GetKey(kvp.Key))
                {
                    kvp.Value?.Invoke();
                }
            }
        }

        public override void OnDestroy()
        {
            _keyBindings.Clear();
            _keyDownActions.Clear();
            _keyUpActions.Clear();
            _keyHoldActions.Clear();
        }
    }
}
