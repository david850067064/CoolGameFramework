using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolGameFramework.Core
{
    /// <summary>
    /// 模块管理器
    /// </summary>
    public class ModuleManager : Singleton<ModuleManager>
    {
        private readonly Dictionary<Type, IModule> _modules = new Dictionary<Type, IModule>();
        private readonly List<IModule> _updateModules = new List<IModule>();

        /// <summary>
        /// 注册模块
        /// </summary>
        public T RegisterModule<T>() where T : IModule, new()
        {
            Type type = typeof(T);
            if (_modules.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning($"Module {type.Name} already registered!");
                return (T)_modules[type];
            }

            T module = new T();
            _modules.Add(type, module);
            _updateModules.Add(module);

            // 按优先级排序
            _updateModules.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            module.OnInit();
            UnityEngine.Debug.Log($"Module {type.Name} registered successfully.");
            return module;
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        public T GetModule<T>() where T : IModule
        {
            Type type = typeof(T);
            if (_modules.TryGetValue(type, out IModule module))
            {
                return (T)module;
            }
            return default(T);
        }

        /// <summary>
        /// 更新所有模块
        /// </summary>
        public void Update(float deltaTime)
        {
            for (int i = 0; i < _updateModules.Count; i++)
            {
                _updateModules[i].OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 固定帧率更新所有模块（FixedUpdate 节拍）
        /// </summary>
        public void FixedUpdate(float fixedDeltaTime)
        {
            for (int i = 0; i < _updateModules.Count; i++)
            {
                _updateModules[i].OnFixedUpdate(fixedDeltaTime);
            }
        }

        /// <summary>
        /// 销毁所有模块
        /// </summary>
        public override void Dispose()
        {
            for (int i = _updateModules.Count - 1; i >= 0; i--)
            {
                _updateModules[i].OnDestroy();
            }
            _updateModules.Clear();
            _modules.Clear();
            base.Dispose();
        }
    }
}
