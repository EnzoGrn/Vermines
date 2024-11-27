using System;

namespace OMGG.Network {

    /*
     * @brief INetworkStatistics is an interface for network statistics.
     * It provides the ping, packet loss and bandwidth usage.
     */
    public interface INetworkStatistics {

        /*
         * @brief Ping represents the round trip time in milliseconds.
         */
        float Ping { get; }

        /*
         * @brief PacketLoss represents the percentage of packets lost.
         */
        int PacketLoss { get; }

        /*
         * @brief BandwidthUsage represents the bandwidth usage in kbps.
         */
        float BandwidthUsage { get; }

        /*
         * @brief OnStatisticsUpdated is triggered when the statistics are updated.
         */
        event Action OnStatisticsUpdated;
    }
}
