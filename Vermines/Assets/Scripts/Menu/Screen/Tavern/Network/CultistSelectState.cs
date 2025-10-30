using Fusion;
using System;

namespace Vermines.Menu.CustomLobby {

    public struct CultistSelectState : INetworkStruct, IEquatable<CultistSelectState> {

        #region Attributes

        public PlayerRef ClientID;

        public int CultistID;

        public NetworkBool IsLockedIn;

        #endregion

        #region Constructors

        public CultistSelectState(PlayerRef player, int cultistID = -1, bool isLockedIn = false)
        {
            ClientID   = player;
            CultistID  = cultistID;
            IsLockedIn = isLockedIn;
        }

        #endregion

        #region Override Methods

        public readonly bool Equals(CultistSelectState other)
        {
            return (ClientID == other.ClientID && CultistID == other.CultistID && IsLockedIn == other.IsLockedIn);
        }

        #endregion
    }
}
