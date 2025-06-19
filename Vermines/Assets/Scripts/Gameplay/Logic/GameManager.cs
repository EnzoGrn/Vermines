using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using OMGG.Menu.Connection;
using OMGG.Network.Fusion;
using OMGG.DesignPattern;
using Fusion;

namespace Vermines {

    using Vermines.Gameplay.Phases;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.Menu.Screen;
    using Vermines.Service;
    using Vermines.Menu.Connection.Element;
    using Vermines.Configuration.Network;
    using Vermines.Configuration;
    using System.Collections;

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

        public GameConfiguration Configuration;

        [Networked, OnChangedRender(nameof(OnSettingsDataChanged))]
        public GameSettingsData SettingsData { get; set; }

        public System.Random Rand { get; set; } = new(0);

        private void OnSettingsDataChanged()
        {
            Rand = new System.Random(SettingsData.Seed);
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

        public void WaitAndStartGame(float waitTime = 0.5f)
        {
            if (HasStateAuthority == false)
                return;
            if (Start)
                return;
            CancelInvoke(nameof(StartGame));
            Invoke(nameof(StartGame), waitTime);
        }

        public void StartGame()
        {
            if (HasStateAuthority == false)
                return;
            if (SettingsData.Equals(default(GameSettingsData))) // If it's a default value (not a custom game), then load the default game configuration.
                SettingsData = Configuration.ToGameSettingsData();
            if (_Initializer.InitializePlayers(SettingsData) == -1)
                return;
            InitializePlayerOrder();
            if (_Initializer.InitalizePhase() == -1)
                return;
            if (_Initializer.DeckDistribution(Rand) == -1)
                return;
            if (_Initializer.InitializeShop(SettingsData.Seed) == -1)
                return;
            _Initializer.StartingDraw(SettingsData.NumberOfCardsToStartWith);

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

        public async Task ReturnToMenu()
        {
            // Get the Vermines Services
            VerminesPlayerService services = FindFirstObjectByType<VerminesPlayerService>(FindObjectsInactive.Include);

            if (services.IsCustomGame()) { // Custom game
                if (HasStateAuthority) { // Load the lobby
                    VerminesConnectionBehaviour connection = FindFirstObjectByType<VerminesConnectionBehaviour>(FindObjectsInactive.Include);
                    VMUI_PartyMenu                   party = FindFirstObjectByType<VMUI_PartyMenu>(FindObjectsInactive.Include);

                    await connection.ChangeScene(party.SceneRef);

                    SettingsManager settingsManager = FindFirstObjectByType<SettingsManager>(FindObjectsInactive.Include);

                    if (settingsManager) { // Copy the current settings configuration for the next game and create a new seed.
                        GameSettingsData settings = SettingsData;

                        settings.Seed = GameConfiguration.CreateSeed();

                        settingsManager.SetConfiguration(settings);
                    } else {
                        Debug.LogError(
                            "[GameManager]: Cannot find SettingsManager." +
                            "We cannot save this game settings to the custom game lobby."
                        );
                    }

                    RPC_ForceReturnToLobbyEveryone();
                } else
                    await ReturnToCustomTavern();
            } else { // Matchmaking game
                if (HasStateAuthority) // When you are the host and you leave for disconnect everyone because you close the server.
                    RPC_ForceReturnToTavernEveryone();
                else // Local leave.
                    await ReturnToTavern();
            }
        }

        private async Task ReturnToTavern()
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

        private async Task ReturnToCustomTavern()
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
        private async void RPC_ForceReturnToTavernEveryone()
        {
            await ReturnToTavern();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private async void RPC_ForceReturnToLobbyEveryone()
        {
            await ReturnToCustomTavern();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_StartClientSideStuff()
        {
            _routineManager = FindFirstObjectByType<RoutineManager>();

            if (_routineManager)
                _routineManager.StartRoutine();
            else
                Debug.LogError("[GameManager]: Cannot find RoutineManager in the scene, please add it to the scene.");
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
