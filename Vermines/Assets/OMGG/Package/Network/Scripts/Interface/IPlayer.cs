namespace OMGG.Network {

    /*
     * @brief IPlayer is an interface representing a player connected to the network.
     * It is responsible for storing player-specific data (e.g. name, id).
     * It also enables them to communicate with each other. * 
     */
    public interface IPlayer {

        /*
         * @brief Unique identifier of the player.
         * @note Most of the time, this id represents the player's connection order.
         * 
         * @warning Be careful, although it looks practical it's only between 1 and 4, it represents the connection order but on the game!
         * So it can increase up to 10,000+.
         */
        string PlayerId { get; }

        /*
         * @brief Name of the player.
         */
        string PlayerName { get; }

        /*
         * @brief Method that sends a message to the player.
         * 
         * @param message The message to send. (see INetworkMessage)
         */
        void SendMessage(INetworkMessage message);
    }
}
