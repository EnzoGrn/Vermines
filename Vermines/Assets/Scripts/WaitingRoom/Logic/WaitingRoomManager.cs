using Fusion;
using FusionUtilsEvents;
using UnityEngine;


namespace Vermines
{
    using Fusion.Menu;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Vermines.Player;

    public class WaitingRoomManager : NetworkBehaviour, IAfterSpawned
    {
        [Header("UI")]
        [SerializeField] private WaitingRoomUiHandler WaitingRoomUIHandler;
        [SerializeField] private Button _StartButton;
        [SerializeField] private TMPro.TextMeshProUGUI _StatusText;

        [Header("Events")]
        public FusionEvent OnShutdownEvent;
        public FusionEvent OnDisconnectEvent;
        public FusionEvent OnPlayerLeftEvent;
        public FusionEvent OnPlayerJoinnedEvent;
        public FusionEvent OnHostMigrationEvent;

        [Header("Network Variable")]
        [Networked, Capacity(10)] public NetworkDictionary<PlayerRef, WaitingRoomPlayerData> Players { get; }

        [Header("Scene References")]
        [SerializeField] private FusionMenuConfig _SceneConfig;

        public override void Spawned()
        {
            Debug.Log("WaitingRoomManager spawned.");
        }

        public void AfterSpawned()
        {
            Debug.Log("WaitingRoomManager AfterSpawned.");

            RegisterFusionEvent();

            // Call update list for new player joining, because they don't receive PlayerJoined event
            UpdateUiList(Runner);

            // Update Ui
            if (Runner.IsServer)
            {
                _StatusText.gameObject.SetActive(false);
                _StartButton.gameObject.SetActive(true);
            }
            else
            {
                _StatusText.gameObject.SetActive(true);
                _StartButton.gameObject.SetActive(false);
            }
        }

        public void StartGame()
        {
            if (!Runner.IsServer)
                return;

            // Load the game scene
            PhotonMenuSceneInfo gameScene = _SceneConfig.AvailableScenes.Find(scene => scene.SceneName == "Game");

            // Check if the game scene is available
            if (gameScene.Equals(null))
            {
                Debug.LogError("WaitingRoomManager Game scene not found.");
                return;
            }

            // Load the game scene
            Runner.LoadScene(gameScene.SceneName);
        }

        // Fusion Event (not Fusion interface implementation)
        private void PlayerJoined(PlayerRef player, NetworkRunner networkRunner)
        {
            Debug.Log("WaitingRoomManager Player joined the room callback Id: " + player.PlayerId + ".");

            // Add player in the list if the player is the host
            if (networkRunner.IsServer)
            {
                WaitingRoomPlayerData playerData = new WaitingRoomPlayerData
                {
                    Nickname = "Player " + player.PlayerId,
                    PlayerRef = player,
                    IsHost = (networkRunner.LocalPlayer == player)
                };
                Players.Set(player, playerData);
            }

            // Update list
            UpdateUiList(networkRunner);
        }

        // Fusion Event (not Fusion interface implementation)
        private void PlayerLeft(PlayerRef player, NetworkRunner networkRunner)
        {
            Debug.Log("WaitingRoomManager Player left the room callback Id: " + player.PlayerId + ".");

            // Remove player from the list if the player is the host
            if (networkRunner.IsServer)
            {
                Players.TryGet(player, out var playerData);
                Players.Remove(player);

                // Dump Active Players
                foreach (var activePlayers in Runner.ActivePlayers)
                {
                    Debug.Log("---------> Player: " + activePlayers.PlayerId + " is active.");
                }
            }

            if (player == networkRunner.LocalPlayer)
            {
                UnregisterFusionEvent();
                return;
            }

            // Update list
            UpdateUiList(networkRunner);
        }

        private void UpdateUiList(NetworkRunner networkRunner)
        {
            WaitingRoomUIHandler.ClearList();
            
            foreach (var player in Players)
            {
                WaitingRoomUIHandler.AddPlayerList(player.Value, (player.Key == networkRunner.LocalPlayer));
            }
        }

        private void UpdateListOnHostMigration(PlayerRef player, NetworkRunner networkRunner)
        {
            Debug.Log("WaitingRoomManager UpdateListOnHostMigration.");

            if (networkRunner.IsServer)
            {
                // Update Players network dictionary
                foreach (var playerData in Players)
                {
                    if (!networkRunner.ActivePlayers.Contains(playerData.Value.PlayerRef))
                    {
                        // Remvoe the player from the Players network dictionary
                        Players.Remove(playerData.Value.PlayerRef);
                        continue;
                    }

                    WaitingRoomPlayerData newPlayerData = new WaitingRoomPlayerData
                    {
                        Nickname = "Player " + playerData.Value.PlayerRef,
                        PlayerRef = playerData.Value.PlayerRef,
                        IsHost = (networkRunner.LocalPlayer == playerData.Value.PlayerRef)
                    };

                    // Update the player data by setting the new player data
                    Players.Set(playerData.Value.PlayerRef, newPlayerData);
                }
            }

            UpdateUiList(networkRunner);
        }

        // Called from button
        public void LeaveWaitingRoom()
        {
            _ = LeaveWaitingRoomAsync();
        }

        private async Task LeaveWaitingRoomAsync()
        {
            if (Runner.IsServer)
            {
                CloseLobby();
                // Add a delay to avoid the host being desconnected before the other players
                // TODO: Maybe find a better solution
                await Task.Delay(1000);
            }
            await Runner?.Shutdown();
        }

        private void CloseLobby()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (player != Runner.LocalPlayer)
                    Runner.Disconnect(player);
            }
        }

        private void OnShutdown(PlayerRef player, NetworkRunner runner)
        {
            Debug.Log("WaitingRoomManager OnShutdown.");

            //if (runner.SceneManager != null)
            //{
            //    if (runner.SceneManager.MainRunnerScene.IsValid() == true)
            //    {
            //        SceneRef sceneRef = runner.SceneManager.GetSceneRef(runner.SceneManager.MainRunnerScene.name);
            //        Debug.Log("---------------------> UnloadScene : " + runner.SceneManager.MainRunnerScene.name + "<---------------------");
            //        runner.SceneManager.UnloadScene(sceneRef);
            //    }
            //}

            SceneManager.LoadScene("StartUp");
        }

        private void OnDisconnect(PlayerRef player, NetworkRunner runner)
        {
            Debug.Log($"Disconnecting {player}");
            runner.Shutdown();
        }

        private void OnDisable()
        {
            Debug.Log("WaitingRoomManager disabled.");
            UnregisterFusionEvent();
        }

        private void RegisterFusionEvent()
        {
            Debug.Log("WaitingRoomManager RegisterFusionEvent.");
            OnShutdownEvent.RegisterResponse(OnShutdown);
            OnPlayerLeftEvent.RegisterResponse(PlayerLeft);
            OnDisconnectEvent.RegisterResponse(OnDisconnect);
            OnPlayerJoinnedEvent.RegisterResponse(PlayerJoined);
            OnHostMigrationEvent.RegisterResponse(UpdateListOnHostMigration);
        }

        private void UnregisterFusionEvent()
        {
            Debug.Log("WaitingRoomManager UnregisterFusionEvent.");
            OnShutdownEvent.RemoveResponse(OnShutdown);
            OnPlayerLeftEvent.RemoveResponse(PlayerLeft);
            OnDisconnectEvent.RegisterResponse(OnDisconnect);
            OnPlayerJoinnedEvent.RemoveResponse(PlayerJoined);
            OnHostMigrationEvent.RemoveResponse(UpdateListOnHostMigration);
        }
    }
}