using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.HUD;
    using Vermines.Player;
    using Vermines.UI;

    public class GainPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Gain;

        #endregion

        private PlayerRef _CurrentPlayer;

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            _CurrentPlayer = player;

            ResetEveryEffectsThatWasActivatedDuringTheLastRound();

            Debug.Log($"Phase {Type} is now running for player {_CurrentPlayer}");

            ExecutePlayedCardsEffect(player);

            ICommand earnCommand = new EarnCommand(player, GameManager.Instance.Config.NumberOfEloquencesToStartTheTurnWith.Value, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameDataStorage.Instance.PlayerData.TryGet(player, out PlayerData playerData);

            TurnManager.Instance.UpdatePlayer(playerData);

            OnPhaseEnding(player, true);
        }

        private void ExecutePlayedCardsEffect(PlayerRef player)
        {
            // TODO: Let the player interact with the effects of played cards.

            // Check if the effect is a passive effect or an active effect.
            // If it's a passive effect, the effect is already applied to the player.

            foreach (ICard card in GameDataStorage.Instance.PlayerDeck[player].PlayedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Play) {
                        effect.Play(player);

                        // TODO: update the player

                        Debug.Log($"[Client]: Card {card.ID} is {card.Data.Name}, effect played");
                    }
                }
            }
            return;
        }

        public void OnEffectActivated(int cardID)
        {
            PlayerController.Local.OnActiveEffectActivated(cardID);
        }

        #endregion

        #region Methods

        private void ResetEveryEffectsThatWasActivatedDuringTheLastRound()
        {
            RoundEventDispatcher.ExecutePlayerEvents(_CurrentPlayer);
        }

        #endregion
    }
}
