using System;

namespace OMGG.Network.Fusion {

    public class FusionMessageSerializer : IMessageSerializer {

        public INetworkMessage Deserialize(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize(INetworkMessage message)
        {
            throw new NotImplementedException();
        }
    }

    public class FusionNetworkMessage : INetworkMessage {

        public byte[] Payload => throw new NotImplementedException();

        public byte ChannelId => throw new NotImplementedException();
    }
}
