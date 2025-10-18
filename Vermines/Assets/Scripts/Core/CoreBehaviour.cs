using UnityEngine;

namespace Vermines.Core {

    public abstract class CoreBehaviour : MonoBehaviour {

        public new string name
        {
            get
            {
                #if UNITY_EDITOR
                    if (!Application.isPlaying)
                        return base.name;
                #endif

                if (!_NameCached) {
                    _CachedName = base.name;
                    _NameCached = true;
                }

                return _CachedName;
            }
            set
            {
                if (string.CompareOrdinal(_CachedName, value) != 0) {
                    base.name   = value;
                    _CachedName = value;
                    _NameCached = true;
                }
            }
        }

        public new GameObject gameObject
        {
            get
            {
                #if UNITY_EDITOR
                    if (!Application.isPlaying)
                        return base.gameObject;
                #endif

                if (!_GameObjectCached) {
                    _CachedGameObject = base.gameObject;
                    _GameObjectCached = true;
                }

                return _CachedGameObject;
            }
        }

        public new Transform transform
        {
            get
            {
                #if UNITY_EDITOR
                    if (!Application.isPlaying)
                        return base.transform;
                #endif

                if (!_TransformCached) {
                    _CachedTransfom  = base.transform;
                    _TransformCached = true;
                }

                return _CachedTransfom;
            }
        }

        private string _CachedName;
        private bool   _NameCached;

        private GameObject _CachedGameObject;
        private bool       _GameObjectCached;

        private Transform _CachedTransfom;
        private bool      _TransformCached;
    }
}
