using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using OMGG.Menu.Connection;
using OMGG.Network.Fusion;
using OMGG.DesignPattern;
using Fusion;

namespace Vermines {

    using Vermines.Config;
    using Vermines.Gameplay.Phases;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.Menu.Screen;
    using Vermines.Service;
    using Vermines.Menu.Connection.Element;

    public class GameManager : NetworkBehaviour {

        #region Private Properties
        private RoutineManager _routineManager;
        #endregion

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
            if (HasStateAuthority) {
                VerminesPlayerService service = FindFirstObjectByType<VerminesPlayerService>(FindObjectsInactive.Include);

                service.RPC_Gameplay();
            }
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
            if (_Initializer.InitializePlayers(Config) == -1)
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

            RPC_StartClientSideStuff();

            PhaseManager.Instance.OnStartPhases();
        }

        public void UnloadSceneForCinematic(List<string> sceneToUnload)
        {
            if (HasStateAuthority == false)
                return;
            foreach (var scene in sceneToUnload)
                Runner.UnloadScene(scene);
        }

        public async void ReturnToMenu()
        {
            // Get the Vermines Services
            VerminesPlayerService services = FindFirstObjectByType<VerminesPlayerService>(FindObjectsInactive.Include);

            if (services.IsCustomGame()) { // Custom game
                if (HasStateAuthority) { // Load the lobby
                    VerminesConnectionBehaviour connection = FindFirstObjectByType<VerminesConnectionBehaviour>(FindObjectsInactive.Include);
                    VMUI_PartyMenu                   party = FindFirstObjectByType<VMUI_PartyMenu>(FindObjectsInactive.Include);

                    await connection.ChangeScene(party.SceneRef);

                    RPC_ForceReturnToLobbyEveryone();
                } else
                    ReturnToCustomTavern();
            } else { // Matchmaking game
                if (HasStateAuthority) // When you are the host and you leave for disconnect everyone because you close the server.
                    RPC_ForceReturnToTavernEveryone();
                else // Local leave.
                    ReturnToTavern();
            }
        }

        private async void ReturnToTavern()
        {
            // Put everyone on the loading screen
            VMUI_Loading loading = FindFirstObjectByType<VMUI_Loading>(FindObjectsInactive.Include);

            loading.Controller.Show<VMUI_Loading>();

            // If you are the host, unload the scenes.
            await SceneManager.UnloadSceneAsync("FinalAnimation");
            await SceneManager.UnloadSceneAsync("Game");

            // Disconnect
            await loading.Connection.DisconnectAsync(ConnectFailReason.GameEnded);

            // Switch to the tavern UI.
            loading.Controller.Show<VMUI_Tavern>(loading);
        }

        private async void ReturnToCustomTavern()
        {
            // Put everyone on the loading screen
            VMUI_Loading loading = FindFirstObjectByType<VMUI_Loading>(FindObjectsInactive.Include);

            loading.Controller.Show<VMUI_Loading>();

            // If you are the host, unload the scenes.
            await SceneManager.UnloadSceneAsync("FinalAnimation");
            await SceneManager.UnloadSceneAsync("Game");

            // Switch to the tavern UI.
            loading.Controller.Show<VMUI_CustomTavern>(loading);
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

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ForceReturnToTavernEveryone()
        {
            ReturnToTavern();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ForceReturnToLobbyEveryone()
        {
            ReturnToCustomTavern();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_StartClientSideStuff()
        {
            _routineManager = FindFirstObjectByType<RoutineManager>();

            if (_routineManager)
            {
                _routineManager.StartRoutine();
            }
            else
            {
                Debug.LogError("[CamManager]: Cannot find RoutineManager in the scene, please add it to the scene.");
            }
        }

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
        public void RPC_DiscardCardNoEffect(int playerId, int cardID)
        {
            Player.PlayerController.Local.RPC_DiscardCardNoEffect(playerId, cardID);
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
        public void RPC_NetworkEventCardEffect(int playerID, int cardID, string data)
        {
            Player.PlayerController.Local.RPC_NetworkEventCardEffect(playerID, cardID, data);
        }

        #endregion
    }
}
