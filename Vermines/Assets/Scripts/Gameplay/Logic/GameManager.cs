using OMGG.Network.Fusion;
using UnityEngine;
using Fusion;

namespace Vermines {

    using Vermines.Player;

    public class GameManager : NetworkBehaviour {

        #region Editor

        [SerializeField]
        private GameInitializer _Initializer;

        #endregion

        #region Singleton

        public static GameManager Instance => NetworkSingleton<GameManager>.Instance;

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

        [Networked]
        public int Seed
        {
            get => default;
            set { }
        }

        public void StartGame()
        {
            if (HasStateAuthority == false)
                return;
            if (_Initializer.Initialize() == -1)
                return;
            _Initializer.DeckDistribution();
            _Initializer.StartingDraw();

            Start = true;
        }
    }
}
