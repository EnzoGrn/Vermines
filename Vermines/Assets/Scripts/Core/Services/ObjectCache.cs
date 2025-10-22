using System.Collections.Generic;
using System;
using UnityEngine;

namespace Vermines.Core.Services {

    using Vermines.Core.Scene;
    using Vermines.Extension;

    public sealed class ObjectCache : SceneService {

        #region Attributes

        [SerializeField]
        private bool _HideCachedObjectsInHierarchy = true;

        [SerializeField]
        private List<CacheObject> _PrecacheObjects;

        private readonly Dictionary<GameObject, Stack<GameObject>> _Cached = new();

        private readonly Dictionary<GameObject, GameObject> _Borrowed = new();

        private readonly List<DeferredReturn> _Deferred = new();

        private readonly Stack<DeferredReturn> _Pool = new();

        private readonly List<GameObject> _All = new();

        #endregion

        #region Getters & Setters

        public int CachedCount
        {
            get => _All.Count;
        }

        public int BorrowedCount
        {
            get => _Borrowed.Count;
        }

        public bool HideCachedObjects
        {
            get => _HideCachedObjectsInHierarchy;
        }

        public T Get<T>(T prefab, bool activate = true, bool createIfEmpty = true) where T : UnityEngine.Component
        {
            return Get(prefab, null, activate, createIfEmpty);
        }

        public GameObject Get(GameObject prefab, bool activate = true, bool createIfEmpty = true)
        {
            return Get(prefab, null, activate, createIfEmpty);
        }

        public T Get<T>(T prefab, Transform parent, bool activate = true, bool createIfEmpty = true) where T : UnityEngine.Component
        {
            GameObject instance = Get(prefab.gameObject, parent, activate, createIfEmpty);

            return instance != null ? instance.GetComponent<T>() : null;
        }

        public GameObject Get(GameObject prefab, Transform parent, bool activate = true, bool createIfEmpty = true)
        {
            if (!_Cached.TryGetValue(prefab, out Stack<GameObject> stack)) {
                stack = new();

                _Cached[prefab] = stack;
            }

            if (stack.Count == 0) {
                if (createIfEmpty)
                    CreateInstance(prefab);
                else {
                    Debug.LogWarningFormat("Prefab {0} not available in cache, returning NULL", prefab.name);

                    return null;
                }
            }
            GameObject instance = stack.Pop();

            _Borrowed[instance] = prefab;

            Transform instanceTransform = instance.transform;

            if (parent != null)
                instanceTransform.SetParent(parent, false);
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale    = Vector3.one;

            if (activate)
                instance.SetActive(true);

            #if UNITY_EDITOR
                if (_HideCachedObjectsInHierarchy)
                    instance.hideFlags &= ~HideFlags.HideInHierarchy;
            #endif

            return instance;
        }

        #endregion

        #region Methods

        public void Return(UnityEngine.Component component, bool deactivate = true)
        {
            Return(component.gameObject, deactivate);
        }

        public void Return(GameObject instance, bool deactivate = true)
        {
            if (deactivate)
                instance.SetActive(false);
            instance.transform.SetParent(null, false);

            _Cached[_Borrowed[instance]].Push(instance);
            _Borrowed.Remove(instance);

            #if UNITY_EDITOR
                if (_HideCachedObjectsInHierarchy)
                    instance.hideFlags |= HideFlags.HideInHierarchy;
            #endif
        }

        public void ReturnRange(List<GameObject> instances, bool deactivate = true)
        {
            foreach (GameObject instance in instances)
                Return(instance, deactivate);
        }

        public void ReturnDeferred(GameObject instance, float delay)
        {
            DeferredReturn toReturn = _Pool.Count > 0 ? _Pool.Pop() : new DeferredReturn();

            toReturn.GameObject = instance;
            toReturn.Delay      = delay;

            _Deferred.Add(toReturn);
        }

        public void Prepare(GameObject prefab, int desiredCount)
        {
            if (!_Cached.TryGetValue(prefab, out Stack<GameObject> stack)) {
                stack = new();

                _Cached[prefab] = stack;
            }

            while (stack.Count < desiredCount)
                CreateInstance(prefab);
        }

        private void CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, null, false);

            instance.name = prefab.name;

            instance.SetActive(false);

            _Cached[prefab].Push(instance);
            _All.Add(instance);

            #if UNITY_EDITOR
                if (_HideCachedObjectsInHierarchy)
                    instance.hideFlags |= HideFlags.HideInHierarchy;
            #endif
        }

        #endregion

        #region Interface

        protected override void OnInitialize()
        {
            foreach (CacheObject cacheObject in _PrecacheObjects) {
                _Cached[cacheObject.GameObject] = new Stack<GameObject>();

                for (int i = 0; i < cacheObject.Count; ++i)
                    CreateInstance(cacheObject.GameObject);
            }
        }

        protected override void OnDeinitialize()
        {
            foreach (var item in _Borrowed) {
                GameObject     go = item.Key;
                bool shouldReturn = go != null;

                foreach (var deferredItem in _Deferred) {
                    if (go == deferredItem.GameObject) {
                        shouldReturn = false;

                        break;
                    }
                }

                if (shouldReturn)
                    Debug.LogWarning($"Object {go.name} from cache was not returned and will be destroyed");
            }
            _Deferred.Clear();
            _Borrowed.Clear();
            _Cached.Clear();

            foreach (GameObject instance in _All)
                Destroy(instance);
            _All.Clear();
        }

        protected override void OnTick()
        {
            for (int i = _Deferred.Count; i-- > 0;) {
                DeferredReturn deferred = _Deferred[i];

                deferred.Delay -= Time.deltaTime;

                if (deferred.Delay > 0.0f)
                    continue;
                _Deferred.RemoveBySwap(i);

                Return(deferred.GameObject, true);

                deferred.Reset();

                _Pool.Push(deferred);
            }
        }

        #endregion

        #region Helpers

        [Serializable]
        private sealed class CacheObject {
            public int Count;
            public GameObject GameObject;
        }

        private sealed class DeferredReturn {
            public GameObject GameObject;
            public float Delay;

            public void Reset()
            {
                GameObject = null;
                Delay      = 0.0f;
            }
        }

        #endregion
    }
}
