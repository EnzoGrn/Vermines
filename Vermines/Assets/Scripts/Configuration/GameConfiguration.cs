using UnityEngine;

namespace Vermines.Configuration {

    using Vermines.Configuration.Network;

    [CreateAssetMenu(fileName = "New Game Config", menuName = "Vermines/Game/Create a new game config")]
    public class GameConfiguration : ScriptableObject {

        #region Hand configuration

        public int NumberOfCardsToStartWith       = 2;
        public int NumberOfCardsToDrawAtEndOfTurn = 3;

        #endregion

        #region Game rules configuration

        public int MaxEloquence                      = 20;
        public int EloquenceToStartWith              =  0;
        public int NumberOfEloquencesEarnInGainPhase =  2;

        public int MaxSoul                    = 100;
        public int SoulToStartWith            =   0;
        public int BonusSoulInFamilySacrifice =   5;

        #endregion

        #region Sacrifice configuration

        public int MaxSacrificesPerTurn = 1;

        #endregion

        #region Methods

        public GameSettingsData ToGameSettingsData()
        {
            return new GameSettingsData() {
                Seed = Random.Range(int.MinValue, int.MaxValue),

                NumberOfCardsToStartWith       = this.NumberOfCardsToStartWith,
                NumberOfCardsToDrawAtEndOfTurn = this.NumberOfCardsToDrawAtEndOfTurn,

                MaxEloquence                      = this.MaxEloquence,
                EloquenceToStartWith              = this.EloquenceToStartWith,
                NumberOfEloquencesEarnInGainPhase = this.NumberOfEloquencesEarnInGainPhase,

                MaxSoul                    = this.MaxSoul,
                SoulToStartWith            = this.SoulToStartWith,
                BonusSoulInFamilySacrifice = this.BonusSoulInFamilySacrifice,

                MaxSacrificesPerTurn = this.MaxSacrificesPerTurn
            };
        }

        public void LoadFromGameSettingsData(GameSettingsData data)
        {
            NumberOfCardsToStartWith       = data.NumberOfCardsToStartWith;
            NumberOfCardsToDrawAtEndOfTurn = data.NumberOfCardsToDrawAtEndOfTurn;

            MaxEloquence                      = data.MaxEloquence;
            EloquenceToStartWith              = data.EloquenceToStartWith;
            NumberOfEloquencesEarnInGainPhase = data.NumberOfEloquencesEarnInGainPhase;

            MaxSoul                    = data.MaxSoul;
            SoulToStartWith            = data.SoulToStartWith;
            BonusSoulInFamilySacrifice = data.BonusSoulInFamilySacrifice;

            MaxSacrificesPerTurn = data.MaxSacrificesPerTurn;
        }

        #endregion
    }
}
