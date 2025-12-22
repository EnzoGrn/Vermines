using Fusion;

namespace Vermines.Gameplay {

    using Vermines.Core;

    public abstract class GameModeInitializer {

        #region Attributes

        private readonly GameplayMode _Mode;

        #endregion

        #region Getters

        protected GameplayMode Mode => _Mode;
        protected NetworkRunner Runner => _Mode.Runner;
        protected NetworkGame NetworkGame => _Mode.Context.NetworkGame;

        #endregion

        #region Methods

        protected GameModeInitializer(GameplayMode mode)
        {
            _Mode = mode;
        }

        public void Initialize(string data)
        {
            OnInitialize(data);
        }

        public void Activate()
        {
            OnActivate();
        }

        #endregion

        #region Events

        protected abstract void OnInitialize(string data);
        protected abstract void OnActivate();

        #endregion
    }
}
