using Fusion;
using OMGG.DesignPattern;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.Player;
    using Vermines.ShopSystem.Commands;

    [CreateAssetMenu(menuName = "Vermines/Phases/ResolutionPhase")]
    public class ResolutionPhaseAsset : PhaseAsset {

        #region Attributes

        public int NumberOfCardsToDrawAtEndOfTurn = 3;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef playerRef)
        {
            if (_Context.GameplayMode.State != Vermines.Core.GameplayMode.GState.Active)
                return;
            base.Run(playerRef);

            PlayerController player = _Context.NetworkGame.GetPlayer(playerRef);

            ICommand refillShopCommand = new FillShopCommand(_Context.GameplayMode.Shop);

            CommandInvoker.ExecuteCommand(refillShopCommand);

            foreach (var shopSection in _Context.GameplayMode.Shop.Sections)
                GameEvents.OnShopRefilled.Invoke(shopSection.Key, _Context.GameplayMode.Shop.GetDisplayCards(shopSection.Key));
            player.Deck.MergeToolDiscard(_Context.NetworkGame.Seed);

            for (int i = 0; i < NumberOfCardsToDrawAtEndOfTurn; i++) {
                ICommand drawCardCommand = new DrawCommand(player);

                CommandResponse command = CommandInvoker.ExecuteCommand(drawCardCommand);

                if (command.Status == CommandStatus.Success && _Context.Runner.LocalPlayer == playerRef)
                    GameEvents.InvokeOnDrawCard(player.Deck.Hand.Last());
            }

            StopEffects(player);

            if (UIContextManager.Instance != null)
                UIContextManager.Instance.ClearContext();
            OnPhaseEnding(playerRef, true); // Here true, because everyone know that the phase is over.
        }

        #endregion

        #region Methods

        private void StopEffects(PlayerController player)
        {
            foreach (ICard card in player.Deck.PlayedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Passive) != 0)
                        effect.Stop(player.Object.InputAuthority);
                }
            }

            RoundEventDispatcher.ExecutePlayerEvents(player.Object.InputAuthority);
        }

        #endregion
    }
}