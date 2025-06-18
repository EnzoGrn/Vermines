using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;

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

            _CurrentPlayer = player;

            Debug.Log($"Phase {Type} is now running");

            Reset();

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrified);

            if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef) {
                CamManager camera = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

                if (camera != null)
                    camera.GoOnSacrificeLocation();
            } else if (playedCards.Count == 0) {
                OnPhaseEnding(_CurrentPlayer, true);
            }
        }

        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
        }

        public override void OnPhaseEnding(PlayerRef player, bool logic = false)
        {
            CamManager camera = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            // Return to sky location
            if (_CurrentPlayer == PlayerController.Local.PlayerRef && camera != null)
                camera.GoOnNoneLocation();
            base.OnPhaseEnding(player, logic);

            GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrified);
        }

        #endregion

        #region Events

        public void OnCardSacrified(ICard cardSacrified)
        {
            if (Type != PhaseType.Sacrifice)
                return;
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

                if (_NumberOfCardSacrified >= GameManager.Instance.SettingsData.MaxSacrificesPerTurn
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
