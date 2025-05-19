using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.UI.GameTable;
    using Vermines.UI;
    using Vermines.UI.Screen;

    public class SacrificePhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Sacrifice;

        #endregion

        #region Properties

        /// <summary>
        /// The current number of cards the player sacrificed during this phase.
        /// </summary>
        private int _NumberOfCardSacrified = 0;

        private PlayerRef _CurrentPlayer;

        #endregion

        public SacrificePhase()
        {
            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrified);
        }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            PlayerController.Local.ClearTracker();

            _CurrentPlayer = player;

            Debug.Log($"Phase {Type} is now running");

            Reset();

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef)
            {
                GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
                if (gameplayUIController != null)
                {
                    gameplayUIController.Show<GameplayUITable>();
                    //UIContextManager.Instance.PushContext<SacrificeContext>();
                }
            }
            else if (playedCards.Count == 0)
            {
                OnPhaseEnding(_CurrentPlayer, false);
            }
        }

        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
        }

        #endregion

        #region Events

        public void OnCardSacrified(ICard cardSacrified)
        {
            Debug.Log("[Client]: Card Sacrified");
            int cardId = cardSacrified.ID;
            ICard card = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Find(card => card.ID == cardId);

            if (card != null)
            {
                Debug.Log($"[Client]: Card {card.ID} is now sacrificed");
                if (_CurrentPlayer == PlayerController.Local.PlayerRef)
                {
                    PlayerController.Local.OnCardSacrified(card.ID);
                }

                _NumberOfCardSacrified++;

                if (_NumberOfCardSacrified >= GameManager.Instance.Config.MaxSacrificesPerTurn.Value
                    || GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Count == 0)
                {
                    // Pop up context
                    OnPhaseEnding(_CurrentPlayer, true);
                }
            }
            else
            {
                Debug.LogWarning($"[Client]: Card {cardId} not found in played cards.");
                GameEvents.OnCardSacrifiedRefused.Invoke(cardSacrified);
            }
        }
        #endregion
    }
}
