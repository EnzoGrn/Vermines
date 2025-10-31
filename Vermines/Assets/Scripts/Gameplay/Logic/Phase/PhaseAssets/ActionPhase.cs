using Fusion;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.Player;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.Gameplay.Phases
{
    [CreateAssetMenu(menuName = "Vermines/Phases/ActionPhase")]
    public class ActionPhaseAsset : PhaseAsset
    {
        private PlayerRef _CurrentPlayerRef;

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            GameEvents.OnCardPurchaseRequested.AddListener(OnCardPurchaseRequested);
            GameEvents.OnCardPlayedRequested.AddListener(OnCardPlayed);

            _CurrentPlayerRef = player;
            Debug.Log($"Phase {Type} is now running");
        }

        public override void Reset()
        {
            base.Reset();

            GameEvents.OnCardPurchaseRequested.RemoveListener(OnCardPurchaseRequested);
            GameEvents.OnCardPlayedRequested.RemoveListener(OnCardPlayed);

            _CurrentPlayerRef = PlayerRef.None;
        }

        #endregion

        #region Methods

        private void OnCardPurchaseRequested(ShopType type, int id)
        {
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
                PlayerController.Local.OnBuy(type, id);
            else
                Debug.LogWarning("You can't buy a card if it's not your turn.");
        }

        public void OnDiscard(ICard card)
        {
            GameManager manager = Object.FindFirstObjectByType<GameManager>();

            if (manager == null)
                return;
            if (card == null)
            {
                Debug.LogWarning("Card is null, can't discard.");
                return;
            }

            if (PlayerController.Local.PlayerRef == manager.GetCurrentPlayer())
            {
                PlayerController.Local.OnDiscard(card.ID);
            }
            else
            {
                Debug.LogWarning("You can't discard a card if it's not your turn.");
            }
        }

        public void OnDiscardNoEffect(ICard card)
        {
            GameManager manager = Object.FindFirstObjectByType<GameManager>();

            if (manager == null)
                return;
            if (card == null)
            {
                Debug.LogWarning("Card is null, can't discard.");
                return;
            }
            if (PlayerController.Local.PlayerRef == manager.GetCurrentPlayer())
            {
                PlayerController.Local.OnDiscardNoEffect(card.ID);
            }
            else
            {
                Debug.LogWarning("You can't discard a card if it's not your turn.");
            }
        }

        private void OnCardPlayed(ICard card)
        {
            GameManager manager = Object.FindFirstObjectByType<GameManager>();

            if (manager == null)
                return;
            if (card == null)
            {
                Debug.LogWarning("Card is null, can't play.");
                return;
            }
            if (PlayerController.Local.PlayerRef == manager.GetCurrentPlayer())
            {
                PlayerController.Local.OnPlay(card.ID);
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
