namespace OMGG.Network {

    /*
     * @brief Presenter for the NetworkManager.
     * The presenter is one of the three components of the MVP pattern.
     * It's role is to handle the communication between the Model and the View.
     */
    public class NetworkManagerPresenter {

        /*
         * @brief The model of the presenter.
         * It contains the logic of the NetworkManager.
         */
        private readonly INetworkManager _Model;

        /*
         * @brief The view of the presenter.
         * It is used to communicate with the NetworkManager.
         */
        private readonly INetworkManagerView _View;

        /*
         * @brief Check if the NetworkManager is connected to the server.
         */
        public bool IsConnected => _Model.IsConnected;

        /*
         * @brief Constructor of the NetworkManagerPresenter.
         * It initializes the model and the view.
         */
        public NetworkManagerPresenter(INetworkManager model, INetworkManagerView view)
        {
            _Model = model;
            _View  = view;
        }

        /*
         * @brief Start the connection to the server.
         * It calls the Connect method of the model.
         */
        public void StartConnection(string address = null, int port = 0, string auth = null)
        {
            _View.ShowMessage("Connecting to the server..."); // Can remove this line, it's not necessary, but can be display during the launch of the game with a loading screen.
            _Model.Connect(address, port, auth);
        }

        /*
         * @brief Stop the connection to the server.
         * It calls the Disconnect method of the model.
         */
        public void StopConnection()
        {
            _Model.Disconnect();
        }
    }
}
