using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Phases {
    using System.Linq;
    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Errors;
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
            List <ICard> playedCards = player.PlayedCards.ToList();

            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrificed);

            if (playedCards.Count > 0 && _CurrentPlayer == _Context.Runner.LocalPlayer) {
                CamManager camera = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

                if (camera != null)
                    camera.GoOnSacrificeLocation();
            } else if (playedCards.Count == 0) {
                OnPhaseEnding(_CurrentPlayer, true);

                GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrificed);
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

            GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrificed);
        }

        #endregion

        #region Events

        public void OnCardSacrificed(ICard cardSacrified)
        {
            if (Type != PhaseType.Sacrifice)
                return;
            if (_CurrentPlayer != _Context.Runner.LocalPlayer)
                return;
            if (_NumberOfCardSacrified >= MaxSacrifice)
                return;
            PlayerController player = _Context.NetworkGame.GetPlayer(_CurrentPlayer);

            int cardId = cardSacrified.ID;
            ICard card = player.PlayedCards.ToList().Find(c => c.ID == cardId);

            if (card != null)
            {
                player.OnCardSacrificed(card.ID);

                _NumberOfCardSacrified++;
            }
            else
            {
                Debug.LogWarning($"[Client]: Card {cardId} not found in played cards.");

                GameActionError localError = new GameActionError
                {
                    Scope = ErrorScope.Local,
                    Target = _Context.Runner.LocalPlayer,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Sacrifice,
                    MessageKey = "Sacrifice_CardNotInTable",
                    MessageArgs = new GameActionErrorArgs(cardSacrified.Data.Name)
                };

                GameEvents.OnActionRefused.Invoke(localError, GameActionError.Localize(localError));
            }
        }

        #endregion
    }
}
