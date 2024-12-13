using OMGG.DesignPattern;
using Fusion.Sockets;
using Fusion;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace OMGG.Network.Fusion {

    public class NetworkRunnerManager : MonoBehaviourSingleton<NetworkRunnerManager> {

        /*
         * @brief NetworkRunner is the main class that manages the connection to the server.
         * But also create simulations and manage the game.
         */
        public NetworkRunner Runner { get; private set; }

        private void Start()
        {
            GetRunner();
        }

        /*
         * @brief Function for start a runner with the given parameters.
         * Runner means in Fusion, a simulation of a game.
         */
        public async Task StartRunner(GameMode mode, NetAddress address, RoomOptions options)
        {
            if (Runner == null)
                GetRunner();
            if (!Runner || Runner.IsRunning)
                return;
            var result = await Runner.StartGame(new StartGameArgs() {
                SessionName = options.RoomName,
                GameMode = mode,
                Address = address
            });
                
            if (result.Ok == false) {
                Debug.LogError($"Failed to start the NetworkRunner: {result.ShutdownReason}");

                throw new Exception($"Failed to start the NetworkRunner: {result.ShutdownReason}");
            }
        }

        /*
         * @brief Function for shutdown the runner.
         */
        public void Shutdown()
        {
            Runner.Shutdown();
        }

        /*
         * @brief Function for get the runner.
         * This getter, is called when the runner is null.
         */
        private void GetRunner()
        {
            if (Runner == null) {
                NetworkRunner runner = (NetworkRunner)FindAnyObjectByType(typeof(NetworkRunner));

                if (runner == null) {
                    GameObject go = new("NetworkRunner");

                    Runner = go.AddComponent<NetworkRunner>();
                } else {
                    Runner = runner;
                }
            }
        }
    }

    public class FusionNetworkManager : INetworkManager {

        /*
         * @brief The current connection state of the client.
         * @see ConnectionState
         */
        private ConnectionState _ConnectionState = ConnectionState.Disconnected;

        /*
         * @brief Boolean that indicates if our client is connected to the server.
         */
        public bool IsConnected => _ConnectionState == ConnectionState.Connected;

        /*
         * @brief The name of the session that the client is connected to.
         */
        private readonly string _SessionName;

        /*
         * @brief Constructor of the FusionNetworkManager class.
         * It initializes the NetworkRunner object.
         */
        public FusionNetworkManager(string sessionName /* Vermines, Monopoly, Uno etc... */)
        {
            _SessionName = sessionName;
        }

        /*
         * @brief Connects the client to the server.
         * @note The port & auth parameters are not used in the current Fusion implementation.
         * The address is for now not used too, but it may be used in the future.
         */
        public async void Connect(string address = null, int port = 0, string auth = null)
        {
            _ConnectionState = ConnectionState.Connecting;

            try {
                await NetworkRunnerManager.Instance.StartRunner(GameMode.Shared, NetAddress.Any(), new RoomOptions() {
                    RoomName = _SessionName
                });

                _ConnectionState = ConnectionState.Connected;

                OnConnected?.Invoke();
            } catch (Exception e) {
                _ConnectionState = ConnectionState.Disconnected;

                OnError?.Invoke(e.Message);
            }
        }

        public void Disconnect()
        {
            NetworkRunnerManager.Instance.Shutdown();

            _ConnectionState = ConnectionState.Disconnected;

            OnDisconnected?.Invoke();
        }

        public ConnectionState GetConnectionState() => _ConnectionState;

        #region Callback Actions

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        #endregion
    }
}
