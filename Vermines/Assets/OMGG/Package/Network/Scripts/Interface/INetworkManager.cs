using System;

namespace OMGG.Network {

    /*
     * @brief ConnectionState is an enum that represents the different states of the network connection.
     * Do not hesitate for adding more states if needed.
     */
    public enum ConnectionState {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        // Add more states if needed.
    }

    /*
     * @brief INetworkManager is an interface that handles the network connection management.
     * He is responsible for connection/disconnection.
     * It provides connection status, and issues connection/disconnection events.
     */
    public interface INetworkManager {

        /*
         * @brief Boolean value that indicates if the network is connected or not.
         */
        bool IsConnected { get; }

        /*
         * @brief Method that connects to the server.
         * @note There is a lot of Connect functions because the user can connect in different ways.
         * And depending on the network library, the connection can be made in different ways.
         */
        void Connect(string address = null, int port = 0, string auth = null);
        // Do not hesitate to add more Connect functions if needed.

        /*
         * @brief Method that disconnects from the server.
         */
        void Disconnect();

        /*
         * @brief Method that returns the current connection state.
         */
        ConnectionState GetConnectionState();

        /*
         * @brief Event that is triggered when the network is connected.
         */
        event Action OnConnected;

        /*
         * @brief Event that is triggered when the network is disconnected.
         */
        event Action OnDisconnected;

        /*
         * @brief Event that is triggered when a network error occurs.
         */
        event Action<string> OnError;
    }
}
