using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;

    [CreateAssetMenu(menuName = "Vermines/Phases/SacrificePhase")]
    public class SacrificePhaseAsset : PhaseAsset {

        #region Properties

        public int MaxSacrifice = 1;

        private int _NumberOfCardSacrified = 0;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef playerRef)
        {
            if (_Context.GameplayMode.State != Vermines.Core.GameplayMode.GState.Active)
                return;
            base.Run(playerRef);

            PlayerController player  = _Context.NetworkGame.GetPlayer(_CurrentPlayer);
            List <ICard> playedCards = player.Deck.PlayedCards;

            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrified);

            if (playedCards.Count > 0 && _CurrentPlayer == _Context.Runner.LocalPlayer) {
                CamManager camera = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

                if (camera != null)
                    camera.GoOnSacrificeLocation();
            } else if (playedCards.Count == 0) {
                OnPhaseEnding(_CurrentPlayer, true);

                GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrified);
            }
        }

        public override void Deinitialize()
        {
            _NumberOfCardSacrified = 0;

            base.Deinitialize();
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
            if (_CurrentPlayer != _Context.Runner.LocalPlayer)
                return;
            if (_NumberOfCardSacrified >= MaxSacrifice)
                return;
            PlayerController player = _Context.NetworkGame.GetPlayer(_CurrentPlayer);

            int cardId = cardSacrified.ID;
            ICard card = player.Deck.PlayedCards.Find(c => c.ID == cardId);

            if (card != null) {
                player.OnCardSacrified(card.ID);

                _NumberOfCardSacrified++;
            } else {
                Debug.LogWarning($"[Client]: Card {cardId} not found in played cards.");

                GameEvents.OnCardSacrifiedRefused.Invoke(cardSacrified);
            }
        }

        #endregion
    }
}