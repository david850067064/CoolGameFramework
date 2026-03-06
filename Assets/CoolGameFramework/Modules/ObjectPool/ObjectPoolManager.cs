using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 对象池管理器
    /// </summary>
    public class ObjectPoolManager : Core.ModuleBase
    {
        public override int Priority => 5;

        private readonly Dictionary<string, GameObjectPool> _gameObjectPools = new Dictionary<string, GameObjectPool>();
        private readonly Dictionary<System.Type, object> _objectPools = new Dictionary<System.Type, object>();
        private Transform _poolRoot;

        public override void OnInit()
        {
            _poolRoot = new GameObject("ObjectPoolRoot").transform;
            GameObject.DontDestroyOnLoad(_poolRoot.gameObject);
        }

        /// <summary>
        /// 创建GameObject对象池
        /// </summary>
        public GameObjectPool CreateGameObjectPool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 100)
        {
            if (_gameObjectPools.ContainsKey(poolName))
            {
                Debug.LogWarning($"GameObject pool {poolName} already exists!");
                return _gameObjectPools[poolName];
            }

            GameObject poolObj = new GameObject($"Pool_{poolName}");
            poolObj.transform.SetParent(_poolRoot);

            GameObjectPool pool = new GameObjectPool(prefab, poolObj.transform, initialSize, maxSize);
            _gameObjectPools.Add(poolName, pool);

            return pool;
        }

        /// <summary>
        /// 获取GameObject对象池
        /// </summary>
        public GameObjectPool GetGameObjectPool(string poolName)
        {
            _gameObjectPools.TryGetValue(poolName, out GameObjectPool pool);
            return pool;
        }

        /// <summary>
        /// 从对象池获取GameObject
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position = default, Quaternion rotation = default)
        {
            if (_gameObjectPools.TryGetValue(poolName, out GameObjectPool pool))
            {
                return pool.Spawn(position, rotation);
            }

            Debug.LogError($"GameObject pool {poolName} not found!");
            return null;
        }

        /// <summary>
        /// 回收GameObject到对象池
        /// </summary>
        public void Despawn(string poolName, GameObject obj)
        {
            if (_gameObjectPools.TryGetValue(poolName, out GameObjectPool pool))
            {
                pool.Despawn(obj);
            }
        }

        /// <summary>
        /// 创建普通对象池
        /// </summary>
        public ObjectPool<T> CreateObjectPool<T>(System.Func<T> createFunc, System.Action<T> onGet = null, System.Action<T> onRelease = null, int initialSize = 10) where T : class
        {
            System.Type type = typeof(T);
            if (_objectPools.ContainsKey(type))
            {
                return _objectPools[type] as ObjectPool<T>;
            }

            ObjectPool<T> pool = new ObjectPool<T>(createFunc, onGet, onRelease, initialSize);
            _objectPools.Add(type, pool);
            return pool;
        }

        /// <summary>
        /// 获取普通对象池
        /// </summary>
        public ObjectPool<T> GetObjectPool<T>() where T : class
        {
            System.Type type = typeof(T);
            if (_objectPools.TryGetValue(type, out object pool))
            {
                return pool as ObjectPool<T>;
            }
            return null;
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _gameObjectPools.Values)
            {
                pool.Clear();
            }
            _gameObjectPools.Clear();
            _objectPools.Clear();
        }

        public override void OnDestroy()
        {
            ClearAll();
            if (_poolRoot != null)
            {
                GameObject.Destroy(_poolRoot.gameObject);
            }
        }
    }

    /// <summary>
    /// GameObject对象池
    /// </summary>
    public class GameObjectPool
    {
        private GameObject _prefab;
        private Transform _parent;
        private Queue<GameObject> _pool;
        private int _maxSize;

        public GameObjectPool(GameObject prefab, Transform parent, int initialSize, int maxSize)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            _pool = new Queue<GameObject>();

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = GameObject.Instantiate(_prefab, _parent);
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public GameObject Spawn(Vector3 position = default, Quaternion rotation = default)
        {
            GameObject obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = GameObject.Instantiate(_prefab, _parent);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(_parent);

            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(obj);
            }
            else
            {
                GameObject.Destroy(obj);
            }
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                GameObject obj = _pool.Dequeue();
                if (obj != null)
                {
                    GameObject.Destroy(obj);
                }
            }
        }
    }

    /// <summary>
    /// 普通对象池
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private Stack<T> _pool;
        private System.Func<T> _createFunc;
        private System.Action<T> _onGet;
        private System.Action<T> _onRelease;
        private System.Action<T> _onDestroy;
        private int _maxSize;

        public int Count => _pool.Count;

        public ObjectPool(System.Func<T> createFunc, System.Action<T> onGet = null, System.Action<T> onRelease = null, int initialSize = 10, int maxSize = 0, System.Action<T> onDestroy = null)
        {
            _createFunc = createFunc;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;
            _pool = new Stack<T>();

            for (int i = 0; i < initialSize; i++)
            {
                _pool.Push(_createFunc());
            }
        }

        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Pop() : _createFunc();
            _onGet?.Invoke(obj);
            return obj;
        }

        public void Release(T obj)
        {
            _onRelease?.Invoke(obj);
            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                _onDestroy?.Invoke(obj);
                return;
            }
            _pool.Push(obj);
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}
