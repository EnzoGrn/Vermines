using OMGG.DesignPattern;
using System.Linq;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
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
            // Refill Shop
            ICommand refillShopCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

            CommandResponse response = CommandInvoker.ExecuteCommand(refillShopCommand);

            if (response.Status == CommandStatus.Success) {
                foreach (var shopSection in GameDataStorage.Instance.Shop.Sections)
                    //ShopManager.Instance.ReceiveFullShopList(shopSection.Key, shopSection.Value.AvailableCards.ToDictionary(x => x.Key, x => x.Value));
                    GameEvents.OnShopRefilled.Invoke(shopSection.Key, shopSection.Value.AvailableCards.ToDictionary(x => x.Key, x => x.Value));
            }

            // Merge the tool discards in the deck.
            GameDataStorage.Instance.PlayerDeck[player].MergeToolDiscard(GameManager.Instance.SettingsData.Seed);

            // Refill Hand
            for (int i = 0; i < GameManager.Instance.SettingsData.NumberOfCardsToDrawAtEndOfTurn; i++) {
                ICommand drawCardCommand = new DrawCommand(player);

                CommandResponse command = CommandInvoker.ExecuteCommand(drawCardCommand);

                if (command.Status == CommandStatus.Success && PlayerController.Local.PlayerRef == player) {
                    PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

                    GameEvents.InvokeOnDrawCard(deck.Hand.Last());
                }
            }

            StopEffects(player);

            // Clear the context manager
            if (UIContextManager.Instance != null)
                UIContextManager.Instance.ClearContext();
            OnPhaseEnding(player, true); // Here true, because everyone know that the phase is over.
        }

        private void StopEffects(PlayerRef player)
        {
            foreach (ICard card in GameDataStorage.Instance.PlayerDeck[player].PlayedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Passive) != 0)
                        effect.Stop(player);
                }
            }

            RoundEventDispatcher.ExecutePlayerEvents(player);
        }

        #endregion
    }
}
