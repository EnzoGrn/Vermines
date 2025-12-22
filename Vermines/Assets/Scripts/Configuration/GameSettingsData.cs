using Fusion;
using System;

namespace Vermines.Configuration.Network {

    [Serializable]
    public struct GameSettingsData : INetworkStruct, IEquatable<GameSettingsData> {

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

        #region Table Configuration

        public int NumberOfSlotInTable;

        #endregion

        #region Sacrifice Configuration

        public int MaxSacrificesPerTurn;

        #endregion

        public readonly bool Equals(GameSettingsData other)
        {
            return (
                Seed                              == other.Seed &&
                NumberOfCardsToStartWith          == other.NumberOfCardsToStartWith &&
                NumberOfCardsToDrawAtEndOfTurn    == other.NumberOfCardsToDrawAtEndOfTurn &&
                MaxEloquence                      == other.MaxEloquence &&
                EloquenceToStartWith              == other.EloquenceToStartWith &&
                NumberOfEloquencesEarnInGainPhase == other.NumberOfEloquencesEarnInGainPhase &&
                MaxSoul                           == other.MaxSoul &&
                SoulToStartWith                   == other.SoulToStartWith &&
                BonusSoulInFamilySacrifice        == other.BonusSoulInFamilySacrifice &&
                NumberOfSlotInTable               == other.NumberOfSlotInTable &&
                MaxSacrificesPerTurn              == other.MaxSacrificesPerTurn
            );
        }

        public override readonly string ToString()
        {
            return (
                $"GameSettingsData(Seed: {Seed}, " +
                $"NumberOfCardsToStartWith: {NumberOfCardsToStartWith}, " +
                $"NumberOfCardsToDrawAtEndOfTurn: {NumberOfCardsToDrawAtEndOfTurn}, " +
                $"MaxEloquence: {MaxEloquence}, " +
                $"EloquenceToStartWith: {EloquenceToStartWith}, " +
                $"NumberOfEloquencesEarnInGainPhase: {NumberOfEloquencesEarnInGainPhase}, " +
                $"MaxSoul: {MaxSoul}, " +
                $"SoulToStartWith: {SoulToStartWith}, " +
                $"BonusSoulInFamilySacrifice: {BonusSoulInFamilySacrifice}, " +
                $"NumberOfSlotInTable: {NumberOfSlotInTable}, " +
                $"MaxSacrificesPerTurn: {MaxSacrificesPerTurn})"
            );
        }
    }
}
