using System.Collections.Generic;
using System;

namespace OMGG.Network.Fusion {

    public class FusionNetworkRoom : INetworkRoom {

        public string RoomId => throw new NotImplementedException();

        public IEnumerable<IPlayer> Players => throw new NotImplementedException();

        public event Action<IPlayer> OnPlayerJoined;
        public event Action<IPlayer> OnPlayerLeft;

        public void CreateRoom(string roomName, RoomOptions options)
        {
            throw new NotImplementedException();
        }

        public void JoinRoom(string roomName)
        {
            throw new NotImplementedException();
        }

        public void LeaveRoom()
        {
            throw new NotImplementedException();
        }
    }
}
