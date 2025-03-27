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
            GameEvents.OnCardSacrified.AddListener(OnCardSacrified);
        }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            _CurrentPlayer = player;

            Debug.Log($"Phase {Type} is now running");

            Reset();

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            foreach (ICard card in playedCards) {
                foreach (IEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Passive)
                        effect.Play(_CurrentPlayer);
                }
            }

            if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef)
                HUDManager.instance.OpenDeskOverlay();
            else
                OnPhaseEnding(_CurrentPlayer, true);
        }

        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
        }

        #endregion

        #region Events

        public void OnCardSacrified(int cardId)
        {
            Debug.Log("[Client]: Card Sacrified");
            ICard card = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Find(card => card.ID == cardId);

            Debug.Log($"[Client]: Card {card.ID} is now sacrificed");

            if (card != null)
            {
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
        }

        #endregion
    }
}
