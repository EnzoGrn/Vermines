using Fusion.Menu;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using FusionUtilsEvents;

namespace Vermines {

    public class MenuConnectionBehaviour : FusionMenuConnectionBehaviour {

        [Header("Settings")]

        [Tooltip("The runner prefab to use for the connection.")]
        public NetworkRunner RunnerPrefab;

        [Tooltip("The UI controller to use for the connection.")]
        public MenuUIController UIController;

        [Tooltip("The connection plugin to use for the connection.")]
        public MenuConnectionPlugin ConnectionPlugin;

        public override IFusionMenuConnection Create()
        {
            if (ConnectionPlugin != null)
                return ConnectionPlugin.Create(this);

            return new MenuConnection(this);
        }
    }

    public class MenuConnection : IFusionMenuConnection {

        [Header("Settings")]

        public bool IsSessionOwner => _runner && _runner.IsRunning ? _runner.IsSceneAuthority : default;

        public string SessionName => _runner && _runner.IsRunning ? _runner.SessionInfo.Name : default;

        public int MaxPlayerCount => _runner && _runner.IsRunning ? _runner.SessionInfo.MaxPlayers : default;

        public string Region => _runner && _runner.IsRunning ? _runner.SessionInfo.Region : default;

        public string AppVersion => PhotonAppSettings.Global.AppSettings.AppVersion;

        public List<string> Usernames => default;

        public bool IsConnected => _runner ? _runner.IsConnectedToServer : default;

        public int Ping => _runner && _runner.IsRunning ? Mathf.RoundToInt((float)(_runner.GetPlayerRtt(PlayerRef.None) * 1000.0)) : default;

        protected FusionMenuConnectionBehaviour ConnectionBehaviour => _connectionBehaviour;

        /// <summary>
        /// The connection behaviour.
        /// </summary>
        private readonly MenuConnectionBehaviour _connectionBehaviour;

        /// <summary>
        /// The network runner (network instance of Fusion).
        /// </summary>
        private NetworkRunner _runner;

        public MenuConnection(MenuConnectionBehaviour connectionBehaviour)
        {
            _connectionBehaviour = connectionBehaviour;
        }

        public virtual Task<List<FusionMenuOnlineRegion>> RequestAvailableOnlineRegionsAsync(IFusionMenuConnectArgs connectArgs)
        {
            List<FusionMenuOnlineRegion> regions = new();

            foreach (var region in _connectionBehaviour.UIController.Config.AvailableRegions) {
                regions.Add(new FusionMenuOnlineRegion {
                    Code = region,
                    Ping = 0
                });
            }
            return Task.FromResult(regions);
        }

        public virtual async Task<ConnectResult> ConnectAsync(IFusionMenuConnectArgs connectionArgs)
        {
            _runner = CreateRunner();

            var appSettings = PhotonAppSettings.Global.AppSettings.GetCopy();

            appSettings.FixedRegion = connectionArgs.Region;

            var startGameArgs = new StartGameArgs() {
                SessionName             = connectionArgs.Session,
                PlayerCount             = connectionArgs.MaxPlayerCount,
                GameMode                = GetGameMode(connectionArgs),
                CustomPhotonAppSettings = appSettings
            };

            if (connectionArgs.Creating == false && string.IsNullOrEmpty(connectionArgs.Session) == true) {
                startGameArgs.EnableClientSessionCreation = false;

                var randomJoinResult = await StartRunner(startGameArgs);

                if (randomJoinResult.Success)
                    return await StartGame(connectionArgs.Scene.SceneName);
                if (randomJoinResult.FailReason == ConnectFailReason.UserRequest)
                    return ConnectionFail(randomJoinResult.FailReason);
                connectionArgs.Creating = true;

                _runner = CreateRunner();

                startGameArgs.EnableClientSessionCreation = true;
                startGameArgs.SessionName                 = _connectionBehaviour.UIController.Config.CodeGenerator.Create();
                startGameArgs.GameMode                    = GetGameMode(connectionArgs);
            }

            var result = await StartRunner(startGameArgs);

            if (result.Success)
                return await StartGame(connectionArgs.Scene.SceneName);
            await DisconnectAsync(result.FailReason);

            return ConnectionFail(result.FailReason);
        }

