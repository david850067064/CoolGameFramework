using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : Core.ModuleBase
    {
        public override int Priority => 6;

        private readonly Dictionary<string, UIBase> _openedUIs = new Dictionary<string, UIBase>();
        private readonly List<UIBase> _uiStack = new List<UIBase>();
        private Transform _uiRoot;
        private Canvas _canvas;

        // UI层级
        private Transform _backgroundLayer;
        private Transform _normalLayer;
        private Transform _topLayer;
        private Transform _systemLayer;

        public override void OnInit()
        {
            InitializeUIRoot();
        }

        private void InitializeUIRoot()
        {
            GameObject uiRootObj = new GameObject("UIRoot");
            GameObject.DontDestroyOnLoad(uiRootObj);
            _uiRoot = uiRootObj.transform;

            // 创建Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(_uiRoot);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 创建UI层级
            _backgroundLayer = CreateLayer("BackgroundLayer", 0);
            _normalLayer = CreateLayer("NormalLayer", 100);
            _topLayer = CreateLayer("TopLayer", 200);
            _systemLayer = CreateLayer("SystemLayer", 300);
        }

        private Transform CreateLayer(string layerName, int sortingOrder)
        {
            GameObject layerObj = new GameObject(layerName);
            layerObj.transform.SetParent(_canvas.transform, false);

            Canvas canvas = layerObj.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            return layerObj.transform;
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        public T OpenUI<T>(string uiName, UILayer layer = UILayer.Normal) where T : UIBase
        {
            if (_openedUIs.TryGetValue(uiName, out UIBase existingUI))
            {
                existingUI.Show();
                return existingUI as T;
            }

            string path = $"UI/{uiName}";
            GameObject uiPrefab = Core.GameEntry.Resource.Load<GameObject>(path);
            if (uiPrefab == null)
            {
                Debug.LogError($"UI prefab not found: {path}");
                return null;
            }

            Transform parent = GetLayerTransform(layer);
            GameObject uiObj = GameObject.Instantiate(uiPrefab, parent);
            uiObj.name = uiName;

            T ui = uiObj.GetComponent<T>();
            if (ui == null)
            {
                ui = uiObj.AddComponent<T>();
            }

            ui.OnInit();
            ui.Show();

            _openedUIs[uiName] = ui;
            _uiStack.Add(ui);

            return ui;
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void CloseUI(string uiName)
        {
            if (_openedUIs.TryGetValue(uiName, out UIBase ui))
            {
                ui.Hide();
                ui.OnDestroy();
                GameObject.Destroy(ui.gameObject);
                _openedUIs.Remove(uiName);
                _uiStack.Remove(ui);
            }
        }

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAllUI()
        {
            foreach (var ui in _openedUIs.Values)
            {
                ui.Hide();
                ui.OnDestroy();
                GameObject.Destroy(ui.gameObject);
            }
            _openedUIs.Clear();
            _uiStack.Clear();
        }

        /// <summary>
        /// 获取UI
        /// </summary>
        public T GetUI<T>(string uiName) where T : UIBase
        {
            if (_openedUIs.TryGetValue(uiName, out UIBase ui))
            {
                return ui as T;
            }
            return null;
        }

        /// <summary>
        /// 检查UI是否打开
        /// </summary>
        public bool IsUIOpen(string uiName)
        {
            return _openedUIs.ContainsKey(uiName);
        }

        private Transform GetLayerTransform(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Background:
                    return _backgroundLayer;
                case UILayer.Normal:
                    return _normalLayer;
                case UILayer.Top:
                    return _topLayer;
                case UILayer.System:
                    return _systemLayer;
                default:
                    return _normalLayer;
            }
        }

        public override void OnDestroy()
        {
            CloseAllUI();
            if (_uiRoot != null)
            {
                GameObject.Destroy(_uiRoot.gameObject);
            }
        }
    }

    /// <summary>
    /// UI层级枚举
    /// </summary>
    public enum UILayer
    {
        Background,
        Normal,
        Top,
        System
    }

    /// <summary>
    /// UI基类
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        public virtual void OnInit() { }
        public virtual void Show() { gameObject.SetActive(true); }
        public virtual void Hide() { gameObject.SetActive(false); }
        public virtual void OnDestroy() { }
    }
}
