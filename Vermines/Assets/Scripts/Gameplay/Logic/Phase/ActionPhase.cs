using Fusion;
using UnityEngine;

namespace Vermines.Gameplay.Phases {
    using Vermines.Gameplay.Phases.Enumerations;
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
            GameEvents.OnCardBought.AddListener(OnCardBought);
            GameEvents.OnDiscard.AddListener(OnDiscard);
            GameEvents.OnCardPlayed.AddListener(OnCardPlayed);
        }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            _CurrentPlayerRef = player;
            Debug.Log($"Phase {Type} is now running");

            // TODO: Check if that cause a problem when client & server are simulated the turn of someone else.
        }

        private void OnCardBought(ShopType type, int id)
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

        private void OnDiscard(int cardId)
        {
            // Switch the card from the hand deck to the discard deck
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
            {
                PlayerController.Local.OnDiscard(cardId);
            }
            else
            {
                Debug.LogWarning("You can't discard a card if it's not your turn.");
            }
        }

        private void OnCardPlayed(int cardId)
        {
            // Switch the card from the hand deck to the played deck
            if (PlayerController.Local.PlayerRef == _CurrentPlayerRef)
            {
                PlayerController.Local.OnPlay(cardId);
            }
            else
            {
                Debug.LogWarning("You can't play a card if it's not your turn.");
            }
        }
        #endregion
    }
}
