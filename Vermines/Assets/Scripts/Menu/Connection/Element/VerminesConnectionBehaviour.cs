using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Fusion.Photon.Realtime;
using Fusion;
using UnityEngine.SceneManagement;
using UnityEngine;
using OMGG.Menu.Connection.Data;
using OMGG.Menu.Configuration;
using OMGG.Menu.Connection;
using OMGG.Scene.Data;
using OMGG.Menu.Region;

namespace Vermines.Menu.Connection.Element {

    using Vermines.Menu.MPPM;

    public class VerminesConnectionBehaviour : OMGG.Menu.Connection.Element.MenuConnectionBehaviour {

        #region Serialized Fields

        [SerializeField]
        private ServerConfig _Config;

        [SerializeField]
        private Camera _MenuCamera;

        [SerializeField]
        private NetworkPrefabRef _PlayerListService;

        [Tooltip("The network runner prefabs to use for the connection.")]
        [SerializeField]
        private GameObject _NetworkRunner;

        #endregion

        #region Non-Serialized Fields

        [NonSerialized]
        private NetworkRunner _Runner;

        [NonSerialized]
        private string _SessionName;

        public override string SessionName => _SessionName;

        [NonSerialized]
        private string _LobbyCustomName;

        public override string LobbyCustomName => _LobbyCustomName;

        public bool IsCustomLobby => LobbyCustomName != null && LobbyCustomName.Length > 0;

        [NonSerialized]
        private int _MaxPlayerCount;
        public override int MaxPlayerCount => _MaxPlayerCount;

        [NonSerialized]
        private string _Region;
        public override string Region => _Region;

        [NonSerialized]
        private string _AppVersion;
        public override string AppVersion => _AppVersion;

        [NonSerialized]
        private List<string> _Usernames;
        public override List<string> Usernames => _Usernames;

        [NonSerialized]
        private bool _ConnectingSafeCheck;

        [NonSerialized]
        private CancellationTokenSource _CancellationTokenSource;

        [NonSerialized]
        private CancellationToken _CancellationToken;

        #endregion

        #region Getters

        public override bool IsConnected => _Runner && _Runner.IsRunning;

        public override int Ping => (int)(IsConnected ? _Runner.GetPlayerRtt(_Runner.LocalPlayer) * 1000 : 0);

        #endregion

        #region Setters

        private void ToggleMenuCamera(bool value)
        {
            _MenuCamera.gameObject.SetActive(value);
        }

        private void DisableMenuCamera()
        {
            ToggleMenuCamera(false);
        }

        private void EnableMenuCamera(int error)
        {
            ToggleMenuCamera(true);
        }

        public void SetSessionUsernames(List<string> usernames)
        {
            _Usernames = usernames;
        }

        #endregion

        #region Overrides Methods

        private void Awake()
        {
            if (!_MenuCamera)
                _MenuCamera = Camera.current;
            if (!_Config)
                Log.Error("Fusion menu configuration file not provided.");
            OnBeforeDisconnect.AddListener(EnableMenuCamera);
        }

        protected override async Task<ConnectResult> ConnectAsyncInternal(ConnectionArgs connectArgs, SceneRef sceneRef, bool isCustom = false)
        {
            if (_ConnectingSafeCheck) {
                return new ConnectResult {
                    CustomResultHandling = true,
                    Success = false,
                    FailReason = ConnectFailReason.None
                };
            }

            _ConnectingSafeCheck = true;

            if (_Runner && _Runner.IsRunning) {
                await _Runner.Shutdown();

                await Task.Delay(100);
            }

            _Runner = Instantiate(_NetworkRunner).GetComponent<NetworkRunner>();

            // Prepare Runner object
            NetworkSceneManagerDefault sceneManager = _Runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            sceneManager.IsSceneTakeOverEnabled = false;

            // Copy and update AppSettings
            FusionAppSettings appSettings = CopyAppSettings(connectArgs);

            // Solve StartGameArgs
            StartGameArgs args = new() {
                OnGameStarted = SpawnPlayerListService,
                CustomPhotonAppSettings = appSettings,
                GameMode = connectArgs.GameMode ?? ResolveGameMode(connectArgs),
                SessionName = _SessionName = connectArgs.Session,
                PlayerCount = _MaxPlayerCount = connectArgs.MaxPlayerCount,
                CustomLobbyName = _LobbyCustomName = isCustom ? "CustomLobby" : null
            };

            // Scene info
            NetworkSceneInfo sceneInfo = new();

            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);

            args.Scene = sceneInfo;

            // Cancellation Token
            _CancellationTokenSource?.Dispose();

            _CancellationTokenSource = new CancellationTokenSource();
            _CancellationToken = _CancellationTokenSource.Token;

            args.StartGameCancellationToken = _CancellationToken;

            int regionIndex = _Config.AvailableRegions.IndexOf(connectArgs.Region);

            args.SessionNameGenerator = () => _Config.CodeGenerator.EncodeRegion(_Config.CodeGenerator.Create(), regionIndex);

            StartGameResult startGameResult = default;
            ConnectResult connectResult = new();

            startGameResult = await _Runner.StartGame(args);

            connectResult.Success = startGameResult.Ok;
            connectResult.FailReason = ResolveConnectFailReason(startGameResult.ShutdownReason);

