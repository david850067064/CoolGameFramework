using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 有限状态机管理器
    /// </summary>
    public class FSMManager : Core.ModuleBase
    {
        public override int Priority => 10;

        private readonly Dictionary<string, FSM> _fsms = new Dictionary<string, FSM>();

        /// <summary>
        /// 创建状态机
        /// </summary>
        public FSM CreateFSM(string fsmName)
        {
            if (_fsms.ContainsKey(fsmName))
            {
                Debug.LogWarning($"FSM {fsmName} already exists!");
                return _fsms[fsmName];
            }

            FSM fsm = new FSM(fsmName);
            _fsms.Add(fsmName, fsm);
            return fsm;
        }

        /// <summary>
        /// 获取状态机
        /// </summary>
        public FSM GetFSM(string fsmName)
        {
            _fsms.TryGetValue(fsmName, out FSM fsm);
            return fsm;
        }

        /// <summary>
        /// 销毁状态机
        /// </summary>
        public void DestroyFSM(string fsmName)
        {
            if (_fsms.TryGetValue(fsmName, out FSM fsm))
            {
                fsm.Shutdown();
                _fsms.Remove(fsmName);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var fsm in _fsms.Values)
            {
                fsm.Update(deltaTime);
            }
        }

        public override void OnDestroy()
        {
            foreach (var fsm in _fsms.Values)
            {
                fsm.Shutdown();
            }
            _fsms.Clear();
        }
    }

    /// <summary>
    /// 有限状态机
    /// </summary>
    public class FSM
    {
        public string Name { get; private set; }
        public IState CurrentState { get; private set; }

        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        public FSM(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        public void AddState(IState state)
        {
            Type stateType = state.GetType();
            if (_states.ContainsKey(stateType))
            {
                Debug.LogWarning($"State {stateType.Name} already exists in FSM {Name}!");
                return;
            }

            _states.Add(stateType, state);
            state.OnInit(this);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState<T>() where T : IState
        {
            Type stateType = typeof(T);
            if (!_states.TryGetValue(stateType, out IState newState))
            {
                Debug.LogError($"State {stateType.Name} not found in FSM {Name}!");
                return;
            }

            if (CurrentState != null)
            {
                CurrentState.OnExit();
            }

            CurrentState = newState;
            CurrentState.OnEnter();
        }

        /// <summary>
        /// 更新当前状态
        /// </summary>
        public void Update(float deltaTime)
        {
            CurrentState?.OnUpdate(deltaTime);
        }

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public void Shutdown()
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit();
                CurrentState = null;
            }

            foreach (var state in _states.Values)
            {
                state.OnDestroy();
            }
            _states.Clear();
        }
    }

    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState
    {
        void OnInit(FSM fsm);
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
        void OnDestroy();
    }

    /// <summary>
    /// 状态基类
    /// </summary>
    public abstract class StateBase : IState
    {
        protected FSM fsm;

        public virtual void OnInit(FSM fsm)
        {
            this.fsm = fsm;
        }

        public virtual void OnEnter() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnExit() { }
        public virtual void OnDestroy() { }
    }
}
