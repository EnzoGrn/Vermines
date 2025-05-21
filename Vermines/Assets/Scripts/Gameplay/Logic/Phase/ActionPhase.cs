using Fusion;
using UnityEngine;

namespace Vermines.Gameplay.Phases {
    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.HUD;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    public class ActionPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Action;

        #endregion

        #region Fields

        private PlayerRef _CurrentPlayerRef;

        #endregion

        public ActionPhase()
        {
            GameEvents.OnCardPurchaseRequested.AddListener(OnCardPurchaseRequested);
            GameEvents.OnCardDiscardedRequested.AddListener(OnDiscard);
            GameEvents.OnCardDiscardedRequestedNoEffect.AddListener(OnDiscardNoEffect);
            GameEvents.OnCardPlayedRequested.AddListener(OnCardPlayed);
        }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            _CurrentPlayerRef = player;
            Debug.Log($"Phase {Type} is now running");
        }

        private void OnCardPurchaseRequested(ShopType type, int id)
        {
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
            {
                PlayerController.Local.OnBuy(type, id);
            }
            else
            {
                Debug.LogWarning("You can't buy a card if it's not your turn.");
            }
        }

        private void OnDiscard(ICard card)
        {
            if (card == null)
            {
                Debug.LogWarning("Card is null, can't discard.");
                return;
            }
            int cardId = card.ID;
            // Switch the card from the hand deck to the discard deck
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
            {
                PlayerController.Local.OnDiscard(cardId);
            }
            else
            {
                Debug.LogWarning("You can't discard a card if it's not your turn.");
                GameEvents.OnCardDiscardedRefused.Invoke(card);
            }
        }

        private void OnDiscardNoEffect(ICard card)
        {
            if (card == null)
            {
                Debug.LogWarning("Card is null, can't discard.");
                return;
            }
            int cardId = card.ID;
            // Switch the card from the hand deck to the discard deck
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
            {
                PlayerController.Local.OnDiscardNoEffect(cardId);
            }
            else
            {
                Debug.LogWarning("You can't discard a card if it's not your turn.");
                GameEvents.OnCardDiscardedRefused.Invoke(card);
            }
        }

        private void OnCardPlayed(ICard card)
        {
            if (card == null)
            {
                Debug.LogWarning("Card is null, can't play.");
                return;
            }
            int cardId = card.ID;
            // Switch the card from the hand deck to the played deck
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
            {
                PlayerController.Local.OnPlay(cardId);
            }
            else
            {
                Debug.LogWarning("You can't play a card if it's not your turn.");
                GameEvents.OnCardPlayedRefused.Invoke(card);
            }
        }
        #endregion
    }
}
