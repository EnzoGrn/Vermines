using Fusion;
using System;

namespace Vermines.Configuration.Network {

    [Serializable]
    public struct GameSettingsData : INetworkStruct {

        #region Game Information

        public int Seed;

        #endregion

        #region Hand Configuration

        public int NumberOfCardsToStartWith;
        public int NumberOfCardsToDrawAtEndOfTurn;

        #endregion

        #region Game rules Configuration

        public int MaxEloquence;
        public int EloquenceToStartWith;
        public int NumberOfEloquencesEarnInGainPhase;

        public int MaxSoul;
        public int SoulToStartWith;
        public int BonusSoulInFamilySacrifice;

        #endregion

        #region Sacrifice Configuration

        public int MaxSacrificesPerTurn;

        #endregion
    }
}
