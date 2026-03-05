using System;
using System.Collections;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 场景管理器
    /// </summary>
    public class SceneManager : Core.ModuleBase
    {
        public override int Priority => 5;

        private string _currentSceneName;
        private bool _isLoading;

        /// <summary>
        /// 当前场景名称
        /// </summary>
        public string CurrentSceneName => _currentSceneName;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading => _isLoading;

        public override void OnInit()
        {
            _currentSceneName = UnitySceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning("Scene is already loading!");
                return;
            }

            UnitySceneManager.LoadScene(sceneName);
            _currentSceneName = sceneName;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        public void LoadSceneAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            if (_isLoading)
            {
                Debug.LogWarning("Scene is already loading!");
                return;
            }

            Core.GameEntry.Instance.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onProgress, onComplete));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action<float> onProgress, Action onComplete)
        {
            _isLoading = true;

            AsyncOperation operation = UnitySceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                onProgress?.Invoke(operation.progress);
                yield return null;
            }

            onProgress?.Invoke(1f);
            operation.allowSceneActivation = true;

            yield return operation;

            _currentSceneName = sceneName;
            _isLoading = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 重新加载当前场景
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(_currentSceneName);
        }

        /// <summary>
        /// 加载场景（附加模式）
        /// </summary>
        public void LoadSceneAdditive(string sceneName, Action onComplete = null)
        {
            Core.GameEntry.Instance.StartCoroutine(LoadSceneAdditiveCoroutine(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAdditiveCoroutine(string sceneName, Action onComplete)
        {
            AsyncOperation operation = UnitySceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            yield return operation;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public void UnloadScene(string sceneName, Action onComplete = null)
        {
            Core.GameEntry.Instance.StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
        }

        private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
        {
            AsyncOperation operation = UnitySceneManager.UnloadSceneAsync(sceneName);
            yield return operation;
            onComplete?.Invoke();
        }
    }
}
