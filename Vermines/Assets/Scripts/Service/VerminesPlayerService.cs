using OMGG.Menu.Connection;
using Fusion.Sockets;
using Fusion;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Vermines.Service {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Menu.Connection.Element;
    using Vermines.Menu.Screen;

    public class VerminesPlayerService : NetworkBehaviour, IPlayerLeft, INetworkRunnerCallbacks {

        private ChangeDetector              _ChangeDetector;
        private VerminesConnectionBehaviour _Connection;
        private VMUI_Gameplay               _MenuUIGameplay;

        #region Players

        private const int PLAYERS_MAX_COUNT = 4; // ! This value must be bigger than the one on the config file.

        /// <summary>
        /// The list of players usernames.
        /// </summary>
        [Networked, Capacity(PLAYERS_MAX_COUNT)]
        private NetworkDictionary<PlayerRef, NetworkString<_16>> _PlayersUsernames => default;

        public List<string> GetPlayersUsernames()
        {
            List<string> playersList = new();

            foreach (var pair in _PlayersUsernames)
                playersList.Add(pair.Value.Value);
            return playersList;
        }

        public void CheckMaxPlayerCount()
        {
            if (Runner.SessionInfo.MaxPlayers > PLAYERS_MAX_COUNT)
                Debug.LogWarning($"Current gameplay overlay max clients capacity ({PLAYERS_MAX_COUNT}) is less than the session max players ({Runner.SessionInfo.MaxPlayers}). Consider increasing.");
        }

        private void RemovePlayer(PlayerRef player)
        {
            _PlayersUsernames.Remove(player);
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (Object.HasStateAuthority)
                RemovePlayer(player);
        }

        #endregion

        #region Override Methods

        public override void Spawned()
        {
            Runner.AddCallbacks(this);

            VerminesConnectionBehaviour connectionBehaviour = FindFirstObjectByType<VerminesConnectionBehaviour>(FindObjectsInactive.Include);

            if (connectionBehaviour == false) {
                Log.Error("Connection behaviour not found!");

                return;
            }

            _MenuUIGameplay = FindFirstObjectByType<VMUI_Gameplay>(FindObjectsInactive.Include);

            if (_MenuUIGameplay == false) {
                Log.Error("FusionMenuUIGameplay not found!");

                return;
            }

            _Connection = connectionBehaviour;

            CheckMaxPlayerCount();

            _ChangeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            VMUI_Tavern tavern = FindFirstObjectByType<VMUI_Tavern>(FindObjectsInactive.Include);

            if (tavern && tavern.SelectedCultist != null)
                RPC_AddPlayer(Runner.LocalPlayer, PlayerPrefs.GetString("OMGG.Profile.Username"), tavern.SelectedCultist.family);
            else
                RPC_AddPlayer(Runner.LocalPlayer, PlayerPrefs.GetString("OMGG.Profile.Username"));
        }

        public override void Render()
        {
            if (_ChangeDetector == null)
                return;
            foreach (var change in _ChangeDetector.DetectChanges(this)) {
                if (change == nameof(_PlayersUsernames))
                    OnPlayersChange();
            }
        }

        #endregion

        #region Events

        private void OnPlayersChange()
        {
            (_Connection as VerminesConnectionBehaviour)?.SetSessionUsernames(GetPlayersUsernames());
        }

        public async void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // Host / Server left
            if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
            {
                await _Connection.DisconnectAsync(ConnectFailReason.Disconnect);

                _MenuUIGameplay.Controller.Show<VMUI_MainMenu>(_MenuUIGameplay);
            }
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddPlayer(PlayerRef player, string username, CardFamily family = CardFamily.None)
        {
            _PlayersUsernames.Add(player, username);

            GameDataStorage gameDataStorage = FindFirstObjectByType<GameDataStorage>(FindObjectsInactive.Include);

            if (gameDataStorage)
                gameDataStorage.AddPlayer(player, username, family);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_Gameplay()
        {
            VMUI_Gameplay gameplay = FindFirstObjectByType<VMUI_Gameplay>(FindObjectsInactive.Include);

            gameplay.Controller.Show<VMUI_Gameplay>();
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
