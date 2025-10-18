using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System;
using Fusion.Sockets;
using Fusion;
using WebSocketSharp;

namespace Vermines.Core.Network {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Extension;
    using Vermines.Core.Scene;

    public class Networking : MonoBehaviour {

        #region Constant

        public const string STATUS_SERVER_CLOSED = "server_closed";

        public const string DISPLAY_NAME_KEY = "name";
        public const string MODE_KEY         = "mode";

        #endregion

        #region Attributes

        public string Status { get; private set; }
        public string StatusDescription { get; private set; }
        public string ErrorStatus { get; private set; }

        private Session _PendingSession;
        private Session _CurrentSession;

        private bool _StopGameOnDisconnect;

        private string _LoadingScene;

        private Coroutine _Coroutine;

        #endregion

        #region MonoBehaviour Methods

        protected void Awake()
        {
            _LoadingScene = Global.Settings.LoadingScene;
        }

        protected void Update()
        {
            if (_PendingSession != null) {
                if (_CurrentSession == null) {
                    _CurrentSession = _PendingSession;
                    _PendingSession = null;
                } else {
                    // Request the end of the current session.
                    _CurrentSession.ConnectionRequested = false;
                }
            }

            UpdateCurrentSession();

            // Check if current session finished
            if (_Coroutine == null && _CurrentSession != null && !_CurrentSession.IsConnected) {
                if (_PendingSession == null) {
                    Log($"Starting LoadMenuCoroutine().");

                    // Current session is finished and there is no pending session, let's go to menu.
                    _Coroutine = StartCoroutine(LoadMenuCoroutine());
                }

                _CurrentSession = null;
            }
        }

        #endregion

        #region Methods

        #region Game

        public void StartGame(SessionRequest request)
        {
            Session session = new();

            if (request.ExtraPeers > 0 && NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Single) {
                Debug.LogError("Cannot start with multiple peers. PeerMode is set to Single.");

                request.ExtraPeers = 0;
            }
            SceneRef sceneRef = default;

            int sceneIndex = SceneUtility.GetBuildIndexByScenePath(request.ScenePath);

            if (sceneIndex >= 0)
                sceneRef = SceneRef.FromIndex(sceneIndex);
            int totalPeers = 1 + request.ExtraPeers;

            session.GamePeers = new GamePeer[totalPeers];

            NetworkSceneInfo sceneInfo = new();

            if (sceneRef.IsValid)
                sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive, LocalPhysicsMode.None, true);
            for (int i = 0; i < totalPeers; i++) {
                session.GamePeers[i] = new(i) {
                    UserID   = i == 0 ? request.UserID : $"{request.UserID}.{i}",
                    Scene    = sceneInfo,
                    GameMode = i == 0 ? request.GameMode : GameMode.Client,
                    Request  = request
                };
            }
            session.ConnectionRequested = true;

            _PendingSession       = session;
            _StopGameOnDisconnect = false;
            ErrorStatus           = null;

            Log($"StartGame() UserID:{request.UserID} GameMode:{request.GameMode} DisplayName:{request.DisplayName} SessionName:{request.SessionName} ScenePath:{request.ScenePath} MaxPlayers:{request.MaxPlayers} ExtraPeers:{request.ExtraPeers} CustomLobby:{request.CustomLobby}");
        }

        private void StopGame(string errorStatus = null)
        {
            Log($"StopGame()");

            _PendingSession       = null;
            _StopGameOnDisconnect = false;

            if (_CurrentSession != null)
                _CurrentSession.ConnectionRequested = false;
            ErrorStatus = errorStatus;
        }

        public void StopGameOnDisconnect()
        {
            Log($"StopGameOnDisconnect()");

            _StopGameOnDisconnect = true;
        }

        #endregion

