using System;

namespace OMGG.Network {

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
        void Connect(string address, string auth);
        void Connect(string address, int port);
        void Connect();
        // Do not hesitate to add more Connect functions if needed.

        /*
         * @brief Method that disconnects from the server.
         */
        void Disconnect();

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
