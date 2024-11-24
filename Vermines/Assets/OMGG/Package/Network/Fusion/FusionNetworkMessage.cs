using System;

namespace OMGG.Network.Fusion {

    public class FusionNetworkMessage : INetworkMessage {

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