        public void UpdateCurrentSession()
        {
            if (_CurrentSession == null) {
                Status            = string.Empty;
                StatusDescription = string.Empty;

                return;
            }

            if (_Coroutine != null)
                return;
            GamePeer[] peers = _CurrentSession.GamePeers;

            if (_StopGameOnDisconnect) {
                for (int i = 0; i < peers.Length; i++) {
                    if (_CurrentSession.ConnectionRequested && !peers[i].IsConnected) {
                        Log($"Stopping game after disconnect.");

                        _StopGameOnDisconnect = false;

                        StopGame();

                        return;
                    }
                }
            }

            for (int i = 0; i < peers.Length; i++) {
                GamePeer    peer = peers[i];
                bool isConnected = peer.IsConnected;

                if (_CurrentSession.ConnectionRequested && !peer.Loaded && !isConnected && peer.CanConnect) { // First connect or reconnect after failed connect
                    Status = !peer.WasConnected ? "starting" : "reconnecting";

                    Log($"Starting ConnectPeerCoroutine() - {Status} - Peer {peer.ID}");

                    _Coroutine = StartCoroutine(ConnectPeerCoroutine(peer));

                    return;
                } else if (!_CurrentSession.ConnectionRequested && (isConnected || peer.Loaded)) { // Disconnect Request
                    Status = "quitting";

                    Log($"Starting DisconnectPeerCoroutine() - {Status} - Peer {peer.ID}");

                    _Coroutine = StartCoroutine(DisconnectPeerCoroutine(peer));

                    return;
                } else if (peer.Loaded && !isConnected) { // Connection Lost
                    Status = "connexion_lost";

                    Log($"Starting DisconnectPeerCoroutine() - {Status} - Peer {peer.ID}");

                    _Coroutine = StartCoroutine(DisconnectPeerCoroutine(peer));

                    return;
                }
            }
        }

        public void ClearErrorStatus()
        {
            ErrorStatus = null;
        }

        #endregion

        #region Coroutine

        #region Peers

        private IEnumerator ConnectPeerCoroutine(GamePeer peer, float connectionTimeout = 10f, float loadTimeout = 45f)
        {
            peer.Loaded = true;

            if (peer.WasConnected)
                peer.ReconnectionTries--;
            else
                peer.ConnectionTries--;
            StatusDescription = "unloading_current_scene";

            UnityScene activeScene = SceneManager.GetActiveScene();

            if (!IsSameScene(activeScene.path, peer.Request.ScenePath) && activeScene.name != _LoadingScene) {
                Log($"Show loading scene.");

                yield return ShowLoadingSceneCoroutine(true);

                bool unloadScene = true;

                for (int i = 0; i < _CurrentSession.GamePeers.Length; i++) {
                    if (activeScene == _CurrentSession.GamePeers[i].LoadedScene) {
                        unloadScene = false;

                        break;
                    }
                }

                if (unloadScene) {
                    Scene currentScene = activeScene.GetComponent<Scene>();

                    if (currentScene != null) {
                        Log($"Deinitializing Scene.");

                        currentScene.Deinitialize();
                    }

                    Log($"Unloading scene {activeScene.name}");

                    yield return SceneManager.UnloadSceneAsync(activeScene);
                    yield return null;
                }
            }
            float baseTime  = Time.realtimeSinceStartup;
            float limitTime = baseTime + connectionTimeout;
            string peerName = $"{peer.GameMode}#{peer.ID}";

            Debug.LogWarning($"Starting {peerName} ...");

            StatusDescription = "starting_network_connection";

            yield return null;

            NetworkObjectPool pool = new();
            NetworkRunner   runner = Instantiate(Global.Settings.RunnerPrefab);

            runner.name = peerName;

            runner.EnableVisibilityExtension();

            peer.Runner       = runner;
            peer.SceneManager = runner.GetComponent<NetworkSceneManager>();
            peer.LoadedScene  = default;

            StartGameArgs startGameArgs = new() {
                GameMode                    = peer.GameMode,
                SessionName                 = peer.Request.SessionName,
                Scene                       = peer.Scene,
                ObjectProvider              = pool,
                CustomLobbyName             = peer.Request.CustomLobby,
                SceneManager                = peer.SceneManager,
                EnableClientSessionCreation = false
            };

            if (peer.Request.MaxPlayers > 0)
                startGameArgs.PlayerCount = peer.Request.MaxPlayers;
            if (peer.GameMode == GameMode.Server || peer.GameMode == GameMode.Host)
                startGameArgs.SessionProperties = CreateSessionProperties(peer.Request);
            if (!peer.Request.IPAddress.IsNullOrEmpty())
                startGameArgs.Address = NetAddress.CreateFromIpPort(peer.Request.IPAddress, peer.Request.Port);
            else if (peer.Request.Port > 0)
                startGameArgs.Address = NetAddress.Any(peer.Request.Port);
            Log($"NetworkRunner.StartGame().");

            var startGameTask = runner.StartGame(startGameArgs);

            while (!startGameTask.IsCompleted) {
                yield return null;

                if (Time.realtimeSinceStartup >= limitTime) {
                    Debug.LogError($"{peerName} start timeout! IsCompleted: {startGameTask.IsCompleted} IsCanceled: {startGameTask.IsCanceled} IsFaulted: {startGameTask.IsFaulted}");

                    break;
                }

                if (_CurrentSession.ConnectionRequested == false) {
                    Log($"Stopping coroutine (requested by user");

                    break;
                }
            }

            if (startGameTask.IsCanceled || startGameTask.IsFaulted || !startGameTask.IsCompleted) {
                Debug.LogError($"{peerName} failed to start!");
                Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}.");

                yield return DisconnectPeerCoroutine(peer);

                _Coroutine = null;

                yield break;
            }

