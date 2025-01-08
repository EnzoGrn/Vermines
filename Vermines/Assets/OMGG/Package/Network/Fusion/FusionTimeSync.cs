using System;

namespace OMGG.Network.Fusion {

    public class FusionTimeSync : ITimeSync {

        public float NetworkTime => throw new NotImplementedException();

        public event Action<float> OnTimeSynced;

        public void SyncTime()
        {
            throw new NotImplementedException();
        }
    }
}
