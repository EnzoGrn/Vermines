using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.ShopSystem.Commands;

    public class ResolutionPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Resolution;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");

            Debug.Log($"[SERVER]: Player hand deck at resolution: {GameDataStorage.Instance.PlayerDeck[player].Serialize()}");

            // Refill Shop
            ICommand refillShopCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

            CommandInvoker.ExecuteCommand(refillShopCommand);

            // Refill Hand
            for (int i = 0; i < GameManager.Instance.Config.NumberOfCardsToDrawAtEndOfTurn.Value; i++)
            {
                Debug.Log($"Player {player} is tying to draw a card");
                ICommand drawCardCommand = new DrawCommand(player);
                CommandInvoker.ExecuteCommand(drawCardCommand);

                // Dump all the deck of the user
                Debug.Log($"Player {player} draw a card");
                Debug.Log($"Player {player} Decks: {GameDataStorage.Instance.PlayerDeck[player].Serialize()}");
            }

            OnPhaseEnding(player, true); // Here true, because everyone know that the phase is over.
        }

        #endregion
    }
}
