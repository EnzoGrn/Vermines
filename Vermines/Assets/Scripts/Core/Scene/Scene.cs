using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Vermines.Core.Scene {

    public class Scene : CoreBehaviour {

        #region Attributes

        public bool ContextReady { get; private set; }
        public bool IsActive { get; private set; }

        public SceneContext Context => _Context;

        [SerializeField]
        private SceneContext _Context;

        [SerializeField]
        private bool _SelfInitialized;
        private bool _IsInitialized;

        private List<SceneService> _Services = new();

        #endregion

        #region Initialization

        public void Initialize()
        {
            if (_IsInitialized)
                return;
            if (!ContextReady)
                OnPrepareContext(_Context);
            OnInitialize();

            _IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!_IsInitialized)
                return;
            Deactivate();
            OnDeinitialize();

            ContextReady   = false;
            _IsInitialized = false;
        }

        public void PrepareContext()
        {
            OnPrepareContext(_Context);
        }

        #endregion

        #region MonoBehaviour Methods

        protected void Awake()
        {
            if (_SelfInitialized)
                Initialize();
        }

        protected IEnumerator Start()
        {
            if (!_IsInitialized)
                yield break;
            if (_SelfInitialized && !IsActive) { // UI cannot be initialized in Awake, Canvas elements need to awake first.
                AddService(_Context.UI);

                yield return Activate();
            }
        }

        protected virtual void Update()
        {
            if (!IsActive)
                return;
            OnTick();
        }

        protected virtual void LateUpdate()
        {
            if (!IsActive)
                return;
            OnLateTick();
        }

        protected void OnDestroy()
        {
            Deinitialize();
        }

        protected void OnApplicationQuit()
        {
            Deinitialize();
        }

        #endregion

        #region Methods

        public IEnumerator Activate()
        {
            if (!_IsInitialized)
                yield break;
            yield return OnActivate();

            IsActive = true;
        }

        private void Deactivate()
        {    
            if (!IsActive)
                return;
            OnDeactivate();

            IsActive = false;
        }

        public void Quit()
        {
            Deinitialize();

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        protected void AddService(SceneService service)
        {
            if (service == null) {
                Debug.LogError("Missing Service.");

                return;
            }

            if (_Services.Contains(service)) {
                Debug.LogError($"Service {service.gameObject.name} already exists.");

                return;
            }

            service.Initialize(this, Context);

            _Services.Add(service);
        }

        public T GetService<T>() where T : SceneService
        {
            for (int i = 0; i < _Services.Count; i++)
                if (_Services[i] is T service)
                    return service;
            return null;
        }

        protected virtual void CollectServices()
        {
            SceneService[] services = GetComponentsInChildren<SceneService>(true);

            foreach (SceneService service in services)
                AddService(service);
        }

        #endregion

        #region Events

        protected virtual void OnInitialize()
        {
            CollectServices();
        }

        protected virtual void OnDeinitialize()
        {
            for (int i = 0; i < _Services.Count; i++)
                _Services[i].Deinitialize();
            _Services.Clear();
        }

        protected virtual void OnPrepareContext(SceneContext context)
        {
            context.PlayerData      = Global.PlayerService.PlayerData;
            context.Settings        = Global.Settings;
            context.RuntimeSettings = Global.RuntimeSettings;

            context.HasInput  = true;
            context.IsVisible = true;

            ContextReady = true;
        }

        protected virtual IEnumerator OnActivate()
        {
            for (int i = 0; i < _Services.Count; i++)
                _Services[i].Activate();
            yield break;
        }

        protected virtual void OnDeactivate()
        {
            for (int i = 0; i < _Services.Count; i++)
                _Services[i].Deactivate();
        }

        protected virtual void OnTick()
        {
            for (int i = 0; i < _Services.Count; i++)
                _Services[i].Tick();
        }

        protected virtual void OnLateTick()
        {
            for (int i = 0; i < _Services.Count; i++)
                _Services[i].LateTick();
        }

        #endregion
    }
}
