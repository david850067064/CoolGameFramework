using UnityEngine;
using CoolGameFramework.Core;
using CoolGameFramework.Modules;

namespace CoolGameFramework.Examples
{
    /// <summary>
    /// 框架使用示例
    /// </summary>
    public class FrameworkExample : MonoBehaviour
    {
        private void Start()
        {
            // 示例：事件系统
            ExampleEvent();

            // 示例：定时器
            ExampleTimer();

            // 示例：资源加载
            ExampleResource();

            // 示例：音频播放
            ExampleAudio();

            // 示例：数据存储
            ExampleData();

            // 示例：Tween动画
            ExampleTween();
        }

        private void ExampleEvent()
        {
            // 订阅事件
            GameEntry.Event.Subscribe<TestEvent>(OnTestEvent);

            // 发布事件
            GameEntry.Event.Publish(new TestEvent { Message = "Hello CoolGameFramework!" });
        }

        private void OnTestEvent(TestEvent evt)
        {
            Debug.Log($"Received event: {evt.Message}");
        }

        private void ExampleTimer()
        {
            // 延迟执行
            GameEntry.Timer.DelayCall(2f, () =>
            {
                Debug.Log("2 seconds later!");
            });

            // 循环执行
            GameEntry.Timer.LoopCall(1f, () =>
            {
                Debug.Log("Every second!");
            });
        }

        private void ExampleResource()
        {
            // 同步加载
            // GameObject prefab = GameEntry.Resource.Load<GameObject>("Prefabs/Player");

            // 异步加载
            // GameEntry.Resource.LoadAsync<GameObject>("Prefabs/Enemy", (asset) =>
            // {
            //     if (asset != null)
            //     {
            //         Instantiate(asset);
            //     }
            // });
        }

        private void ExampleAudio()
        {
            // 播放背景音乐
            // GameEntry.Audio.PlayMusic("BGM_Main");

            // 播放音效
            // GameEntry.Audio.PlaySFX("SFX_Click");
        }

        private void ExampleData()
        {
            // 保存数据
            SaveManager.SaveInt("PlayerLevel", 10);
            SaveManager.SaveString("PlayerName", "Hero");

            // 加载数据
            int level = SaveManager.LoadInt("PlayerLevel", 1);
            string name = SaveManager.LoadString("PlayerName", "Unknown");

            Debug.Log($"Player: {name}, Level: {level}");
        }

        private void ExampleTween()
        {
            // 移动动画
            // GameEntry.Tween.MoveTo(transform, new Vector3(5, 0, 0), 2f)
            //     .SetEase(TweenBase.EaseInOutQuad)
            //     .OnComplete(() => Debug.Log("Move completed!"));

            // 缩放动画
            // GameEntry.Tween.ScaleTo(transform, Vector3.one * 2f, 1f);

            // 旋转动画
            // GameEntry.Tween.RotateTo(transform, new Vector3(0, 180, 0), 1.5f);
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            GameEntry.Event.Unsubscribe<TestEvent>(OnTestEvent);
        }
    }

    // 测试事件
    public struct TestEvent
    {
        public string Message;
    }
}
