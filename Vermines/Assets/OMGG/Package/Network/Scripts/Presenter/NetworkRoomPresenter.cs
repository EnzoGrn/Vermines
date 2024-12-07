namespace OMGG.Network {

    /*
     * @brief Presenter for the NetworkRoom.
     * The presenter is one of the three components of the MVP pattern.
     * It's role is to handle the communication between the Model and the View.
     */
    public class NetworkRoomPresenter {

        /*
         * @brief The model of the presenter.
         * It contains the logic of the NetworkRoom.
         */
        private readonly INetworkRoom _Model;

        /*
         * @brief The view of the presenter.
         * It is used to communicate with the NetworkRoom.
         */
        private readonly INetworkRoomView _View;

        /*
         * @brief Constructor of the NetworkRoomPresenter.
         * It initializes the model and the view.
         */
        public NetworkRoomPresenter(INetworkRoom roomModel, INetworkRoomView roomView)
        {
            _Model = roomModel;
            _View  = roomView;

            _Model.OnPlayerJoined += player => _View.ShowPlayerJoined(player);
            _Model.OnPlayerLeft   += player => _View.ShowPlayerLeft(player);
        }

        /*
         * @brief Create a room with the given name and options.
         */
        public void CreateRoom(string roomName, RoomOptions options)
        {
            _View.ShowMessage($"Creating room: {roomName}");

            _Model.CreateRoom(roomName, options);
        }

        /*
         * @brief Join a room with the given name.
         */
        public void JoinRoom(string roomName)
        {
            _View.ShowMessage($"Joining room: {roomName}");

            _Model.JoinRoom(roomName);
        }

        /*
         * @brief Leave the current room.
         */
        public void LeaveRoom()
        {
            _View.ShowMessage("Leaving room...");

            _Model.LeaveRoom();
        }
    }
}
