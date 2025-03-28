using OMGG.Network.Fusion;
using UnityEngine;
using Fusion;

namespace Vermines {
    using OMGG.DesignPattern;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.Config;
    using Vermines.Gameplay.Phases;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;

    public class GameManager : NetworkBehaviour {

        #region Editor

        [SerializeField]
        private GameInitializer _Initializer;

        #endregion

        #region Singleton

        public static GameManager Instance => NetworkSingleton<GameManager>.Instance;

        #endregion

        #region Game Rules

        public GameConfiguration Config;

        public void SetNewConfiguration(GameConfiguration newConfig)
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

        #endregion

        #region Player Order

        // TODO: Change depending of the number of players max (possible in the settings)
        // TODO: Currently the game initialize in order of connexion, maybe create a random of the first player and next etc...
        [Networked, Capacity(4)]
        public NetworkArray<PlayerRef> PlayerTurnOrder { get; }

        /// <summary>
        /// The total amount of turn that has been played.
        /// </summary>
        [Networked]
        public int TotalTurnPlayed { get; set; } = 0;

        /// <summary>
        /// Do PlayerTurnOrder[CurrentPlayerIndex] to get the current player.
        /// </summary>
        [Networked]
        public int CurrentPlayerIndex { get; set; } = 0;

        public bool IsMyTurn()
        {
            return (PlayerTurnOrder.Get(CurrentPlayerIndex) == Runner.LocalPlayer);
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
            // TODO: need to handle it with the Waiting Room (before sending the Game Configuration to clients). // WIP
            if (Config.RandomSeed.Value == true)
                Config.Seed = Random.Range(0, int.MaxValue);
            if (_Initializer.InitializePlayers(Config.Seed, Config.EloquenceToStartWith.Value) == -1)
                return;
            InitializePlayerOrder();
            if (_Initializer.InitalizePhase() == -1)
                return;
            if (_Initializer.DeckDistribution(Config.Rand) == -1)
                return;
            if (_Initializer.InitializeShop(Config.Seed) == -1)
                return;
            _Initializer.StartingDraw(Config.NumberOfCardsToStartWith.Value);

            Start = true;

            PhaseManager.Instance.OnStartPhases();
        }

        private void InitializePlayerOrder()
        {
            int orderIndex = 0;

            foreach (var playerData in GameDataStorage.Instance.PlayerData) {
                PlayerTurnOrder.Set(orderIndex, playerData.Key);

                orderIndex++;
            }
        }

        #region Rpcs

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_BuyCard(ShopType shopType, int slot, int playerId)
        {
            BuyParameters parameters = new()
            {
                Decks = GameDataStorage.Instance.PlayerDeck,
                Player = PlayerRef.FromEncoded(playerId),
                Shop = GameDataStorage.Instance.Shop,
                ShopType = shopType,
                Slot = slot
            };
            
            ICommand buyCommand = new CheckBuyCommand(parameters);

            CommandResponse response = CommandInvoker.ExecuteCommand(buyCommand);

            if (response.Status == CommandStatus.Success)
                Player.PlayerController.Local.RPC_BuyCard(playerId, shopType, slot);
            else
                Debug.LogWarning($"[SERVER]: {response.Message}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            Player.PlayerController.Local.RPC_CardPlayed(playerId, cardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DiscardCard(int playerId, int cardID)
        {
            Player.PlayerController.Local.RPC_DiscardCard(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CardSacrified(int playerId, int cardId)
        {
            Player.PlayerController.Local.RPC_CardSacrified(playerId, cardId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ActivateEffect(int playerID, int cardID)
        {
            Player.PlayerController.Local.RPC_ActivateEffect(playerID, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ReplaceCardInShop(int playerId, ShopType shopType, int slot)
        {
            Player.PlayerController.Local.RPC_ReplaceCardInShop(playerId, shopType, slot);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ReducedInSilenced(int playerId, int cardToBeSilenced)
        {
            Player.PlayerController.Local.RPC_ReducedInSilenced(playerId, cardToBeSilenced);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveReducedInSilenced(int playerId, int cardID, int originalSouls)
        {
            Player.PlayerController.Local.RPC_RemoveReducedInSilenced(playerId, cardID, originalSouls);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CopiedEffect(int playerId, int cardID, int cardToCopiedID)
        {
            Player.PlayerController.Local.RPC_CopiedEffect(playerId, cardID, cardToCopiedID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RemoveCopiedEffect(int playerId, int cardID)
        {
            Player.PlayerController.Local.RPC_RemoveCopiedEffect(playerId, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_NetworkEventCardEffect(int playerID, int cardID)
        {
            Player.PlayerController.Local.RPC_NetworkEventCardEffect(playerID, cardID);
        }

        #endregion
    }
}
