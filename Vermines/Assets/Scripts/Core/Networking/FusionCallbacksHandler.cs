using System.Collections.Generic;
using System;
using Fusion.Sockets;
using Fusion;

namespace Vermines.Core.Network {

    public sealed class FusionCallbacksHandler : INetworkRunnerCallbacks {

        #region Attributes

        public Action<NetworkRunner, NetworkObject, PlayerRef> ObjectExitAOI;
        public Action<NetworkRunner, NetworkObject, PlayerRef> ObjectEnterAOI;

        public Action<NetworkRunner, PlayerRef> PlayerJoined;
        public Action<NetworkRunner, PlayerRef> PlayerLeft;

        public Action<NetworkRunner, Fusion.NetworkInput> Input;
        public Action<NetworkRunner, PlayerRef, Fusion.NetworkInput> InputMissing;

        public Action<NetworkRunner, ShutdownReason> Shutdown;

        public Action<NetworkRunner> ConnectedToServer;

        public Action<NetworkRunner, NetDisconnectReason> DisconnectedFromServer;

        public Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]> ConnectRequest;
        public Action<NetworkRunner, NetAddress, NetConnectFailedReason> ConnectFailed;

        public Action<NetworkRunner, SimulationMessagePtr> UserSimulationMessage;

        public Action<NetworkRunner, List<SessionInfo>> SessionListUpdated;

        public Action<NetworkRunner, Dictionary<string, object>> CustomAuthenticationResponse;

        public Action<NetworkRunner, HostMigrationToken> HostMigration;

        public Action<NetworkRunner, PlayerRef, ReliableKey, ArraySegment<byte>> ReliableDataReceived;
        public Action<NetworkRunner, PlayerRef, ReliableKey, float> ReliableDataProgress;

        public Action<NetworkRunner> SceneLoadDone;
        public Action<NetworkRunner> SceneLoadStart;

        #endregion

        #region Methods

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
            DisconnectedFromServer?.Invoke(runner, reason);
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
