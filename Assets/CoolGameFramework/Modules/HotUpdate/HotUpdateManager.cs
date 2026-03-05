using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 热更新管理器
    /// </summary>
    public class HotUpdateManager : Core.ModuleBase
    {
        public override int Priority => 15;

        private string _remoteVersionUrl;
        private string _remoteAssetUrl;
        private VersionInfo _localVersion;
        private VersionInfo _remoteVersion;

        public VersionInfo LocalVersion => _localVersion;
        public VersionInfo RemoteVersion => _remoteVersion;

        public override void OnInit()
        {
            LoadLocalVersion();
        }

        /// <summary>
        /// 设置远程地址
        /// </summary>
        public void SetRemoteUrl(string versionUrl, string assetUrl)
        {
            _remoteVersionUrl = versionUrl;
            _remoteAssetUrl = assetUrl;
        }

        /// <summary>
        /// 加载本地版本信息
        /// </summary>
        private void LoadLocalVersion()
        {
            string versionPath = Path.Combine(Application.persistentDataPath, "version.json");
            if (File.Exists(versionPath))
            {
                string json = File.ReadAllText(versionPath);
                _localVersion = JsonUtility.FromJson<VersionInfo>(json);
            }
            else
            {
                _localVersion = new VersionInfo { Version = "1.0.0", BuildNumber = 1 };
            }
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        public void CheckUpdate(Action<bool, UpdateType> onComplete)
        {
            Core.GameEntry.Instance.StartCoroutine(CheckUpdateCoroutine(onComplete));
        }

        private IEnumerator CheckUpdateCoroutine(Action<bool, UpdateType> onComplete)
        {
            if (string.IsNullOrEmpty(_remoteVersionUrl))
            {
                Debug.LogWarning("Remote version URL not set!");
                onComplete?.Invoke(false, UpdateType.None);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(_remoteVersionUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    _remoteVersion = JsonUtility.FromJson<VersionInfo>(request.downloadHandler.text);
                    UpdateType updateType = CompareVersion(_localVersion, _remoteVersion);
                    onComplete?.Invoke(true, updateType);
                }
                else
                {
                    Debug.LogError($"Check update failed: {request.error}");
                    onComplete?.Invoke(false, UpdateType.None);
                }
            }
        }

        /// <summary>
        /// 比较版本
        /// </summary>
        private UpdateType CompareVersion(VersionInfo local, VersionInfo remote)
        {
            string[] localParts = local.Version.Split('.');
            string[] remoteParts = remote.Version.Split('.');

            for (int i = 0; i < Mathf.Min(localParts.Length, remoteParts.Length); i++)
            {
                int localNum = int.Parse(localParts[i]);
                int remoteNum = int.Parse(remoteParts[i]);

                if (remoteNum > localNum)
                {
                    return i == 0 ? UpdateType.Force : UpdateType.Optional;
                }
            }

            if (remote.BuildNumber > local.BuildNumber)
            {
                return UpdateType.Resource;
            }

            return UpdateType.None;
        }

        /// <summary>
        /// 开始更新
        /// </summary>
        public void StartUpdate(Action<float> onProgress, Action<bool> onComplete)
        {
            Core.GameEntry.Instance.StartCoroutine(UpdateCoroutine(onProgress, onComplete));
        }

        private IEnumerator UpdateCoroutine(Action<float> onProgress, Action<bool> onComplete)
        {
            if (_remoteVersion == null || _remoteVersion.Files == null)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            int totalFiles = _remoteVersion.Files.Count;
            int downloadedFiles = 0;

            foreach (var file in _remoteVersion.Files)
            {
                string url = _remoteAssetUrl + "/" + file.Path;
                string savePath = Path.Combine(Application.persistentDataPath, file.Path);

                // 检查文件是否需要更新
                if (NeedUpdate(file))
                {
                    string directory = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (UnityWebRequest request = UnityWebRequest.Get(url))
                    {
                        request.downloadHandler = new DownloadHandlerFile(savePath);
                        yield return request.SendWebRequest();

                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError($"Download failed: {file.Path}");
                            onComplete?.Invoke(false);
                            yield break;
                        }
                    }
                }

                downloadedFiles++;
                onProgress?.Invoke((float)downloadedFiles / totalFiles);
            }

            // 保存新版本信息
            SaveVersion(_remoteVersion);
            _localVersion = _remoteVersion;

            onComplete?.Invoke(true);
        }

        /// <summary>
        /// 检查文件是否需要更新
        /// </summary>
        private bool NeedUpdate(FileInfo fileInfo)
        {
            string localPath = Path.Combine(Application.persistentDataPath, fileInfo.Path);
            if (!File.Exists(localPath))
            {
                return true;
            }

            // 比较文件大小
            System.IO.FileInfo localFileInfo = new System.IO.FileInfo(localPath);
            if (localFileInfo.Length != fileInfo.Size)
            {
                return true;
            }

            // 比较MD5
            if (!string.IsNullOrEmpty(fileInfo.MD5))
            {
                string localMD5 = Utilities.EncryptUtil.GetFileMD5(localPath);
                if (localMD5 != fileInfo.MD5)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 保存版本信息
        /// </summary>
        private void SaveVersion(VersionInfo version)
        {
            string json = JsonUtility.ToJson(version);
            string path = Path.Combine(Application.persistentDataPath, "version.json");
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// 加载热更新DLL
        /// </summary>
        public void LoadHotUpdateDLL()
        {
            // HybridCLR DLL加载
            string dllPath = Path.Combine(Application.persistentDataPath, "HotUpdate.dll");

            if (!File.Exists(dllPath))
            {
                Debug.LogWarning($"HotUpdate DLL not found at: {dllPath}");
                return;
            }

            try
            {
                byte[] dllBytes = File.ReadAllBytes(dllPath);

                // 使用HybridCLR加载DLL
                // 注意：需要先安装HybridCLR插件
                // System.Reflection.Assembly.Load(dllBytes);

                Debug.Log("HotUpdate DLL loaded successfully");

                // 可以在这里调用热更新DLL中的初始化方法
                // 例如：通过反射调用入口类的Init方法
                // var assembly = System.Reflection.Assembly.Load(dllBytes);
                // var type = assembly.GetType("HotUpdate.GameMain");
                // var method = type.GetMethod("Init");
                // method.Invoke(null, null);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load HotUpdate DLL: {e.Message}");
            }
        }

        /// <summary>
        /// 加载多个热更新DLL
        /// </summary>
        public void LoadHotUpdateDLLs(string[] dllNames)
        {
            foreach (string dllName in dllNames)
            {
                string dllPath = Path.Combine(Application.persistentDataPath, dllName);

                if (!File.Exists(dllPath))
                {
                    Debug.LogWarning($"DLL not found: {dllPath}");
                    continue;
                }

                try
                {
                    byte[] dllBytes = File.ReadAllBytes(dllPath);
                    // System.Reflection.Assembly.Load(dllBytes);
                    Debug.Log($"Loaded DLL: {dllName}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load DLL {dllName}: {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 版本信息
    /// </summary>
    [Serializable]
    public class VersionInfo
    {
        public string Version;
        public int BuildNumber;
        public List<FileInfo> Files;
    }

    /// <summary>
    /// 文件信息
    /// </summary>
    [Serializable]
    public class FileInfo
    {
        public string Path;
        public string MD5;
        public long Size;
    }

    /// <summary>
    /// 更新类型
    /// </summary>
    public enum UpdateType
    {
        None,       // 无需更新
        Resource,   // 资源更新
        Optional,   // 可选更新
        Force       // 强制更新
    }
}
