using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using OMGG.Menu.Connection;
using OMGG.Network.Fusion;
using Fusion;

namespace Vermines {

    using Vermines.Gameplay.Phases;
    using Vermines.Menu.Screen;
    using Vermines.Service;
    using Vermines.Menu.Connection.Element;
    using Vermines.Configuration.Network;
    using Vermines.Configuration;
    using Vermines.Gameplay.Phases.Enumerations;
    using OMGG.Chronicle;

    public partial class GameManager : NetworkBehaviour {

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

        public ChronicleManager ChronicleManager = new();

        #region Override Methods

        public override void Spawned()
        {
            if (Runner.Mode == SimulationModes.Server)
                Application.targetFrameRate = TickRate.Resolve(Runner.Config.Simulation.TickRateSelection).Server;
            ChronicleManager.OnChronicleAdded += (entry) => Debug.Log($"[Chronicle]: {entry.Id} - {entry.TitleKey}: {entry.MessageKey} {entry.PayloadJson}"); // Some entry can already have their payload when they are adds.
            ChronicleManager.OnChronicleUpdated += (entry) => Debug.Log($"[Chronicle]: {entry.Id} - {entry.PayloadJson}");
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

        public PlayerRef GetCurrentPlayer()
        {
            return PlayerTurnOrder.Get(CurrentPlayerIndex);
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
            OnSettingsDataChanged();
            InitializePlayerOrder();

            _Initializer.InitializePlayers(SettingsData);

            if (_Initializer.DeckDistribution(Rand) == -1)
                return; // TODO: Handle error.
            if (_Initializer.InitializeShop(SettingsData.Seed) == -1)
                return; // TODO: Handle error.
            _Initializer.StartingDraw(SettingsData.NumberOfCardsToStartWith);

            Start = true;

            RPC_StartClientSideStuff();

            PhaseManager.Instance.OnStartPhases();
        }

        public PhaseType GetCurrentPhase()
        {
            return PhaseManager.Instance.CurrentPhase;
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
                else {
                    await ReturnToTavern();
                }
            }
        }

        public async Task ForceEndCustomGame(PlayerRef player)
        {
            if (HasStateAuthority) { // Load the lobby
                VerminesConnectionBehaviour connection = FindFirstObjectByType<VerminesConnectionBehaviour>(FindObjectsInactive.Include);
                VMUI_PartyMenu              party      = FindFirstObjectByType<VMUI_PartyMenu>(FindObjectsInactive.Include);

                await connection.ChangeScene(party.SceneRef);

                SettingsManager settingsManager = FindFirstObjectByType<SettingsManager>(FindObjectsInactive.Include);

                if (settingsManager) { // Copy the current settings configuration for the next game and create a new seed.
                    GameSettingsData settings = SettingsData;

                    settings.Seed = GameConfiguration.CreateSeed();

                    settingsManager.SetConfiguration(settings);
                } else {
                    Debug.LogError("[GameManager]: Cannot find SettingsManager. We cannot save this game settings to the custom game lobby.");
                }
            }

            await ReturnToCustomTavern();
        }

        public async Task ReturnToTavern()
        {
            if (_routineManager)
                _routineManager.StopRoutine();

            // Put everyone on the loading screen
            VMUI_Loading loading = FindFirstObjectByType<VMUI_Loading>(FindObjectsInactive.Include);

            loading.Controller.Show<VMUI_Loading>();

            if (!HasStateAuthority)
                await SceneUtils.SafeUnloadAll("FinalAnimation", "Game", "UI", "GameplayCameraTravelling");

            // Disconnect
            await loading.Connection.DisconnectAsync(ConnectFailReason.GameEnded);
        }

        private async Task ReturnToCustomTavern()
        {
            if (_routineManager)
                _routineManager.StopRoutine();

            // Put everyone on the loading screen
            VMUI_Loading loading = FindFirstObjectByType<VMUI_Loading>(FindObjectsInactive.Include);

            loading.Controller.Show<VMUI_Loading>();

            // If you are the host, unload the scenes.
            if (HasStateAuthority)
                await SceneUtils.SafeUnloadAll(Runner, "FinalAnimation", "Game", "UI", "GameplayCameraTravelling");

            // Switch to the tavern UI.
            loading.Controller.Show<VMUI_CustomTavern>(loading);
        }

        public static async Task SafeUnload(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.isLoaded)
                await SceneManager.UnloadSceneAsync(sceneName);
        }

        private void InitializePlayerOrder()
        {
            // 1. Get all players
            List<PlayerRef> players = new();

            foreach (var kvp in GameDataStorage.Instance.PlayerData)
                players.Add(kvp.Key);
            // 2. Shuffle with the game seed.
            for (int i = players.Count - 1; i > 0; i--) {
                int k = Rand.Next(i + 1);

                (players[i], players[k]) = (players[k], players[i]);
            }

            // 3. Fill the NetworkArray
            for (int i = 0; i < players.Count; i++)
                PlayerTurnOrder.Set(i, players[i]);
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
            // Get the skyboxEvolution componennt to set it up and bind listeners
            Debug.Log("[GameManager]: Initialise SkyboxEvolution...");
            FindAnyObjectByType<SkyboxEvolution>(FindObjectsInactive.Include)?.InitSkyboxSettings();

            PhaseManager.Instance.Initialize();

            _routineManager = FindFirstObjectByType<RoutineManager>();

            if (_routineManager)
                _routineManager.StartRoutine();
            else
                Debug.LogError("[GameManager]: Cannot find RoutineManager in the scene, please add it to the scene.");
        }

        #endregion
    }
}
