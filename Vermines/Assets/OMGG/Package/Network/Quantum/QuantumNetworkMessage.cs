using System;

namespace OMGG.Network.Quantum {

    public class QuantumNetworkMessage : INetworkMessage {

        public byte[] Payload => throw new NotImplementedException();

        public byte ChannelId => throw new NotImplementedException();

        public void Deserialize(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
