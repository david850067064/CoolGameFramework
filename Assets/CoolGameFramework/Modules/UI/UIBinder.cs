using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// UI组件绑定器
    /// </summary>
    public class UIBinder : MonoBehaviour
    {
        [System.Serializable]
        public class BindData
        {
            public string Name;
            public GameObject GameObject;
        }

        public List<BindData> BindList = new List<BindData>();

        private Dictionary<string, GameObject> _bindDict;

        private void Awake()
        {
            _bindDict = new Dictionary<string, GameObject>();
            foreach (var data in BindList)
            {
                if (!string.IsNullOrEmpty(data.Name) && data.GameObject != null)
                {
                    _bindDict[data.Name] = data.GameObject;
                }
            }
        }

        /// <summary>
        /// 获取绑定的GameObject
        /// </summary>
        public GameObject Get(string name)
        {
            _bindDict.TryGetValue(name, out GameObject obj);
            return obj;
        }

        /// <summary>
        /// 获取绑定的组件
        /// </summary>
        public T Get<T>(string name) where T : Component
        {
            GameObject obj = Get(name);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        /// <summary>
        /// 获取Text组件
        /// </summary>
        public Text GetText(string name)
        {
            return Get<Text>(name);
        }

        /// <summary>
        /// 获取Image组件
        /// </summary>
        public Image GetImage(string name)
        {
            return Get<Image>(name);
        }

        /// <summary>
        /// 获取Button组件
        /// </summary>
        public Button GetButton(string name)
        {
            return Get<Button>(name);
        }

        /// <summary>
        /// 获取Transform组件
        /// </summary>
        public Transform GetTransform(string name)
        {
            return Get<Transform>(name);
        }
    }
}
