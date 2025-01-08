namespace OMGG.Network {

    /*
     * @brief Interface for the INetworkManager.
     * It is used to communicate with the NetworkManager.
     * He is a view, so he can show messages and errors.
     */
    public interface INetworkManagerView {

        /*
         * @brief Show a message to the user.
         * For example, it can be used as debug, for showing the some information to the screen.
         */
        void ShowMessage(string message);

        /*
         * @brief Show an error to the user.
         * Call when the NetworkManager has an error (e.g. connection error).
         */
        void ShowError(string message);
    }
}
