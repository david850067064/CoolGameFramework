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

        public virtual void OnDestroy() { }
    }
}
