using System.Collections.Generic;
using System.Linq;
using System;
using OMGG.Menu.Connection;
using OMGG.Menu.Screen;
using Fusion.Sockets;
using Fusion;
using UnityEngine;

namespace Vermines.Service {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Configuration.Network;
    using Vermines.Configuration;
    using Vermines.Menu.Connection.Element;
    using Vermines.Menu.Screen;
    using UnityEngine.SceneManagement;

    public class VerminesPlayerService : NetworkBehaviour, IPlayerLeft, INetworkRunnerCallbacks {

        private VerminesConnectionBehaviour _Connection;

        #region Players

        private const int PLAYERS_MAX_COUNT = 4; // ! Change this value if you edit it in the config file.

        /// <summary>
        /// The list of players usernames.
        /// </summary>
        [Networked, Capacity(PLAYERS_MAX_COUNT), OnChangedRender(nameof(OnPlayersChange))]
        private NetworkDictionary<PlayerRef, NetworkString<_16>> _PlayersUsernames => default;

        public List<string> GetPlayersUsernames()
        {
            List<string> playersList = new();

            foreach (var pair in _PlayersUsernames)
                playersList.Add(pair.Value.Value);
            return playersList;
        }

        private void RemovePlayer(PlayerRef player)
        {
            _PlayersUsernames.Remove(player);
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (Object.HasStateAuthority)
                RemovePlayer(player);
            // TODO: Alert the current game or lobby selection that someone left the game.
        }

        #endregion

        #region Methods

        /// <summary>
        /// Function that checks if the current game is a custom game or not.
        /// </summary>
        /// <returns>
        /// True if the game is a custom game, false otherwise.
        /// </returns>
        public bool IsCustomGame()
        {
            if (_Connection == null)
                _Connection = FindFirstObjectByType<VerminesConnectionBehaviour>(FindObjectsInactive.Include);
            return _Connection != null && _Connection.IsCustomLobby;
        }

        /// <summary>
        /// Function that get the username and cultist selected by the player to give it to the game.
        /// </summary>
        private void AddPlayerMatchmaking()
        {
            // Those lines are here to ensure everyone has the same screen at the same time.
            if (Object.HasStateAuthority)
                RPC_Gameplay();
            else
                SwitchScreen<VMUI_Gameplay>();

            // Get the tavern to access the local player's selected cultist.
            VMUI_Tavern tavern = FindFirstObjectByType<VMUI_Tavern>(FindObjectsInactive.Include);

            if (tavern && tavern.SelectedCultist != null)
                RPC_UpdatePlayerState(Runner.LocalPlayer, PlayerPrefs.GetString("OMGG.Profile.Username"), tavern.SelectedCultist.family);
            else // -> This condition is normaly called only by the MPPM clients (Multiplayer Player Play Mode), Editor side.
                RPC_UpdatePlayerState(Runner.LocalPlayer, PlayerPrefs.GetString("OMGG.Profile.Username"));
        }

        /// <summary>
        /// Function that initializes a player in a custom game.
        /// </summary>
        /// <remarks>
        /// When the player join a custom lobby, they don't have selected a cultist yet.
        /// </remarks>
        private void AddPlayerCustom()
        {
            // Those lines are here to ensure everyone has the same screen at the same time.
            if (Object.HasStateAuthority)
                RPC_NetworkTavern();
            else
                SwitchScreen<VMUI_CustomTavern>();
            RPC_UpdatePlayerState(Runner.LocalPlayer, PlayerPrefs.GetString("OMGG.Profile.Username"));
        }

        /// <summary>
        /// Function that switches the current screen to the specified type of screen.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the screen to switch to. Must be a subclass of <see cref="MenuUIScreen" />.
        /// </typeparam>
        public void SwitchScreen<T>() where T : MenuUIScreen
        {
            T screen = FindFirstObjectByType<T>(FindObjectsInactive.Include);

            if (screen != null)
                screen.Controller.Show<T>();
            else
                Debug.LogError($"[VerminesPlayerService] SwitchScreen() - Screen of type {typeof(T).Name} not found.");
        }

        public void Update()
        {
            UpdateMatchmaking();
        }

        #endregion

