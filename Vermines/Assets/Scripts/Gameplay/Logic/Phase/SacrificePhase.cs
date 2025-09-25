using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.UI;

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

        public SacrificePhase() {}

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            // Check if the game is currently initialized
            if (player == PlayerRef.None || PlayerController.Local == null || GameDataStorage.Instance.PlayerDeck == null || GameDataStorage.Instance.PlayerDeck.TryGetValue(player, out PlayerDeck _) == false)
                return;
            Reset();

            _CurrentPlayer = player;

            Debug.Log($"Phase {Type} is now running");

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrified);

            if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef)
            {
                CamManager camera = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

                if (camera != null)
                    camera.GoOnSacrificeLocation();
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
            _CurrentPlayer         = PlayerRef.None;
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
            if (_CurrentPlayer != PlayerController.Local.PlayerRef)
                return;
            if (_NumberOfCardSacrified >= GameManager.Instance.Configuration.MaxSacrificesPerTurn)
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

                if (_NumberOfCardSacrified >= GameManager.Instance.SettingsData.MaxSacrificesPerTurn || cardCount == 0)
                {
                    // Pop up context
                    OnPhaseEnding(_CurrentPlayer, false);
                    GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
                    if (gameplayUIController != null)
                    {
                        // TODO: Hide the tableAdd commentMore actions
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
