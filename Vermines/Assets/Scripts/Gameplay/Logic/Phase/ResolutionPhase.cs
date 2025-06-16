using OMGG.DesignPattern;
using UnityEngine;
using System.Linq;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.HUD.Card;
    using Vermines.Player;
    using Vermines.ShopSystem.Commands;
    using Vermines.UI.Shop;

    public class ResolutionPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Resolution;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");

            // Refill Shop
            ICommand refillShopCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

            CommandResponse response = CommandInvoker.ExecuteCommand(refillShopCommand);

            if (response.Status == CommandStatus.Success) {
                foreach (var shopSection in GameDataStorage.Instance.Shop.Sections)
                    //ShopManager.Instance.ReceiveFullShopList(shopSection.Key, shopSection.Value.AvailableCards.ToDictionary(x => x.Key, x => x.Value));
                    GameEvents.OnShopRefilled.Invoke(shopSection.Key, shopSection.Value.AvailableCards.ToDictionary(x => x.Key, x => x.Value));
            }

            // Refill Hand
            for (int i = 0; i < GameManager.Instance.SettingsData.NumberOfCardsToDrawAtEndOfTurn; i++) {
                ICommand drawCardCommand = new DrawCommand(player);

                CommandResponse command = CommandInvoker.ExecuteCommand(drawCardCommand);

                if (command.Status == CommandStatus.Success && PlayerController.Local.PlayerRef == player) {
                    PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

                    GameEvents.InvokeOnDrawCard(deck.Hand.Last());
                }

                // Dump all the deck of the user
                Debug.Log($"[SERVER]: (Resolution Phase) Player {player} refill his hand: {GameDataStorage.Instance.PlayerDeck[player].Serialize()}");
            }

            foreach (ICard card in GameDataStorage.Instance.PlayerDeck[player].PlayedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Passive)
                        effect.Stop(player);
                }
            }

            OnPhaseEnding(player, true); // Here true, because everyone know that the phase is over.
        }

        #endregion
    }
}
