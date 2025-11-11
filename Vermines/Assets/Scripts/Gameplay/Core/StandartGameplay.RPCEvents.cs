using Fusion;
using Vermines.Core;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.Gameplay.Core {

    public partial class StandartGameplay : GameplayMode {

        #region Shop

        public override void OnBuyCard(int playerID, ShopType shopType, int cardID)
        {
        }

        public override void OnReplaceCardInShop(int playerID, ShopType shopType, int cardID)
        {
        }

        #endregion

        #region Table (Play, Discard)

        public override void OnCardPlayed(int playerID, int cardID)
        {
        }

        public override void OnDiscardCard(int playerID, int cardID, bool hasEffect = true)
        {
        }

        #endregion

        #region Sacrifice

        public override void OnCardSacrified(int playerID, int cardID)
        {
        }

        #endregion

        #region Recycle
        
        public override void OnCardRecycled(int playerID, int cardID)
        {
        }

        #endregion

        #region Effects

        public override void OnActivateEffect(int playerID, int cardID)
        {
        }

        public override void OnReducedInSilenced(int playerID, int cardToBeSilenced)
        {
        }

        public override void OnRemoveReducedInSilenced(int playerID, int cardID, int originalSouls)
        {
        }

        public override void OnNetworkEventCardEffect(int playerID, int cardID, string data)
        {
        }

        public override void OnEffectChosen(int playerID, int cardID, int effectIndex)
        {
        }

        #endregion
    }
}
