using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 下载管理器
    /// </summary>
    public class DownloadManager : Core.ModuleBase
    {
        public override int Priority => 13;

        private readonly List<DownloadTask> _downloadQueue = new List<DownloadTask>();
        private readonly List<DownloadTask> _activeDownloads = new List<DownloadTask>();
        private int _maxConcurrentDownloads = 3;

        /// <summary>
        /// 设置最大并发下载数
        /// </summary>
        public void SetMaxConcurrentDownloads(int max)
        {
            _maxConcurrentDownloads = Mathf.Max(1, max);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public void DownloadFile(string url, string savePath, Action<float> onProgress = null, Action<bool> onComplete = null)
        {
            DownloadTask task = new DownloadTask
            {
                Url = url,
                SavePath = savePath,
                OnProgress = onProgress,
                OnComplete = onComplete
            };

            _downloadQueue.Add(task);
            ProcessQueue();
        }

        /// <summary>
        /// 处理下载队列
        /// </summary>
        private void ProcessQueue()
        {
            while (_activeDownloads.Count < _maxConcurrentDownloads && _downloadQueue.Count > 0)
            {
                DownloadTask task = _downloadQueue[0];
                _downloadQueue.RemoveAt(0);
                _activeDownloads.Add(task);
                Core.GameEntry.Instance.StartCoroutine(DownloadCoroutine(task));
            }
        }

        /// <summary>
        /// 下载协程
        /// </summary>
        private IEnumerator DownloadCoroutine(DownloadTask task)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(task.Url))
            {
                request.downloadHandler = new DownloadHandlerFile(task.SavePath);

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    task.OnProgress?.Invoke(operation.progress);
                    yield return null;
                }

                bool success = request.result == UnityWebRequest.Result.Success;
                task.OnComplete?.Invoke(success);

                if (!success)
                {
                    Debug.LogError($"Download failed: {task.Url}, Error: {request.error}");
                }

                _activeDownloads.Remove(task);
                ProcessQueue();
            }
        }

        /// <summary>
        /// 取消所有下载
        /// </summary>
        public void CancelAll()
        {
            _downloadQueue.Clear();
            _activeDownloads.Clear();
        }

        public override void OnDestroy()
        {
            CancelAll();
        }

        private class DownloadTask
        {
            public string Url;
            public string SavePath;
            public Action<float> OnProgress;
            public Action<bool> OnComplete;
        }
    }
}
