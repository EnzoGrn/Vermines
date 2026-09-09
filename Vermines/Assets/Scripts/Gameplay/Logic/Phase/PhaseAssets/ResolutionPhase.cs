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

        public int NumberOfCardsToHaveInHand = 3;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef playerRef)
        {
            if (_Context.GameplayMode.State != Vermines.Core.GameplayMode.GState.Active)
                return;
            base.Run(playerRef);

            PlayerController player = _Context.NetworkGame.GetPlayer(playerRef);

            player.Deck.MergeToolDiscard(_Context.NetworkGame.Seed);

            PlayerDeck merged = player.Deck;
            player.UpdateDeck(merged);

            for (int i = player.Hand.Count; i < NumberOfCardsToHaveInHand; i++) {
                CommandInvoker.ExecuteCommand(new DrawCommand(player));
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
                    if ((effect.Type & EffectType.Passive) != 0 || (effect.Type & EffectType.Activate) != 0 || (effect.Type & EffectType.OnOtherSacrifice) != 0 || (effect.Type & EffectType.OnOtherDiscard) != 0)
                        effect.Stop(player.Object.InputAuthority);
                }
            }

            if (player.God.Effects != null) {
                foreach (AEffect effect in player.God.Effects) {
                    if ((effect.Type & EffectType.Passive) != 0 || (effect.Type & EffectType.Activate) != 0 || (effect.Type & EffectType.OnOtherSacrifice) != 0 || (effect.Type & EffectType.OnOtherDiscard) != 0)
                        effect.Stop(player.Object.InputAuthority);
                }
            }

            RoundEventDispatcher.ExecutePlayerEvents(player.Object.InputAuthority);
        }

        #endregion
    }
}