        #region Matchmaking Logics

        [SerializeField]
        private float _MatchmakingCountDownDelay = 2f;

        [SerializeField]
        private float _TimeoutBeforeReturnToMenu = 30f;

        private bool _MatchmakingCountdownStarted = false;
        private bool _MatchmakingGameStarted      = false;

        private float _WaitingTime = 0f;

        private async void UpdateMatchmaking()
        {
            if (_MatchmakingGameStarted || IsCustomGame() || Runner == null || !Runner.IsRunning)
                return;
            int playerCount = Runner.ActivePlayers.Count();

            if (playerCount >= 2) {
                _WaitingTime = 0f;

                if (!_MatchmakingCountdownStarted && HasStateAuthority) {
                    _MatchmakingCountdownStarted = true;

                    Invoke(nameof(StartMatchmakingGame), _MatchmakingCountDownDelay);
                }
            } else {
                if (_MatchmakingCountdownStarted) {
                    _MatchmakingCountdownStarted = false;

                    CancelInvoke(nameof(StartMatchmakingGame));
                }

                _WaitingTime += Time.deltaTime;

                if (_WaitingTime >= _TimeoutBeforeReturnToMenu) {
                    _WaitingTime = 0f;

                    VMUI_Loading loading = FindFirstObjectByType<VMUI_Loading>(FindObjectsInactive.Include);

                    await SceneManager.UnloadSceneAsync("FinalAnimation");
                    await SceneManager.UnloadSceneAsync("Game");
                    await SceneManager.UnloadSceneAsync("GameplayCameraTravelling");
                    await SceneManager.UnloadSceneAsync("UIv3");

                    await loading.Connection.DisconnectAsync(ConnectFailReason.GameEnded);

                    SwitchScreen<VMUI_Tavern>();
                }
            }
        }

        private void StartMatchmakingGame()
        {
            if (_MatchmakingGameStarted)
                return;
            if (Runner.ActivePlayers.Count() < 2) { // No more enough players to start the game.
                _MatchmakingCountdownStarted = false;

                return;
            }

            _MatchmakingGameStarted = true;

            RPC_Gameplay();

            GameManager manager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);

            if (manager)
                manager.StartGame();
        }

        #endregion

        #region Override Methods

        public override void Spawned()
        {
            Runner.AddCallbacks(this);

            // ! Call a IsCustomGame at the beginning for initialize the _Connection variable.
            if (IsCustomGame())
                AddPlayerCustom();
            else
                AddPlayerMatchmaking();

            if (HasStateAuthority && !IsCustomGame())
                RPC_Gameplay();
        }

        #endregion

        #region Events

        private void OnPlayersChange()
        {
            if (_Connection != null)
                _Connection.SetSessionUsernames(GetPlayersUsernames());
        }

        public async void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // Host / Server left
            if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic) {
                await _Connection.DisconnectAsync(ConnectFailReason.Disconnect);

                SwitchScreen<VMUI_Tavern>();
            }
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_UpdatePlayerState(PlayerRef player, string username, CardFamily family = CardFamily.None)
        {
            UpdatePlayerState(player, username, family);
        }

        public void UpdatePlayerState(PlayerRef player, string username, CardFamily family = CardFamily.None)
        {
            if (HasStateAuthority) {
                // Update or add the player username in the dictionary.
                if (_PlayersUsernames.TryGet(player, out NetworkString<_16> _)) // Already exists, update the username.
                    _PlayersUsernames.Set(player, username);
                else // Does not exist, add the player with the username.
                    _PlayersUsernames.Add(player, username);

                // Update or add the player in the GameDataStorage.
                GameDataStorage gameDataStorage = FindFirstObjectByType<GameDataStorage>(FindObjectsInactive.Include);

                if (gameDataStorage)
                    gameDataStorage.AddPlayer(player, username, family);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_Gameplay()
        {
            SwitchScreen<VMUI_Gameplay>();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_Loading()
        {
            SwitchScreen<VMUI_Loading>();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_NetworkTavern()
        {
            SwitchScreen<VMUI_CustomTavern>();
        }

        #endregion

        #region Unused Callbacks

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        #endregion
    }
}
