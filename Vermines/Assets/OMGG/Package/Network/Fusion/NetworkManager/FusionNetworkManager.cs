using Fusion;
using System;
using System.Diagnostics;

namespace OMGG.Network.Fusion {

    public class FusionNetworkManager : INetworkManager {

        /*
         * @brief NetworkRunner is the main class that manages the connection to the server.
         * Is allow to send and receive messages.
         */
        private NetworkRunner _NetworkRunner;

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
        public FusionNetworkManager(NetworkRunner runner, string sessionName /* Vermines, Monopoly, Uno etc... */)
        {
            _NetworkRunner = runner;
            _SessionName   = sessionName;
        }

        /*
         * @brief Connects the client to the server.
         * @note The port & auth parameters are not used in the current Fusion implementation.
         * The address is for now not used too, but it may be used in the future.
         */
        public void Connect(string address = null, int port = 0, string auth = null)
        {
            _ConnectionState = ConnectionState.Connecting;

            _NetworkRunner.StartGame(new StartGameArgs {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = _SessionName,
                // Address = address /* Address  */ - In NetAddress type
            }).ContinueWith(task => {
                if (task.IsCompletedSuccessfully) {
                    _ConnectionState = ConnectionState.Connected;

                    OnConnected?.Invoke();
                } else {
                    _ConnectionState = ConnectionState.Disconnected;

                    OnError?.Invoke(task.Exception.Message ?? "Unknown error");
                }
            });
        }

        public void Disconnect()
        {
            _NetworkRunner.Shutdown();

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
