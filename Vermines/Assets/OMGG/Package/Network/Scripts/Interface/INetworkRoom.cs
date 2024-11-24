using System.Collections.Generic;

namespace OMGG.Network {

    public class RoomOptions {

        /*
         * @brief Boolean that indicates if the room is visible or not (public or private)
         */
        public bool IsVisible { get; set; }

        /*
         * @brief Boolean that indicates if the room is open or not (joinable or not)
         * For example, a room can be visible but not open, meaning that you can see it but not join it
         * That can append when the party is full or when the game has already started
         */
        public bool IsOpen { get; set; }

        /*
         * @brief Maximum number of players that can join the room
         */
        public int MaxPlayers { get; set; }

        /*
         * @brief Minimum number of players that can join the room
         */
        public int MinPlayers { get; set; }

        /*
         * @brief Custom properties of the room
         * It can be used to store any kind of data you want to share with the players
         */
        public Dictionary<string, object> CustomProperties { get; set; }
    }

    /*
     * @brief INetworkRoom is an interface for managing rooms or lobbies.
     * It allows you to create, join or even leave one.
     * It can also manage the room's parameters and players.
     */
    public interface INetworkRoom {

        /*
         * @brief Unique identifier of the room
         */
        string RoomId { get; }

        /*
         * @brief List of every player currently in the room
         */
        IEnumerable<IPlayer> Players { get; }

        /*
         * @brief Create a room with the given name and options (see RoomOptions)
         */
        void CreateRoom(string roomName, RoomOptions options);

        /*
         * @brief Join the room with the given name
         */
        void JoinRoom(string roomName);

        /*
         * @brief Leave the current room
         */
        void LeaveRoom();
    }
}
