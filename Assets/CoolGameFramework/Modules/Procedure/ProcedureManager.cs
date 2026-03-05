using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 流程管理器
    /// </summary>
    public class ProcedureManager : Core.ModuleBase
    {
        public override int Priority => 11;

        private FSM _procedureFSM;
        private readonly Dictionary<Type, ProcedureBase> _procedures = new Dictionary<Type, ProcedureBase>();

        public ProcedureBase CurrentProcedure => _procedureFSM?.CurrentState as ProcedureBase;

        public override void OnInit()
        {
            _procedureFSM = Core.GameEntry.FSM.CreateFSM("ProcedureFSM");
        }

        /// <summary>
        /// 添加流程
        /// </summary>
        public void AddProcedure<T>() where T : ProcedureBase, new()
        {
            Type procedureType = typeof(T);
            if (_procedures.ContainsKey(procedureType))
            {
                Debug.LogWarning($"Procedure {procedureType.Name} already exists!");
                return;
            }

            T procedure = new T();
            _procedures.Add(procedureType, procedure);
            _procedureFSM.AddState(procedure);
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        public void ChangeProcedure<T>() where T : ProcedureBase
        {
            _procedureFSM.ChangeState<T>();
        }

        /// <summary>
        /// 获取流程
        /// </summary>
        public T GetProcedure<T>() where T : ProcedureBase
        {
            Type procedureType = typeof(T);
            if (_procedures.TryGetValue(procedureType, out ProcedureBase procedure))
            {
                return procedure as T;
            }
            return null;
        }

        public override void OnDestroy()
        {
            _procedures.Clear();
        }
    }

    /// <summary>
    /// 流程基类
    /// </summary>
    public abstract class ProcedureBase : StateBase
    {
        public override void OnEnter()
        {
            Debug.Log($"Enter Procedure: {GetType().Name}");
        }

        public override void OnExit()
        {
            Debug.Log($"Exit Procedure: {GetType().Name}");
        }
    }
}
