using Fusion;
namespace Vermines.Player
{
    public struct WaitingRoomPlayerData : INetworkStruct
    {
        [Networked, Capacity(24)]
        public string Nickname
        {
            get => default;
            set { }
        }
        
        public PlayerRef PlayerRef;
        public bool IsHost;

        #region In-game Data
        // TODO: Add in-game data -> Eloquence, Souls, etc...
        #endregion
    }
}