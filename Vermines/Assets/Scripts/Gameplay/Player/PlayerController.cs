using Fusion;

namespace Vermines.Player {

    using Vermines.Network.Utilities;

    public class PlayerController : NetworkBehaviour {

        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);
        }
    }
}
