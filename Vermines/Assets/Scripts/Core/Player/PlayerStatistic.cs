using Fusion;

namespace Vermines.Core.Player {

    using Vermines.CardSystem.Enumerations;

    public struct PlayerStatistics : INetworkStruct {

        public PlayerRef PlayerRef;

        public int Eloquence;
        public int Souls;

        public CardFamily Family;

        public bool IsValid => PlayerRef.IsRealPlayer;
    }
}
