using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Fusion.Sockets;
using Fusion;
using UnityEngine;

namespace Vermines.Core {

    using Vermines.Core.Network;
    using Vermines.Core.Scene;
    using Vermines.Core.Settings;

    public class Matchmaking : SceneService, INetworkRunnerCallbacks {

        #region Attributes

        public bool IsJoiningToLobby;
        public bool IsConnectedToLobby;

        public Action LobbyJoined;
        public Action LobbyJoinFailed;
        public Action LobbyLeft;

        public event Action<NetworkRunner, NetworkObject, PlayerRef> ObjectExitAOI;
        public event Action<NetworkRunner, NetworkObject, PlayerRef> ObjectEnterAOI;
        public event Action<NetworkRunner, PlayerRef> PlayerJoined;
        public event Action<NetworkRunner, PlayerRef> PlayerLeft;
        public event Action<NetworkRunner, Fusion.NetworkInput> Input;
        public event Action<NetworkRunner, PlayerRef, Fusion.NetworkInput> InputMissing;
        public event Action<NetworkRunner, ShutdownReason> Shutdown;
        public event Action<NetworkRunner> ConnectedToServer;
        public event Action<NetworkRunner, NetDisconnectReason> DisconnectedFromServer;
        public event Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]> ConnectRequest;
        public event Action<NetworkRunner, NetAddress, NetConnectFailedReason> ConnectFailed;
        public event Action<NetworkRunner, SimulationMessagePtr> UserSimulationMessage;
        public event Action<NetworkRunner, List<SessionInfo>> SessionListUpdated;
        public event Action<NetworkRunner, Dictionary<string, object>> CustomAuthenticationResponse;
        public event Action<NetworkRunner, HostMigrationToken> HostMigration;
        public event Action<NetworkRunner, PlayerRef, ReliableKey, ArraySegment<byte>> ReliableDataReceived;
        public event Action<NetworkRunner, PlayerRef, ReliableKey, float> ReliableDataProgress;
        public event Action<NetworkRunner> SceneLoadDone;
        public event Action<NetworkRunner> SceneLoadStart;

        [SerializeField]
        private NetworkRunner _LobbyRunner;

        private string _LobbyName;

        #endregion

        #region Methods

        public void CreateSession(SessionRequest request, bool isCustom)
        {
            if (request.GameMode != GameMode.Server && request.GameMode != GameMode.Host && request.GameMode != GameMode.AutoHostOrClient)
                return;
            NetworkSettings settings = Context.Settings.Network;

            request.UserID = Context.PlayerData.UserID;

            if (isCustom)
                request.SessionName = settings.CodeGenerator.EncodeRegion(settings.CodeGenerator.Create(), 0);
            request.IsCustom = isCustom;

            Global.Networking.StartGame(request);
        }

        public void JoinSession(string sessionName, bool isCustom)
        {
            var request = new SessionRequest {
                UserID       = Context.PlayerData.UserID,
                GameMode     = GameMode.Client,
                GameplayType = GameplayType.Standart,
                IsCustom     = isCustom,
                SessionName  = sessionName
            };

            Global.Networking.StartGame(request);
        }

        public async Task JoinLobby(bool force = false)
        {
            if (IsJoiningToLobby || (IsConnectedToLobby && !force))
                return;
            IsJoiningToLobby = true;

            await LeaveLobby();

            var joinTask = _LobbyRunner.JoinSessionLobby(SessionLobby.Custom, _LobbyName);

            await joinTask;

            IsJoiningToLobby   = false;
            IsConnectedToLobby = joinTask.Result.Ok;

            if (IsConnectedToLobby == true)
                LobbyJoined?.Invoke();
            else
                LobbyJoinFailed?.Invoke();
        }

        public async Task LeaveLobby()
        {
            if (IsConnectedToLobby == true)
                LobbyLeft?.Invoke();
            IsConnectedToLobby = false;

            await _LobbyRunner.Shutdown(false, ShutdownReason.PhotonCloudTimeout);
        }

        #endregion

        #region Interface

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _LobbyName = "Vermines." + Application.version;

            _LobbyRunner.AddCallbacks(this);

            Context.Runner = _LobbyRunner;
        }

        protected override void OnDeinitialize()
        {
            if (_LobbyRunner != null)
                _LobbyRunner.RemoveCallbacks(this);
            base.OnDeinitialize();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnTick()
        {
            base.OnTick();
        }

        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            ObjectExitAOI?.Invoke(runner, obj, player);
        }

        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            ObjectEnterAOI?.Invoke(runner, obj, player);
        }

        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            PlayerJoined?.Invoke(runner, player);
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            PlayerLeft?.Invoke(runner, player);
        }

        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, Fusion.NetworkInput input)
        {
            Input?.Invoke(runner, input);
        }

        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, Fusion.NetworkInput input)
        {
            InputMissing?.Invoke(runner, player, input);
        }

        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Shutdown?.Invoke(runner, shutdownReason);
        }

        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
        {
            ConnectedToServer?.Invoke(runner);
        }

        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            DisconnectedFromServer.Invoke(runner, reason);
        }

        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            ConnectRequest?.Invoke(runner, request, token);
        }

        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            ConnectFailed?.Invoke(runner, remoteAddress, reason);
        }

        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            UserSimulationMessage?.Invoke(runner, message);
        }

        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            SessionListUpdated?.Invoke(runner, sessionList);
        }

        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            CustomAuthenticationResponse?.Invoke(runner, data);
        }

        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.LogError($"OnHostMigration");

            HostMigration?.Invoke(runner, hostMigrationToken);
        }

        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            ReliableDataReceived?.Invoke(runner, player, key, data);
        }

        void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            ReliableDataProgress?.Invoke(runner, player, key, progress);
        }

        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        {
            SceneLoadDone?.Invoke(runner);
        }

        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
        {
            SceneLoadStart?.Invoke(runner);
        }

        #endregion
    }
}