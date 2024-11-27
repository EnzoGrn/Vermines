namespace OMGG.Network {

    /*
     * @brief IMessageSerializer is an interface that represents the structure for message serialization.
     * It can Serialize and Deserialize messages, it's linked to INetworkMessage.
     */
    public interface IMessageSerializer {

        /*
         * @brief Method that serializes the message.
         */
        public byte[] Serialize(INetworkMessage message);

        /*
         * @brief Method that deserializes the message.
         */
        INetworkMessage Deserialize(byte[] data);
    }

    /*
     * @brief INetworkMessage is an interface that represents the structure for network messages.
     */
    public interface INetworkMessage {

        /*
         * @brief The data of the message.
         */
        byte[] Payload { get; }

        /*
         * @brief The channel id of the message.
         */
        byte ChannelId { get; }
    }
}
