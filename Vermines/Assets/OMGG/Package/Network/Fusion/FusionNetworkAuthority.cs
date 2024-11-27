using System;

namespace OMGG.Network.Fusion {

    public class FusionNetworkAuthority : INetworkAuthority {

        public event Action<string, string> OnAuthorityTransfered;

        public bool HasAutority(string objectId)
        {
            throw new NotImplementedException();
        }

        public void TransferAuthority(string objectId, string newOwner)
        {
            throw new NotImplementedException();
        }
    }
}
