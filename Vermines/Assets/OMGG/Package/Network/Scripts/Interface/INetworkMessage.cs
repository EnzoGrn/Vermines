namespace OMGG.Network {

    /*
     * @brief INetworkMessage is an interface that represents the structure for network messages.
     * It can Serialize/deserialize data, And define communication channels.
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

        /*
         * @brief Method that serializes the message.
         */
        void Serialize();

        /*
         * @brief Method that deserializes the message.
         */
        void Deserialize(byte[] data);
    }
}
