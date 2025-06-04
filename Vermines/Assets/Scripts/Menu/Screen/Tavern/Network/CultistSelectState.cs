using Fusion;
using System;

namespace Vermines.Menu.Screen.Tavern.Network {

    public struct CultistSelectState : INetworkStruct, IEquatable<CultistSelectState> {

        #region Attributes

        public PlayerRef ClientID;

        public NetworkString<_32> Name; // TODO: Set the name

        public int CultistID;

        public NetworkBool IsLockedIn;

        #endregion

        #region Constructors

        public CultistSelectState(PlayerRef player, int cultistID = -1, bool isLockedIn = false)
        {
            ClientID   = player;
            CultistID  = cultistID;
            IsLockedIn = isLockedIn;

            // TODO: Ask Vermines service his player name.
            Name = default;
        }

        #endregion

        #region Override Methods

        public readonly bool Equals(CultistSelectState other)
        {
            return (ClientID == other.ClientID && Name == other.Name && CultistID == other.CultistID && IsLockedIn == other.IsLockedIn);
        }

        #endregion
    }
}