        public virtual async Task DisconnectAsync(int reason)
        {
            var runner = _runner;

            _runner = null;

            if (runner != null) {
                Scene sceneToUnload = default;

                if (runner.IsSceneAuthority == true && runner.TryGetSceneInfo(out NetworkSceneInfo sceneInfo) == true) {
                    foreach (var sceneRef in sceneInfo.Scenes)
                        await runner.UnloadScene(sceneRef);
                } else {
                    sceneToUnload = runner.SceneManager.MainRunnerScene;
                }

                await runner.Shutdown();

                if (sceneToUnload.IsValid() == true && sceneToUnload.isLoaded == true && sceneToUnload != _connectionBehaviour.gameObject.scene) {
                    SceneManager.SetActiveScene(_connectionBehaviour.gameObject.scene);

                    _ = SceneManager.UnloadSceneAsync(sceneToUnload);
                }
            }

            if (reason != ConnectFailReason.UserRequest)
                await _connectionBehaviour.UIController.PopupAsync(reason.ToString(), "Disconnected");
            _connectionBehaviour.UIController.OnGameStopped();
        }

        private GameMode GetGameMode(IFusionMenuConnectArgs connectionArgs)
        {
            if (_connectionBehaviour.UIController.SelectedGameMode == GameMode.AutoHostOrClient)
                return connectionArgs.Creating ? GameMode.Host : GameMode.Client;
            return _connectionBehaviour.UIController.SelectedGameMode;
        }

        private NetworkRunner CreateRunner()
        {
            var runner = GameObject.Instantiate(_connectionBehaviour.RunnerPrefab);

            runner.ProvideInput = true;

            return runner;
        }

        private async Task<ConnectResult> StartRunner(StartGameArgs args)
        {
            _ = await _runner.StartGame(args);

            return new ConnectResult() {
                Success    = _runner.IsRunning,
                FailReason = ConnectFailReason.Disconnect
            };
        }

        private async Task<ConnectResult> StartGame(string sceneName)
        {
            try {
                _runner.AddCallbacks(new MenuConnectionCallbacks(_connectionBehaviour.UIController, sceneName));

                if (_runner.IsSceneAuthority)
                    await _runner.LoadScene(sceneName, LoadSceneMode.Additive, LocalPhysicsMode.None, true);

                _connectionBehaviour.UIController.OnGameStarted();

                return ConnectionSuccess();
            } catch (ArgumentException e) {
                Debug.LogError($"Failed to load scene. {e}.");

                await DisconnectAsync(ConnectFailReason.Disconnect);

                return ConnectionFail(ConnectFailReason.Disconnect);
            }
        }

        private static ConnectResult ConnectionSuccess() => new() {
            Success = true
        };

        private static ConnectResult ConnectionFail(int failReason) => new() {
            FailReason = failReason
        };

        private class MenuConnectionCallbacks : INetworkRunnerCallbacks
        {
            /// <summary>
            /// The menu UI controller.
            /// </summary>
            public readonly MenuUIController Controller;

            public readonly string SceneName;

            private Dictionary<string, FusionEvent> _FusionEvents;

            private byte[] _connectionToken;

            public MenuConnectionCallbacks(MenuUIController controller, string sceneName)
            {
                Controller = controller;
                SceneName  = sceneName;

                // Load Fusion Events from scriptable objects, see Assets/Resources/ScriptableObject/FusionEvents
                _FusionEvents = new Dictionary<string, FusionEvent>()
                {
                    { "OnShutDown", null },
                    { "OnDisconnect", null},
                    { "OnPlayerLeft", null},
                    { "OnPlayerJoinned", null},
                    { "OnHostMigration", null}
                };

                LoadFusionEvents();

                // Usefull for the host migration
                // _connectionToken = Vermines.Utils.ConnectionTokenUtils.NewToken();
            }

            private void LoadFusionEvents()
            {
                Dictionary<string, FusionEvent> tempDict = new Dictionary<string, FusionEvent>();

                foreach (var fusionEvent in _FusionEvents)
                {
                    tempDict[fusionEvent.Key] = Resources.Load<FusionEvent>($"ScriptableObject/FusionEvents/{fusionEvent.Key}");
                    
                    if (tempDict[fusionEvent.Key] == null)
                    {
                        Debug.LogError($"Failed to load FusionEvent: {fusionEvent.Key}");
                    }
                }

                _FusionEvents = tempDict;
            }

