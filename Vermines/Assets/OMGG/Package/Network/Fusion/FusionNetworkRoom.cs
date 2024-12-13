using System.Collections.Generic;
using System;
using Fusion;
using Fusion.Sockets;

namespace OMGG.Network.Fusion {

    public class FusionNetworkRoom : INetworkRoom, IPlayerJoined, IPlayerLeft {

        /*
         * @brief The current room options (name, max players, etc...).
         * @see RoomOptions
         */
        private RoomOptions _CurrentRoomOptions;

        /*
         * @brief RoomId is the name of the session that the client is connected to.
         * @note It returns a string.Empty if the client is not connected to any room.
         */
        public string RoomId => NetworkRunnerManager.Instance.Runner.SessionInfo?.Name ?? string.Empty;

        /*
         * @brief The list of players that are currently in the room.
         */
        private readonly List<IPlayer> _Players = new();
        public IEnumerable<IPlayer> Players => _Players;

        /*
         * @brief Create a room with the given name and options.
         */
        public async void CreateRoom(string roomName, RoomOptions options)
        {
            _CurrentRoomOptions = options;

            try
            {
                await NetworkRunnerManager.Instance.StartRunner(GameMode.Host, NetAddress.Any(), _CurrentRoomOptions);
            } catch (Exception e) {
                UnityEngine.Debug.LogWarning(e.Message);
            }
        }

        /*
         * @brief Join a room with the given name.
         */
        public async void JoinRoom(string roomName)
        {
            _CurrentRoomOptions = new RoomOptions() {
                RoomName = roomName
            };

            try {
                await NetworkRunnerManager.Instance.StartRunner(GameMode.Client, NetAddress.Any(), _CurrentRoomOptions);
            } catch (Exception e) {
                UnityEngine.Debug.LogWarning(e.Message);
            }
        }

        /*
         * @brief Leave the current room.
         */
        public void LeaveRoom()
        {
            NetworkRunnerManager.Instance.Shutdown();
        }

        /*
         * @brief Callback from Fusion Core when a player joined the room.
         */
        public void PlayerJoined(PlayerRef player)
        {
            IPlayer newPlayer = new FusionPlayer(player);

            _Players.Add(newPlayer);

            OnPlayerJoined?.Invoke(newPlayer);
        }

        /*
         * @brief Callback from Fusion Core when a player left the room.
         */
        public void PlayerLeft(PlayerRef player)
        {
            IPlayer existingPlayer = _Players.Find(p => p.PlayerId == player.PlayerId.ToString());

            if (existingPlayer != null) {
                _Players.Remove(existingPlayer);

                OnPlayerLeft?.Invoke(existingPlayer);
            }
        }

        #region Callback Actions

        public event Action<IPlayer> OnPlayerJoined;
        public event Action<IPlayer> OnPlayerLeft;

        #endregion
    }
}
