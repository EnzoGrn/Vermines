using OMGG.DesignPattern;
using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Phases.Enumerations;
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

            Debug.Log($"Phase Gain is now running for player {_CurrentPlayer}");

            ExecuteCardEffect();

            ICommand earnCommand = new EarnCommand(_CurrentPlayer, GameManager.Instance.Config.NumberOfEloquencesToStartTheTurnWith.Value, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameDataStorage.Instance.PlayerData.TryGet(_CurrentPlayer, out PlayerData playerData);

            TurnManager.Instance.UpdatePlayer(playerData);

            OnPhaseEnding(_CurrentPlayer, true);
        }

        private void ExecuteCardEffect()
        {
            // TODO: Let the player interact with the effects of played cards.

            // Check if the effect is a passive effect or an active effect.

            List<ICard> playedCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            foreach (ICard card in playedCards)
            {
                foreach (IEffect effect in card.Data.Effects)
                {
                    if (effect.Type == EffectType.Passive)
                        effect.Play(_CurrentPlayer);
                }
            }

            foreach (ICard card in playedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Play) {
                        effect.Play(_CurrentPlayer);

                        // TODO: update the player
                        GameDataStorage.Instance.PlayerData.TryGet(_CurrentPlayer, out PlayerData playerData);
                        TurnManager.Instance.UpdatePlayer(playerData);

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
