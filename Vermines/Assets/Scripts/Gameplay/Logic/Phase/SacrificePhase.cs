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

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Reset();

            Debug.LogWarning($"Phase {Type} is now running");

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[player].PlayedCards;

            if (playedCards.Count > 0)
                Sacrifice(player);
            else
                OnPhaseEnding(player, true);
        }

        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
        }

        #endregion

        #region Methods

        private void Sacrifice(PlayerRef player)
        {
            // TODO: Check if that cause a problem when client & server are simulated the turn of someone else.
            // Here: it's a unique player phase, only the current player can simulate this phase.
            // He will send every command needed to the server to simulate the phase.
            if (player != PlayerController.Local.PlayerRef)
                return;

            // TODO: Implement the logic to pick a card to sacrifice. And to allow the player to choose if they want to sacrifice another card or not.
            Debug.Log($"[TODO | DEBUG]: You can pick a card to sacrifice it. (Cards left {GameManager.Instance.Config.MaxSacrificesPerTurn.Value - _NumberOfCardSacrified}).");
        }

        #endregion

        #region Events

        public void OnCardSacrified(PlayerRef player)
        {
            _NumberOfCardSacrified++;

            if (_NumberOfCardSacrified < GameManager.Instance.Config.MaxSacrificesPerTurn.Value) {
                Sacrifice(player);
            } else {
                OnPhaseEnding(player, true);
            }
        }

        #endregion
    }
}
