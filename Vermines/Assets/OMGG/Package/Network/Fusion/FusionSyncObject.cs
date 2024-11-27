using System.Numerics;
using System;

namespace OMGG.Network.Fusion {

    public class FusionSyncObject : ISyncObject {

        public string ObjectId => throw new NotImplementedException();

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public void Spawn(string prefabId, Vector3 position, Quaternion rotation)
        {
            throw new NotImplementedException();
        }

        public void Spawn(string prefabId, Vector3 position, Quaternion rotation, object initiliazeData = null)
        {
            throw new NotImplementedException();
        }

        public void SyncState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
