using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.HUD;
    using Vermines.Player;
    using Vermines.UI.GameTable;

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
            _CurrentPlayer = player;

            Debug.Log($"Phase {Type} is now running");

            Reset();

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef)
            {
                // TODO: Open sacrifice menu
                Debug.Log($"[Client]: Open Sacrifice Menu for player {_CurrentPlayer}");
                TableUI.Instance.OpenTableUI();
                TableUI.Instance.EnableSacrificeMode();
            }
            else if (playedCards.Count == 0)
            {
                Debug.Log($"[Client]: No cards to sacrifice for player {_CurrentPlayer}");
                OnPhaseEnding(_CurrentPlayer, true);
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

                if (_NumberOfCardSacrified >= GameManager.Instance.Config.MaxSacrificesPerTurn.Value)
                {
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
