namespace Vermines.Core.Scene {

    public class SceneService : CoreBehaviour {

        #region Attributes

        public SceneContext Context => _Context;
        private SceneContext _Context;

        public Scene Scene => _Scene;
        private Scene _Scene;

        public bool IsActive => _IsActive;
        private bool _IsActive;

        public bool IsInitialize => _IsInitialize;
        private bool _IsInitialize;

        #endregion

        #region Internal

        internal void Initialize(Scene scene, SceneContext context)
        {
            if (_IsInitialize)
                return;
            _Scene   = scene;
            _Context = context;

            OnInitialize();

            _IsInitialize = true;
        }

        internal void Deinitialize()
        {
            if (!_IsInitialize)
                return;
            Deactivate();
            OnDeactivate();

            _Scene        = null;
            _Context      = null;
            _IsInitialize = false;
        }

        internal void Activate()
        {
            if (!_IsInitialize || _IsActive)
                return;
            OnActivate();

            _IsActive = true;
        }

        internal void Deactivate()
        {
            if (!_IsActive)
                return;
            OnDeactivate();

            _IsActive = false;
        }

        internal void Tick()
        {
            if (!_IsActive)
                return;
            OnTick();
        }

        internal void LateTick()
        {
            if (!_IsActive)
                return;
            OnLateTick();
        }

        #endregion

        #region Interface

        protected virtual void OnInitialize() {}
        protected virtual void OnDeinitialize() {}
        protected virtual void OnActivate() {}
        protected virtual void OnDeactivate() {}
        protected virtual void OnTick() {}
        protected virtual void OnLateTick() {}

        #endregion
    }
}