            _ConnectingSafeCheck = false;

            if (connectResult.Success)
                _SessionName = _Runner.SessionInfo.Name;
            if (!isCustom) { // Only force join the host if it's a matchmaking session
                FusionMppm.MainEditor?.Send(new VerminesMPPMCommand() {
                    Region       = _Region,
                    Session      = _SessionName,
                    AppVersion   = _AppVersion,
                    IsSharedMode = args.GameMode == GameMode.Shared,
                });
            }

            return connectResult;
        }

        protected override async Task<ConnectResult> ChangeSceneInternal(SceneInformation sceneInfo, List<SceneInformation> sceneInfos)
        {
            ConnectResult result = new() {
                CustomResultHandling = true,
                Success              = false,
                FailReason           = ConnectFailReason.None
            };

            if (_Runner == null || !_Runner.IsRunning) {
                Log.Error("Cannot change scene because NetworkRunner is not running.");

                result.FailReason = ConnectFailReason.Disconnect;

                return result;
            } else if (string.IsNullOrEmpty(sceneInfo.ScenePath)) {
                if (sceneInfos.Count > 1)
                    sceneInfo = sceneInfos[UnityEngine.Random.Range(1, sceneInfos.Count)];
                else {
                    result.FailReason = ConnectFailReason.ArgumentError;

                    return result;
                }
            }

            await _Runner.LoadScene(sceneInfo.ScenePath, LoadSceneMode.Additive);

            result.Success = true;
            result.Data    = sceneInfo.ScenePath;

            return result;
        }

        protected override async Task<ConnectResult> ChangeSceneInternal(SceneRef sceneRef)
        {
            ConnectResult result = new() {
                CustomResultHandling = true,
                Success              = false,
                FailReason           = ConnectFailReason.None
            };

            if (_Runner == null || !_Runner.IsRunning) {
                Log.Error("Cannot change scene because NetworkRunner is not running.");

                result.FailReason = ConnectFailReason.Disconnect;

                return result;
            } else if (sceneRef.IsValid == false) {
                result.FailReason = ConnectFailReason.ArgumentError;

                return result;
            }

            await _Runner.LoadScene(sceneRef, LoadSceneMode.Additive);

            result.Success = true;

            return result;
        }

        protected override async Task<bool> UnloadSceneInternal(SceneRef sceneRef)
        {
            if (_Runner == null || !_Runner.IsRunning) {
                Log.Error("Cannot unload scene because NetworkRunner is not running.");

                return false;
            } else if (sceneRef.IsValid == false) {
                Log.Error("Cannot unload scene because scene reference is invalid.");

                return false;
            }

            await _Runner.UnloadScene(sceneRef);

            return true;
        }


        protected override async Task DisconnectAsyncInternal(int reason)
        {
            var peerMode = _Runner.Config?.PeerMode;

            _CancellationTokenSource.Cancel();

            await _Runner.Shutdown(shutdownReason: ResolveShutdownReason(reason));

            if (peerMode is NetworkProjectConfig.PeerModes.Multiple)
                return;
        }

        public override Task<List<OnlineRegion>> RequestAvailableOnlineRegionsAsync(ConnectionArgs connectArgs)
        {
            return Task.FromResult(new List<OnlineRegion>() {
                new() {
                    Code = string.Empty,
                    Ping = 0
                }
            });
        }

        #endregion

        #region Methods

        private void SpawnPlayerListService(NetworkRunner runner)
        {
            runner.SpawnAsync(_PlayerListService);
        }

        private GameMode ResolveGameMode(ConnectionArgs args)
        {
            // Create session
            if (args.Creating)
                return GameMode.Host;

            // Quick join session
            if (string.IsNullOrEmpty(args.Session))
                return GameMode.AutoHostOrClient;

            // Join session by code
            return GameMode.Client;
        }

        private ShutdownReason ResolveShutdownReason(int reason)
        {
            switch (reason) {
                case ConnectFailReason.UserRequest:
                    return ShutdownReason.Ok;
                case ConnectFailReason.ApplicationQuit:
                    return ShutdownReason.Ok;
                case ConnectFailReason.Disconnect:
                    return ShutdownReason.DisconnectedByPluginLogic;
                case ConnectFailReason.ArgumentError:
                    return ShutdownReason.Error;
                case ConnectFailReason.GameEnded:
                    return ShutdownReason.DisconnectedByPluginLogic;
                default:
                    return ShutdownReason.Error;
            }
        }

        private int ResolveConnectFailReason(ShutdownReason reason)
        {
            switch (reason) {
                case ShutdownReason.Ok:
                case ShutdownReason.OperationCanceled:
                    return ConnectFailReason.UserRequest;

                case ShutdownReason.DisconnectedByPluginLogic:
                case ShutdownReason.Error:
                    return ConnectFailReason.Disconnect;

                default:
                    return ConnectFailReason.None;
            }
        }

        private FusionAppSettings CopyAppSettings(ConnectionArgs connectArgs)
        {
            FusionAppSettings appSettings = new();

            PhotonAppSettings.Global.AppSettings.CopyTo(appSettings);

            appSettings.FixedRegion = _Region     = connectArgs.Region;
            appSettings.AppVersion  = _AppVersion = connectArgs.AppVersion;

            return appSettings;
        }

        #endregion
    }
}
