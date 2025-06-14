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
        }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            PlayerController.Local.ClearTracker();
            Reset();

            _CurrentPlayer = player;

            Debug.Log($"Phase {Type} is now running");

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrified);

            if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef)
            {
                GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
                if (gameplayUIController != null)
                {
                    gameplayUIController.GetActiveScreen(out GameplayUIScreen lastScreen);
                    gameplayUIController.Show<GameplayUITable>(lastScreen);
                }
            }
            else if (playedCards.Count == 0)
            {
                Debug.Log($"[Server]: ({Type}) Calling OnPhaseEnding by logic");
                OnPhaseEnding(_CurrentPlayer, true);
            }
        }

        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
        }

        public override void OnPhaseEnding(PlayerRef player, bool logic = false)
        {
            base.OnPhaseEnding(player, logic);
            GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrified);
        }

        #endregion

        #region Events

        public void OnCardSacrified(ICard cardSacrified)
        {
            if (Type != PhaseType.Sacrifice)
                return;
            if (_CurrentPlayer != PlayerController.Local.PlayerRef)
                return;
            if (_NumberOfCardSacrified >= GameManager.Instance.Config.MaxSacrificesPerTurn.Value)
                return;

            Debug.Log("[Client]: Card Sacrified");
            int cardId = cardSacrified.ID;
            ICard card = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Find(card => card.ID == cardId);

            if (card != null)
            {
                int cardCount = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Count;
                PlayerController.Local.OnCardSacrified(card.ID);
                cardCount--;
                _NumberOfCardSacrified++;

                if (_NumberOfCardSacrified >= GameManager.Instance.Config.MaxSacrificesPerTurn.Value
                    || cardCount == 0)
                {
                    // Pop up context
                    OnPhaseEnding(_CurrentPlayer, false);
                    GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
                    if (gameplayUIController != null)
                    {
                        // TODO: Hide the table
                    }
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