            public NetworkRunner InstantiateNewRunner()
            {
                // Load the prefab from Resources
                var runnerPrefab = Resources.Load<NetworkRunner>("Prefabs/Network/Runner/NetworkRunner");

                if (runnerPrefab == null)
                {
                    Debug.LogError("Failed to load NetworkRunner prefab. Check the path: Assets/Resources/Prefabs/Network/Runner/NetworkRunner.prefab");
                    return null;
                }

                // Instantiate a new runner
                var newRunner = GameObject.Instantiate(runnerPrefab);

                // Ensure input is enabled if needed
                // newRunner.ProvideInput = true;

                return newRunner;
            }

            /// <summary>
            /// Start the Fusion simulation on a particular NetworkRunner
            /// </summary>
            /// <param name="runner">NetworkRunner to start the simulation</param>
            /// <param name="gameMode">GameMode used to start the Runner</param>
            /// <param name="connectionToken">Connection Token used to identify a particular Client</param>
            /// <param name="sceneRef">Scene reference</param>
            /// <param name="migrationToken">[Optional] Host Migration Token</param>
            /// <param name="migrationResume">[Optional] Host Migration Resume Callback</param>
            /// <returns></returns>
            private Task<StartGameResult> StartSimulation(NetworkRunner runner,
              GameMode gameMode,
              byte[] connectionToken,
              SceneRef sceneRef,
              HostMigrationToken migrationToken = null,
              Action<NetworkRunner> migrationResume = null)
            {
                if (runner.gameObject.TryGetComponent<NetworkSceneManagerDefault>(out var sceneManager) == false)
                {
                    sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
                }

                return runner.StartGame(new StartGameArgs()
                {
                    GameMode = gameMode,
                    SceneManager = sceneManager,
                    Scene = sceneRef,
                    HostMigrationToken = migrationToken,
                    HostMigrationResume = migrationResume,
                    ConnectionToken = connectionToken
                });
            }

            public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
            {
                // Can check if the Runner is being shutdown because of the Host Migration
                if (shutdownReason == ShutdownReason.HostMigration)
                {
                    Debug.Log("Network Runner OnShutdown: Host Migration Shutdown");
                }
                else if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
                {
                    Controller.OnGameStopped();
                    Controller.Show<FusionMenuUIMain>();
                    Controller.PopupAsync("Disconnected from the server.", "Disconnected");

                    if (runner.SceneManager != null)
                    {
                        if (runner.SceneManager.MainRunnerScene.IsValid() == true)
                        {
                            SceneRef sceneRef = runner.SceneManager.GetSceneRef(runner.SceneManager.MainRunnerScene.name);
                            Debug.Log("---------------------> UnloadScene <---------------------");
                            runner.SceneManager.UnloadScene(sceneRef);
                        }
                    }
                }
                else
                {
                    Debug.Log("Network Runner OnShutdown: " + shutdownReason);
                    //// Raise the shutdown event
                    //Controller.OnGameStopped();
                    //Controller.Show<FusionMenuUIMain>();
                    //Controller.PopupAsync("Disconnected from the server.", "Disconnected");

                    _FusionEvents["OnShutDown"].Raise(runner.LocalPlayer, runner);
                    //_OnShutdownEvent.Raise(runner.LocalPlayer, runner);
                }
            }

            public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
            public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

            public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
                Debug.Log("Network Runner OnPlayerJoined Callback");
                _FusionEvents["OnPlayerJoinned"].Raise(player, runner);
            }

            public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
                Debug.Log("Network Runner OnPlayerLeft Callback");
                _FusionEvents["OnPlayerLeft"].Raise(player, runner);
            }

            public void OnInput(NetworkRunner runner, NetworkInput input) {}
            public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
            public void OnConnectedToServer(NetworkRunner runner) {}

            public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
            {
                Debug.Log("Network Runner OnDisconnectedFromServer Callback");
                _FusionEvents["OnDisconnect"].Raise(runner: runner);
            }

