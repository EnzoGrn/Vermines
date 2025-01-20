using OMGG.Network.Fusion;
using UnityEngine;
using Fusion;

namespace Vermines {

    using Vermines.Config;

    public class GameManager : NetworkBehaviour {

        #region Editor

        [SerializeField]
        private GameInitializer _Initializer;

        #endregion

        #region Singleton

        public static GameManager Instance => NetworkSingleton<GameManager>.Instance;

        #endregion

        #region Game Rules

        public GameConfig Config;

        public void SetNewConfiguration(GameConfig newConfig)
        {
            // -- Check if the game is already started
            if (Start)
                return;
            Config = newConfig;
        }

        #endregion

        #region Override Methods

        public override void Spawned()
        {
            if (Runner.Mode == SimulationModes.Server)
                Application.targetFrameRate = TickRate.Resolve(Runner.Config.Simulation.TickRateSelection).Server;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false)
                return;
            if (!Start) {
                if (GameDataStorage.Instance.PlayerData.Count >= 2)
                    StartGame();
                return;
            }
        }

        #endregion

        [Networked]
        [HideInInspector]
        public bool Start
        {
            get => default;
            set { }
        }

        public void StartGame()
        {
            if (HasStateAuthority == false)
                return;
            Config.Seed = Random.Range(0, int.MaxValue);

            if (_Initializer.Initialize(Config.Seed, Config.FirstEloquence) == -1)
                return;
            _Initializer.DeckDistribution(Config.Rand);
            _Initializer.StartingDraw(Config.FirstDraw);

            Start = true;
        }
    }
}