            var result = startGameTask.Result;

            Log($"StartGame() Result: {result} - Peer {peer.ID}.");

            if (!result.Ok) {
                Debug.LogError($"{peerName} failed to start! Result: {result}");

                if (!Application.isBatchMode)
                    StopGame();
                if (peer.WasConnected && result.ShutdownReason == ShutdownReason.GameNotFound)
                    ErrorStatus = STATUS_SERVER_CLOSED;
                else
                    ErrorStatus = StringToLabel(result.ShutdownReason.ToString());
                Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}.");

                yield return DisconnectPeerCoroutine(peer);

                _Coroutine = null;

                yield break;
            }
            limitTime += loadTimeout;

            Log($"Waiting for connection - Peer {peer.ID}.");

            StatusDescription = "waiting_server_connection";

            while (!peer.IsConnected) {
                yield return null;

                if (Time.realtimeSinceStartup >= limitTime) {
                    Debug.LogError($"{peerName} start timeout! IsCloudReady: {runner.IsCloudReady} IsRunning: {runner.IsRunning}");
                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}.");

                    yield return DisconnectPeerCoroutine(peer);

                    _Coroutine = null;

                    yield break;
                }
            }
            Log($"Loading gameplay scene - Peer {peer.ID}");

            StatusDescription = "loading_gameplay_scene";

            while (!runner.SimulationUnityScene.IsValid() || !runner.SimulationUnityScene.isLoaded) {
                Log($"Waiting for NetworkRunner.SimulationUnityScene - Peer {peer.ID}.");

                yield return null;

                if (Time.realtimeSinceStartup >= limitTime) {
                    Debug.LogError($"{peerName} scene load timeout!");
                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}.");

                    yield return DisconnectPeerCoroutine(peer);

                    _Coroutine = null;

                    yield break;
                }
            }

            Debug.LogWarning($"{peerName} started in {(Time.realtimeSinceStartup - baseTime):0.00}s");

            peer.LoadedScene = runner.SimulationUnityScene;

            if (peer.ID == 0)
                SceneManager.SetActiveScene(peer.LoadedScene);
            StatusDescription = "waiting_gameplay_scene_load";

            var scene = peer.SceneManager.GameplayScene;

            while (scene == null) {
                Log($"Waiting for GameplayScene - Peer {peer.ID}.");

                yield return null;

                scene = peer.SceneManager.GameplayScene;

                if (Time.realtimeSinceStartup >= limitTime) {
                    Debug.LogError($"{peerName} GameplayScene query timeout!");
                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}.");

                    yield return DisconnectPeerCoroutine(peer);

                    _Coroutine = null;

                    yield break;
                }
            }

            Log($"Scene.PrepareContext() - Peer {peer.ID}");

            scene.PrepareContext();

            var sceneContext = scene.Context;

            sceneContext.IsVisible  = peer.ID == 0;
            sceneContext.HasInput   = peer.ID == 0;
            sceneContext.Runner     = peer.Runner;
            sceneContext.PeerUserID = peer.UserID;

            peer.Context = sceneContext;
            pool.Context = sceneContext;

            StatusDescription = "waiting_networked_game";

            NetworkGame networkGame = scene.GetComponentInChildren<NetworkGame>(true);

            while (networkGame.Object == null) {
                Log($"Waiting for NetworkGame - Peer {peer.ID}");

                yield return null;

                if (Time.realtimeSinceStartup >= limitTime) {
                    Debug.LogError($"{peerName} start timeout! Network game not started properly.");
                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}.");

                    yield return DisconnectPeerCoroutine(peer);

                    _Coroutine = null;

                    yield break;
                }

                if (!_CurrentSession.ConnectionRequested) {
                    Log($"Starting DisconnectPeerCoroutine() - Connection is not requested anymore - Peer {peer.ID}.");

                    yield return DisconnectPeerCoroutine(peer);

                    _Coroutine = null;

                    yield break;
                }
            }

            StatusDescription = "waiting_gameplay_load";

            Log($"NetworkGame.Initialize() - Peer {peer.ID}");

            networkGame.Initialize();

            // TODO: Implement GameplayMode

            StatusDescription = "activating_scene";

            Log($"Scene.Initialize() - Peer {peer.ID}");

            scene.Initialize();

            Log($"Scene.Activate() - Peer {peer.ID}");

            yield return scene.Activate();

            StatusDescription = "activating_network_game";

            Log($"NetworkGame.Activate() - Peer {peer.ID}");

            networkGame.Activate();

            if (SceneManager.GetSceneByName(_LoadingScene).IsValid()) {
                yield return new WaitForSeconds(1f);

                Log($"Hide loading scene");

                yield return ShowLoadingSceneCoroutine(false);
            }

            if (peer.WasConnected)
                peer.ReconnectionTries++;
            peer.WasConnected = true;

            _Coroutine = null;

            Log($"ConnectPeerCoroutine() finished.");
        }

        private IEnumerator DisconnectPeerCoroutine(GamePeer peer)
        {
            StatusDescription = "disconnect_server";

            UnityScene gameplayScene = default;

            try {
                if (peer.Runner != null) {
                    gameplayScene = peer.Runner.SimulationUnityScene;

                    if (peer.Runner.IsServer && peer.Runner.SessionInfo != null) {
                        Log($"Closing the room.");

                        peer.Runner.SessionInfo.IsOpen    = false;
                        peer.Runner.SessionInfo.IsVisible = false;
                    }
                }
            } catch (Exception exception) {
                Debug.LogException(exception);
            }

            if (!gameplayScene.IsValid())
                gameplayScene = peer.LoadedScene;
            if (gameplayScene.IsValid()) {
                Scene scene = gameplayScene.GetComponent<Scene>(true);

                if (scene != null) {
                    try {
                        Log($"Deinitialize scene.");

                        scene.Deinitialize();
                    } catch (Exception exception) {
                        Debug.LogException(exception);
                    }
                }
            }
            Task shutdownTask = null;

            if (peer.Runner != null) {
                Debug.LogWarning($"Shutdown {peer.Runner.name} ...");

                try {
                    shutdownTask = peer.Runner.Shutdown(true);
                } catch (Exception exception) {
                    Debug.LogException(exception);
                }
            }
            Log($"Show loading scene.");

            yield return ShowLoadingSceneCoroutine(true);

            if (shutdownTask != null) {
                for (float operationTimeout = 10.0f; operationTimeout > 0.0f && !shutdownTask.IsCompleted; operationTimeout -= Time.unscaledTime)
                    yield return null;
            }
            StatusDescription = "unloading_gameplay_scene";

            yield return null;

            if (gameplayScene.IsValid()) {
                Debug.LogWarning($"Unloading scene {gameplayScene.name}");

                yield return SceneManager.UnloadSceneAsync(gameplayScene);
                yield return null;
            }
            peer.Loaded       = default;
            peer.Runner       = default;
            peer.SceneManager = default;
            peer.LoadedScene  = default;

            _Coroutine = null;

            Log($"DisconnectPeerCoroutine() finished.");
        }

        #endregion

        private IEnumerator LoadMenuCoroutine()
        {
            string menuSceneName = Global.Settings.MenuScene;

            if (SceneManager.sceneCount == 1 && SceneManager.GetSceneAt(0).name == menuSceneName) {
                _Coroutine = null;

                yield break;
            }
            StatusDescription = "unloading_gameplay_scene";

            yield return ShowLoadingSceneCoroutine(true);

            for (int i = SceneManager.sceneCount - 1; i >= 0; i--) {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.name != _LoadingScene)
                    yield return SceneManager.UnloadSceneAsync(scene);
            }
            StatusDescription = "loading_menu_scene";

            yield return null;
            yield return SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
            yield return ShowLoadingSceneCoroutine(false);

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(menuSceneName));

            _Coroutine = null;
        }

        private IEnumerator ShowLoadingSceneCoroutine(bool show, float additionalTime = 1f)
        {
            UnityScene loadingScene = SceneManager.GetSceneByName(_LoadingScene);

            if (!loadingScene.IsValid()) {
                yield return SceneManager.LoadSceneAsync(_LoadingScene, LoadSceneMode.Additive);

                loadingScene = SceneManager.GetSceneByName(_LoadingScene);
            }
            
            if (!show && additionalTime > 0f)
                yield return new WaitForSeconds(additionalTime); // Wait additional time till fade out start.
            yield return null;

            LoadingScene loadingSceneObject = loadingScene.GetComponent<LoadingScene>();

            if (loadingSceneObject != null) {
                if (show)
                    loadingSceneObject.FadeIn();
                else
                    loadingSceneObject.FadeOut();
                while (loadingSceneObject.IsFading)
                    yield return null;
            }

            if (show && additionalTime > 0f)
                yield return new WaitForSeconds(additionalTime); // Wait additional time after fade in.
            if (!show)
                yield return SceneManager.UnloadSceneAsync(loadingScene);
        }

        #endregion

        #region Utils

        private Dictionary<string, SessionProperty> CreateSessionProperties(SessionRequest request)
        {
            return new Dictionary<string, SessionProperty> {
                [DISPLAY_NAME_KEY] = request.DisplayName,
                [MODE_KEY]         = (int)request.GameMode
            };
        }

        private static string StringToLabel(string str)
        {
            var label = System.Text.RegularExpressions.Regex.Replace(str, "(?<=[A-Z@])(?=[A-Z][a-z])", " ");

            label = System.Text.RegularExpressions.Regex.Replace(label, "(?<=[^A-Z])(?=[A-Z])", " ");

            return label;
        }

        #endregion

        #region Comparator

        private static bool IsSameScene(string assetPath, string scenePath)
        {
            return assetPath == $"Assets/{scenePath}.unity";
        }

        #endregion

        #region Logs

        [System.Diagnostics.Conditional("ENABLE_LOGS")]
        private void Log(string message)
        {
            Debug.Log($"[{Time.realtimeSinceStartup:F3}][{Time.frameCount}] Networking({GetInstanceID()}: {message}");
        }

        #endregion
    }
}
