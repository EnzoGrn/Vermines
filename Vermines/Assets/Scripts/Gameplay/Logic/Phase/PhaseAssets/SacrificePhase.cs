using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;

namespace Vermines.Gameplay.Phases
{
    [CreateAssetMenu(menuName = "Vermines/Phases/SacrificePhase")]
    public class SacrificePhaseAsset : PhaseAsset
    {
        #region Properties

        private int _NumberOfCardSacrified = 0;
        private PlayerRef _CurrentPlayer;

        private Coroutine _sacrificeCoroutine;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            // Check if the game is currently initialized
            if (player == PlayerRef.None || PlayerController.Local == null || GameDataStorage.Instance.PlayerDeck == null || GameDataStorage.Instance.PlayerDeck.TryGetValue(player, out PlayerDeck _) == false)
                return;
            Reset();

            _CurrentPlayer = player;

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
                OnPhaseEnding(_CurrentPlayer, true);

                GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrified);
            }
        }

        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
            _CurrentPlayer = PlayerRef.None;
        }

        public override void OnPhaseEnding(PlayerRef player, bool logic = false)
        {
            base.OnPhaseEnding(player, logic);

            if (_sacrificeCoroutine != null && PlayerController.Local != null)
            {
                PlayerController.Local.StopCoroutine(_sacrificeCoroutine);
                _sacrificeCoroutine = null;
            }

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
            if (_NumberOfCardSacrified >= GameManager.Instance.SettingsData.MaxSacrificesPerTurn)
                return;

            Debug.Log("[Client]: Card Sacrified");

            int cardId = cardSacrified.ID;
            ICard card = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Find(c => c.ID == cardId);

            if (card != null)
            {
                PlayerController.Local.OnCardSacrified(card.ID);
                _NumberOfCardSacrified++;
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