            public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
            public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
            public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
            public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
            public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}

            private void HostMigrationResume(NetworkRunner runner)
            {
                Debug.Log("Network Runner HostMigrationResume called.");

                return;

                // Resume the networkObjects
                //foreach (NetworkObject networkObject in runner.GetResumeSnapshotNetworkObjects())
                //{
                //    Debug.Log("Network Runner Resume NetworkObject: " + networkObject.name + " Id: " + networkObject.Id);

                //    // Extract any NetworkBehavior used to represent the position/rotation of the NetworkObject
                //    // this can be either a NetworkTransform or a NetworkRigidBody, for example
                //    if (networkObject.TryGetBehaviour<NetworkTRSP>(out var posRot)) {

                //        // Debug.Log("Pos Rot: " + posRot.Data.Position + " " + posRot.Data.Rotation.eulerAngles);
                        
                //        if (networkObject.NetworkTypeId.IsPrefab)
                //        {
                //            Debug.Log("Network Runner NetworkObject is a prefab.");
                //            var newNetworkObject = runner.Spawn(networkObject, position: posRot.Data.Position, rotation: posRot.Data.Rotation, onBeforeSpawned: (innerRunner, networkObject) => {
                //                // One key aspects of the Host Migration is to have a simple way of restoring the old NetworkObjects state
                //                // If all state of the old NetworkObject is all what is necessary, just call the NetworkObject.CopyStateFrom
                //                networkObject.CopyStateFrom(networkObject);

                //                // If only partial State is necessary, it is possible to copy it only from specific NetworkBehaviours
                //                //if (resumeNO.TryGetBehaviour<PlayerController>(out var playerControllerComponentRef)) {
                //                //  newNO.GetComponent<PlayerController>().CopyStateFrom(playerControllerComponentRef);
                //                //}
                //            });
                //        }
                //    }
                //}

                //// Resume the SceneObjects
                //foreach ((NetworkObject, NetworkObjectHeaderPtr) resumeSceneObject in runner.GetResumeSnapshotNetworkSceneObjects())
                //{
                //    var sceneObject = resumeSceneObject.Item1; // Reference to local Scene Object
                //    var objectState = resumeSceneObject.Item2; // Reference to Scene Object State from old Host

                //    // Copy data back to Scene Object 
                //    // sceneObject.CopyStateFromSceneObject(objectState);
                //    sceneObject.CopyStateFrom(objectState);

                //    Log.Debug($"Network Runner Resume SceneObject: {sceneObject.name}");
                //}

                //Debug.Log("Network Runner Raising some event.");
                //_OnHostMigrationEvent.Raise(runner.LocalPlayer, runner);
                //_OnPlayerJoinnedEvent.Raise(runner.LocalPlayer, runner);
                //Debug.Log("Network Runner HostMigrationResume done.");
            }

            

            // async
            public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
            {
                Debug.Log("Network Runner OnHostMigration Callback");

                return;

                // Shutdown old Runner
                //await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

                //// Reload Scene
                //var completedLoad = new TaskCompletionSource<bool>();
                //var sceneAsync = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

                //sceneAsync.completed += (finishOp) => completedLoad.SetResult(true);

                //// Wait scene to reload
                //await completedLoad.Task;

                //// Create a new Runner
                //NetworkRunner newRunner = InstantiateNewRunner();

                ////// Start the new Runner using the "HostMigrationToken" and pass a callback ref in "HostMigrationResume".
                ////StartGameResult result = await newRunner.StartGame(new StartGameArgs()
                ////{
                ////    // SessionName = SessionName,              // ignored, peer never disconnects from the Photon Cloud
                ////    // GameMode = gameMode,                    // ignored, Game Mode comes with the HostMigrationToken
                ////    HostMigrationToken = hostMigrationToken,   // contains all necessary info to restart the Runner
                ////    HostMigrationResume = HostMigrationResume, // this will be invoked to resume the simulation
                ////                                               // other args
                ////});

                //var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

                //// Start new Runner with the Host Migration Token
                //var result = await StartSimulation(
                //  newRunner,
                //  hostMigrationToken.GameMode,           // New Fusion GameMode comes from the Host Migration process
                //  _connectionToken,                      // Make sure we are connecting as a client with the same Token
                //  sceneRef,                              // Load same Scene
                //  migrationToken: hostMigrationToken,    // Host Migration Token contains all necessary for Fusion to re-start
                //  migrationResume: HostMigrationResume);

                //// Check StartGameResult as usual
                //if (result.Ok == false)
                //{
                //    Debug.LogWarning(result.ShutdownReason);
                //}
                //else
                //{
                //    Debug.Log("Network Runner OnHostMigration Done");
                //}
            }

            public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
            public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
            public void OnSceneLoadStart(NetworkRunner runner) {}
            public void OnSceneLoadDone(NetworkRunner runner) {}
        }
    }
}
