using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public class NetworkSpawner : MonoBehaviour, INetworkRunnerCallbacks {

    public NetworkPlayer PlayerPrefab;
    
    private SessionListUIHandler _SessionListUIHandler;

    private void Awake()
    {
        _SessionListUIHandler = FindFirstObjectByType<SessionListUIHandler>(FindObjectsInactive.Include);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server.");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Connection failed: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}

    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // Shutdown the current runner.
        await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

        // Find the network runner handler and start host migration.
        FindFirstObjectByType<NetworkRunnerHandler>().StartHostMigration(hostMigrationToken);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer) {
            Debug.Log("[SERVER]: Spawning player.");

            runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, player);
        } else {
            Debug.Log("[CLIENT]: New player joined.");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}

    public void OnSceneLoadDone(NetworkRunner runner) {}

    public void OnSceneLoadStart(NetworkRunner runner) {}

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (_SessionListUIHandler == null)
            return;
        Debug.Log("Session list updated.");
        if (sessionList.Count == 0) {
            Debug.Log("Joined lobby no sessions found.");

            _SessionListUIHandler.OnNoSessionsFound();
        } else {
            _SessionListUIHandler.ClearList();

            foreach (SessionInfo session in sessionList) {
                _SessionListUIHandler.AddToList(session);

                Debug.Log($"Found session: {session.Name}, player count: {session.PlayerCount}");
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
}
