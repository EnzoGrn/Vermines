using System;

namespace OMGG.Network.Fusion {

    public class FusionPlayer : IPlayer {

        public string PlayerId => throw new NotImplementedException();

        public string PlayerName => throw new NotImplementedException();

        public bool IsLocal => throw new NotImplementedException();

        public event Action<INetworkMessage> OnMessageReceived;

        public void SendMessage(INetworkMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
