using System;

namespace OMGG.Network.Fusion {

    public class FusionNetworkStatistics : INetworkStatistics {

        public float Ping => throw new NotImplementedException();

        public int PacketLoss => throw new NotImplementedException();

        public float BandwidthUsage => throw new NotImplementedException();

        public event Action OnStatisticsUpdated;
    }
}
