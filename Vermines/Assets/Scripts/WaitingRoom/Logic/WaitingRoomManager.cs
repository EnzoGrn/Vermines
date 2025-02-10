using Fusion;
using FusionUtilsEvents;
using UnityEngine;
using Fusion.Menu;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vermines {

    using Vermines.Config;
    using Vermines.Player;
    using Vermines.Utils;
    using System.Collections.Generic;
    using Vermines.Config.Utils;
    using OMGG.Network.Fusion;
    using System.Collections;

    public class WaitingRoomManager : NetworkBehaviour, IAfterSpawned
    {
        [Header("UI")]
        [SerializeField] private WaitingRoomUiHandler WaitingRoomUIHandler;
        [SerializeField] private Button _StartButton;
        [SerializeField] private TMPro.TextMeshProUGUI _StatusText;
        [SerializeField] private GameObject _GameSettings;

        [Header("Events")]
        public FusionEvent OnShutdownEvent;
        public FusionEvent OnDisconnectEvent;
        public FusionEvent OnPlayerLeftEvent;
        public FusionEvent OnPlayerJoinnedEvent;
        public FusionEvent OnHostMigrationEvent;

        [Header("Game Settings")]
        [SerializeField] private GameConfiguration _GameSettingsData;

        [Header("Network Variable")]
        [Networked, Capacity(10)] public NetworkDictionary<PlayerRef, WaitingRoomPlayerData> Players { get; }

        [Header("Scene References")]
        [SerializeField] private FusionMenuConfig _SceneConfig;

        #region Private Fields
        private NetworkQueue _Queue = new NetworkQueue();
        private int _RpcCounter = 0;
        [Networked] private int _TotalRpc {  get; set; }
        #endregion

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
                _GameSettings.gameObject.SetActive(true);
            }
            else
            {
                _StatusText.gameObject.SetActive(true);
                _StartButton.gameObject.SetActive(false);
                _GameSettings.gameObject.SetActive(false);
            }
        }

        public void StartGame()
        {
            if (!Runner.IsServer)
                return;

            if (_GameSettingsData.RandomSeed.Value)
            {
                _GameSettingsData.Seed = UnityEngine.Random.Range(0, int.MaxValue);
            }

            // TODO : Maybe use the Rpc Queue
            SetGameSettings();
        }

        // Fusion Event (not Fusion interface implementation)
        private void PlayerJoined(PlayerRef player, NetworkRunner networkRunner)
        {
            Debug.Log("WaitingRoomManager Player joined the room callback Id: " + player.PlayerId + ".");

            // Add player in the list if the player is the host
            if (networkRunner.IsServer)
            {
                WaitingRoomPlayerData playerData = new WaitingRoomPlayerData(player, networkRunner.LocalPlayer == player);
                Players.Set(player, playerData);

                //WaitingRoomPlayerData playerData = new WaitingRoomPlayerData
                //{
                //    Nickname = "Player " + player.PlayerId,
                //    PlayerRef = player,
                //    IsHost = (networkRunner.LocalPlayer == player)
                //};
                //playerData.OnIsReadyUpdated = new UnityEngine.Events.UnityEvent();

                UpdateStartButtonState();
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

                UpdateStartButtonState();
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

                    WaitingRoomPlayerData newPlayerData = new WaitingRoomPlayerData(playerData.Value.PlayerRef, playerData.Value.IsHost);

                    //WaitingRoomPlayerData newPlayerData = new WaitingRoomPlayerData
                    //{
                    //    Nickname = "Player " + playerData.Value.PlayerRef,
                    //    PlayerRef = playerData.Value.PlayerRef,
                    //    IsHost = (networkRunner.LocalPlayer == playerData.Value.PlayerRef),
                    //    OnIsReadyUpdated = new()
                    //};

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

            //if (Runner != null)
            //{
            //    Runner.Shutdown();
            //}

            SceneManager.LoadScene("Startup");
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

        private void UpdateStartButtonState()
        {
            if (Runner.IsServer)
            {
                _StartButton.interactable = (Players.Count >= _GameSettingsData.MinPlayers.Value && Players.Count <= _GameSettingsData.MaxPlayers.Value);
            }
        }

        private void KickLastPlayerJoinned()
        {
            int numberOfPlayersToKick = Players.Count - _GameSettingsData.MaxPlayers.Value;

            for (int i = 0; i < numberOfPlayersToKick; i++)
            {
                PlayerRef playerToKick = Players.Last().Key;
                Debug.Log($"Kicking player {playerToKick} due to settings");
                Runner.Disconnect(playerToKick);
            }
        }

        private void UpdateRoomPlayerLimit()
        {
            // Change the room player limit of fusion room
            if (Runner.IsServer)
            {
                // TODO: Change the room player limit of fusion room
                // The way to make it may be to create a new room with the new player limit
                // It seems that there is no easy way to change the room player limit of a fusion room
            }
        }

        public void OnGameSettingsChangeUpdateUI()
        {
            // Update UI
            if (Runner.IsServer)
            {
                Debug.Log("WaitingRoomManager OnGameSettingsChangeUpdateUI.");

                UpdateRoomPlayerLimit();
                UpdateStartButtonState();

                if (Players.Count > _GameSettingsData.MaxPlayers.Value)
                {
                    Debug.Log("WaitingRoomManager OnGameSettingsChangeUpdateUI. Kicking last player joined.");
                    KickLastPlayerJoinned();
                }
            }
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

        private int OffsetForNoneSettingsField()
        {
            int offset = 0;

            foreach (var field in _GameSettingsData.GetType().GetFields())
            {
                if (field.GetValue(_GameSettingsData) is ASettingBase)
                {
                    break;
                }
                offset++;
            }

            Debug.Log($"Offset = {offset}");

            return offset;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_AddListenerToAllClients()
        {
            _Queue.EnqueueRPC(() => {
                _Queue.OnQueueProcessingDone.AddListener(IsClientReady);
            });
        }

        private void SetGameSettings()
        {
            RPC_AddListenerToAllClients();

            if (!Runner.IsServer)
                return;

            //foreach (var player in Players)
            //{
            //    // TODO : Do it when we instantiate the player
            //    WaitingRoomPlayerData playerData = player.Value; // Copy the struct

            //    //playerData.OnIsReadyUpdated = new UnityEngine.Events.UnityEvent();

            //    if (playerData.OnIsReadyUpdated == null)
            //    {
            //        Debug.LogWarning("playerData.OnIsReadyUpdated is null !");
            //        continue;
            //    }

            //    playerData.OnIsReadyUpdated.AddListener(TryToLoadGameScene);
            //    Players.Set(player.Key, playerData); // Assign it back to the dictionary
            //}

            Dictionary<string, SplittedJsonFragment> data = JsonSerializeUtils.SplitSerializedData(_GameSettingsData.Serialize());

            int additionalOffset = OffsetForNoneSettingsField();

            _TotalRpc = data.Values.Count;
            Debug.LogWarning("_TotalRpc -> " + _TotalRpc);

            foreach (SplittedJsonFragment jsonFragment in data.Values)
            {
                // Use RpcLocalInvokeResult  for the last one to know when every Rpc are done, thanks to the Reliable Channel
                RPC_SendGameSettings((jsonFragment.Offset + additionalOffset), jsonFragment.NumberOfData, jsonFragment.Data.ToString());
            }
        }

        // TODO : Find a better way to get notified when a client is ready.
        private void IsClientReady()
        {
            Debug.LogWarning($"_RpcCounter -> {_RpcCounter}, _TotalRpc {_TotalRpc}");

            if (_RpcCounter != _TotalRpc)
            {
                Debug.LogWarning("Cannot load the game not all rpc as been executed");
                return;
            }

            // Set the isReady of the playerRef to true
            WaitingRoomPlayerData playerData = Players.Get(Runner.LocalPlayer);
            
            playerData.IsReady = true;
            
            Players.Set(playerData.PlayerRef, playerData);

            foreach (var player in Players)
            {
                // Display player is ready
                Debug.Log($" Player name : {player.Value.Nickname}, is Ready {player.Value.IsReady}" );
            }

            if (Runner.IsServer)
            {
                // Start TryToLoadGameScene
                //Rpc_MyStaticRpc(Runner, playerData.PlayerRef.PlayerId);

                StartCoroutine(TryToLoadGameScene());
                //StopAllCoroutines();
            }
            else
            {
                if (!Object.HasStateAuthority) // Only request if we don't already have authority
                {
                    Debug.LogWarning("!Object.HasStateAuthority");
                    Object.RequestStateAuthority();
                    Object.RequestStateAuthority();
                }

                // Send RPC from the local client to the host
                Rpc_MyStaticRpc(Runner, playerData.PlayerRef.PlayerId);

                Debug.LogWarning($"Object.HasStateAuthority: {Object.HasStateAuthority}");

                _Queue.OnQueueProcessingDone.RemoveListener(IsClientReady);
            }

            //else
            //{
            //    // Remove the listener
            //    Debug.Log("Ready To Call RPC_NotifyHostClientIsReady");
            //    if (Runner.Mode == SimulationModes.Server)
            //        return;
            //    Debug.Log($"Is this object networked? {Object != null}");
            //    Debug.Log($"Is this object is valid? {Object.IsValid}");
            //    Debug.Log($"Does it have StateAuthority? {Object.HasStateAuthority}");
            //    Debug.Log($"Does it have InputAuthority? {Object.HasInputAuthority}");
            //    // Call an RPC to notify the host that the client is ready
            //    //PlayerRef.None
            //    RPC_NotifyHostClientIsReady(); // None referer to the server
            //    Debug.Log("Call RPC_NotifyHostClientIsReady as been done !");
            //}
        }

        [Rpc]
        public static void Rpc_MyStaticRpc(NetworkRunner runner, int a)
        {
            Debug.LogError($"Rpc_MyStaticRpc : {a}.");
        }

        //[Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        //private void RPC_NotifyHostClientIsReady()
        //{
        //    //[RpcTarget] PlayerRef player
        //    Debug.Log("sfjsdkfjsdjfslkdjf");

        //    _Queue.EnqueueRPC(() =>
        //    {
        //        Debug.Log($"RPC_NotifyHostClientIsReady: RPC RECEIVED ARE YOU THE HOST : {Runner.IsServer}");

        //        if (!Runner.IsServer)
        //            return;

        //        Debug.LogWarning("Server Will TryToLoadGameScene");
        //        TryToLoadGameScene();
        //    });
        //}

        private IEnumerator TryToLoadGameScene()
        {
            bool areClientsReady = true;
            Debug.Log("TryToLoadGameScene");

            // Check if all clients are ready
            foreach (var player in Players)
            {
                Debug.Log($"TryToLoadGameScene Player name : {player.Value.Nickname}, is Ready {player.Value.IsReady}");

                if (!player.Value.IsReady)
                {
                    areClientsReady = false;
                    break;
                }
            }

            //if (!areClientsReady)
            //{
            //    // Start the coroutine again in 2 seconds
            //    Debug.LogWarning("Not all the players are ready");
            //    yield return new WaitForSeconds(4f);
            //    StartCoroutine(TryToLoadGameScene()); // Restart the coroutine
            //    yield break; // Exit the current coroutine execution
            //}
            //else
            //{
            //    // All clients are ready load the game scene
            //    Debug.LogWarning("OK Done");
            //}

            //yield break;

            if (!areClientsReady)
            {
                yield return new WaitForSeconds(4f);
            }

            // Load the game scene
            PhotonMenuSceneInfo gameScene = _SceneConfig.AvailableScenes.Find(scene => scene.SceneName == "Game");

            // Check if the game scene is available
            if (gameScene.Equals(null))
            {
                Debug.LogError("WaitingRoomManager Game scene not found.");
            }

            _Queue.OnQueueProcessingDone.RemoveListener(IsClientReady);

            // Load the game scene
            Runner.LoadScene(gameScene.SceneName);

            yield break; // Explicitly return to satisfy IEnumerator
        }

        #region RPC
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_SendGameSettings(int offset, int numberOfData, string serializedGameSettings)
        {
            _RpcCounter++;
            _Queue.EnqueueRPC(() => {
                _GameSettingsData.Deserialize(serializedGameSettings, offset, numberOfData);
            });
            Debug.LogWarning("_RpcCounter -> " + _RpcCounter);
        }
        #endregion
    }
}