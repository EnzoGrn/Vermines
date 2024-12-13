namespace OMGG.Network {

    /*
     * @brief Interface for the INetworkRoomView.
     * It is used to communicate with the NetworkRoom class.
     */
    public interface INetworkRoomView {

        /*
         * @brief Show a message to the user.
         * For example, it can be used as debug, for showing the some information to the screen.
         */
        void ShowMessage(string message);

        /*
         * @brief Show the player that joined the room.
         */
        void ShowPlayerJoined(IPlayer player);

        /*
         * @brief Show the player that left the room.
         */
        void ShowPlayerLeft(IPlayer player);
    }
}
