using Fusion;
using UnityEngine;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Elements;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    [CreateAssetMenu(menuName = "Vermines/Phases/ActionPhase")]
    public class ActionPhaseAsset : PhaseAsset {

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            if (_Context.GameplayMode.State != Vermines.Core.GameplayMode.GState.Active)
                return;
            base.Run(player);

            GameEvents.OnCardPurchaseRequested.AddListener(OnCardPurchaseRequested);
            GameEvents.OnCardPlayedRequested.AddListener(OnCardPlayed);
        }

        public override void Deinitialize()
        {
            GameEvents.OnCardPurchaseRequested.RemoveListener(OnCardPurchaseRequested);
            GameEvents.OnCardPlayedRequested.RemoveListener(OnCardPlayed);

            base.Deinitialize();
        }

        #endregion

        #region Methods

        private void OnCardPurchaseRequested(ShopType type, int id)
        {
            if (_Context.Runner.LocalPlayer == _CurrentPlayer)
                PlayerController.Local.OnBuy(type, id);
            else
                Debug.LogWarning("You can't buy a card if it's not your turn.");
        }

        public void OnDiscard(ICard card)
        {
            PlayerController player = _Context.NetworkGame.GetPlayer(_CurrentPlayer);

            if (_Context.Runner.LocalPlayer == _CurrentPlayer)
                player.OnDiscard(card.ID);
            else
                Debug.LogWarning("You can't discard a card if it's not your turn.");
        }

        public void OnDiscardNoEffect(ICard card)
        {
            PlayerController player = _Context.NetworkGame.GetPlayer(_CurrentPlayer);

            if (_Context.Runner.LocalPlayer == _CurrentPlayer)
                player.OnDiscardNoEffect(card.ID);
            else
                Debug.LogWarning("You can't discard a card if it's not your turn.");
        }

        private void OnCardPlayed(ICard card)
        {
            PlayerController player = _Context.NetworkGame.GetPlayer(_CurrentPlayer);

            if (_Context.Runner.LocalPlayer == _CurrentPlayer)
                player.OnPlay(card.ID);
            else {
                Debug.LogWarning("You can't play a card if it's not your turn.");

                GameEvents.OnCardPlayedRefused.Invoke(card);
            }
        }
        #endregion
    }
}
