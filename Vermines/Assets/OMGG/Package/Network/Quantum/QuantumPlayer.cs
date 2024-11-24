using System;

namespace OMGG.Network.Quantum {

    public class QuantumPlayer : IPlayer {

        public string PlayerId => throw new NotImplementedException();

        public string PlayerName => throw new NotImplementedException();

        public void SendMessage(INetworkMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
