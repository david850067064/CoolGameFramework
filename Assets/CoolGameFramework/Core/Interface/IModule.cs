namespace CoolGameFramework.Core
{
    /// <summary>
    /// 模块接口
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// 模块优先级（数字越小越先初始化）
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 初始化模块
        /// </summary>
        void OnInit();

        /// <summary>
        /// 模块更新
        /// </summary>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// 固定帧率更新（FixedUpdate 节拍）
        /// </summary>
        void OnFixedUpdate(float fixedDeltaTime);

        /// <summary>
        /// 模块销毁
        /// </summary>
        void OnDestroy();
    }
}
