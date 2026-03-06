using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager : Core.ModuleBase
    {
        public override int Priority => 9;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private bool _isConnected;
        private byte[] _receiveBuffer = new byte[8192];
        private readonly object _lockObj = new object();

        public bool IsConnected => _isConnected;

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Connect(string host, int port, Action<bool> onResult)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.BeginConnect(host, port, (ar) =>
                {
                    try
                    {
                        _tcpClient.EndConnect(ar);
                        lock (_lockObj)
                        {
                            _networkStream = _tcpClient.GetStream();
                            _isConnected = true;
                        }
                        onResult?.Invoke(true);
                        BeginReceive();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Connect failed: {e.Message}");
                        lock (_lockObj)
                        {
                            _isConnected = false;
                        }
                        onResult?.Invoke(false);
                    }
                }, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"Connect error: {e.Message}");
                onResult?.Invoke(false);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            lock (_lockObj)
            {
                if (_networkStream != null)
                {
                    _networkStream.Close();
                    _networkStream = null;
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient = null;
                }

                _isConnected = false;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void Send(byte[] data)
        {
            if (!_isConnected || _networkStream == null)
            {
                Debug.LogWarning("Not connected to server!");
                return;
            }

            try
            {
                _networkStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"Send error: {e.Message}");
            }
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        public void SendString(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            Send(data);
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        private void BeginReceive()
        {
            if (!_isConnected || _networkStream == null) return;

            try
            {
                _networkStream.BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, OnReceive, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"BeginReceive error: {e.Message}");
            }
        }

        /// <summary>
        /// 接收数据回调
        /// </summary>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int length;
                NetworkStream stream;
                lock (_lockObj)
                {
                    if (_networkStream == null) return;
                    stream = _networkStream;
                    length = stream.EndRead(ar);
                }

                if (length > 0)
                {
                    byte[] data = new byte[length];
                    Array.Copy(_receiveBuffer, data, length);
                    OnDataReceived(data);
                    BeginReceive();
                }
                else
                {
                    Disconnect();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"OnReceive error: {e.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// 数据接收处理
        /// </summary>
        protected virtual void OnDataReceived(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            Debug.Log($"Received: {message}");
        }

        public override void OnDestroy()
        {
            Disconnect();
        }
    }

    /// <summary>
    /// HTTP管理器
    /// </summary>
    public class HttpManager
    {
        /// <summary>
        /// GET请求
        /// </summary>
        public static void Get(string url, Action<string> onSuccess, Action<string> onError)
        {
            Core.GameEntry.Instance.StartCoroutine(GetCoroutine(url, onSuccess, onError));
        }

        private static System.Collections.IEnumerator GetCoroutine(string url, Action<string> onSuccess, Action<string> onError)
        {
            using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    onError?.Invoke(request.error);
                }
            }
        }

        /// <summary>
        /// POST请求
        /// </summary>
        public static void Post(string url, string jsonData, Action<string> onSuccess, Action<string> onError)
        {
            Core.GameEntry.Instance.StartCoroutine(PostCoroutine(url, jsonData, onSuccess, onError));
        }

        private static System.Collections.IEnumerator PostCoroutine(string url, string jsonData, Action<string> onSuccess, Action<string> onError)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    onError?.Invoke(request.error);
                }
            }
        }
    }
}
