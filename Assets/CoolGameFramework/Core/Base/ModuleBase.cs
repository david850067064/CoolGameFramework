namespace CoolGameFramework.Core
{
    /// <summary>
    /// 模块基类
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        public virtual int Priority => 0;

        public virtual void OnInit() { }

        public virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// 固定帧率更新（FixedUpdate 节拍），默认空实现
        /// FrameTimer 依赖此方法保证帧精度
        /// </summary>
        public virtual void OnFixedUpdate(float fixedDeltaTime) { }

        public virtual void OnDestroy() { }
    }
}
