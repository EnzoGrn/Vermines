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
            try {
                foreach (var player in GameDataStorage.Instance.PlayerData)
                {
                    PlayerData data = player.Value;
                    PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player.Key];

                    Debug.LogWarning($"{player.Key} - {data.Nickname} - {data.IsConnected}");
                    Debug.LogWarning($"{data.Eloquence} - {data.Souls} - {data.Family}");

                    System.Text.StringBuilder stringBuilder = new();

                    foreach (var card in deck.Deck)
                        stringBuilder.Append($"{card.ID} ");
                    Debug.LogWarning(stringBuilder);
                }
            }
            catch (System.Exception) { }
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
            if (_Initializer.Initialize() == -1)
                return;
            _Initializer.DeckDistribution();

            Start = true;
        }

        /*public void FixedUpdate()
        {
            if (!Start)
                return;
            try {
                foreach (var player in GameDataStorage.Instance.PlayerData) {
                    PlayerData data = player.Value;
                    PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player.Key];

                    Debug.LogWarning($"{player.Key} - {data.Nickname} - {data.IsConnected}");
                    Debug.LogWarning($"{data.Eloquence} - {data.Souls} - {data.Family}");

                    System.Text.StringBuilder stringBuilder = new();

                    foreach (var card in deck.Deck)
                        stringBuilder.Append($"{card.ID} ");
                    Debug.LogWarning(stringBuilder);
                }
            } catch (System.Exception) {}
        }*/
    }
}
