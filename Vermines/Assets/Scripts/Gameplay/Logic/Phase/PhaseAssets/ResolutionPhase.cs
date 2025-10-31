using Fusion;
using OMGG.DesignPattern;
using UnityEngine;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Commands.Deck;
using Vermines.Player;
using Vermines.ShopSystem.Commands;
using System.Linq;
using Vermines.CardSystem.Enumerations;

namespace Vermines.Gameplay.Phases
{
    [CreateAssetMenu(menuName = "Vermines/Phases/ResolutionPhase")]
    public class ResolutionPhaseAsset : PhaseAsset
    {
        #region Override Methods

        public override void Run(PlayerRef player)
        {
            ICommand refillShopCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

            CommandInvoker.ExecuteCommand(refillShopCommand);

            foreach (var shopSection in GameDataStorage.Instance.Shop.Sections)
                GameEvents.OnShopRefilled.Invoke(shopSection.Key, GameDataStorage.Instance.Shop.GetDisplayCards(shopSection.Key));

            GameDataStorage.Instance.PlayerDeck[player].MergeToolDiscard(GameManager.Instance.SettingsData.Seed);

            for (int i = 0; i < GameManager.Instance.SettingsData.NumberOfCardsToDrawAtEndOfTurn; i++)
            {
                ICommand drawCardCommand = new DrawCommand(player);

                CommandResponse command = CommandInvoker.ExecuteCommand(drawCardCommand);

                if (command.Status == CommandStatus.Success && PlayerController.Local.PlayerRef == player)
                {
                    PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

                    GameEvents.InvokeOnDrawCard(deck.Hand.Last());
                }
            }

            StopEffects(player);

            if (UIContextManager.Instance != null)
                UIContextManager.Instance.ClearContext();

            OnPhaseEnding(player, true); // Here true, because everyone know that the phase is over.
        }

        #endregion

        #region Methods

        private void StopEffects(PlayerRef player)
        {
            foreach (ICard card in GameDataStorage.Instance.PlayerDeck[player].PlayedCards)
            {
                foreach (AEffect effect in card.Data.Effects)
                {
                    if ((effect.Type & EffectType.Passive) != 0)
                        effect.Stop(player);
                }
            }

            RoundEventDispatcher.ExecutePlayerEvents(player);
        }

        #endregion
    }
}