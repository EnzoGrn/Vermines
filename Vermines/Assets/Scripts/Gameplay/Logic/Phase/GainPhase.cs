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
    using Vermines.UI.Screen;

    public class GainPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Gain;

        #endregion

        private PlayerRef _CurrentPlayer;

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            // Check if the game is currently initialized
            if (player == PlayerRef.None || PlayerController.Local == null || GameDataStorage.Instance.PlayerDeck == null || GameDataStorage.Instance.PlayerDeck.TryGetValue(player, out PlayerDeck _) == false)
                return;
            _CurrentPlayer = player;

            ResetEveryEffectsThatWasActivatedDuringTheLastRound();

            ExecuteCardEffect();

            ICommand earnCommand = new EarnCommand(_CurrentPlayer, GameManager.Instance.SettingsData.NumberOfEloquencesEarnInGainPhase, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameDataStorage.Instance.PlayerData.TryGet(_CurrentPlayer, out PlayerData playerData);

            GameEvents.OnPlayerUpdated.Invoke(playerData);

            if (_CurrentPlayer == PlayerController.Local.PlayerRef) {
                GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();

                if (gameplayUIController != null) {
                    gameplayUIController.GetActiveScreen(out GameplayUIScreen lastScreen);
                    gameplayUIController.Show<GameplayUIGainSummary>(lastScreen);
                }
            }
        }

        private void ExecuteCardEffect()
        {
            List<ICard> equipmentCards = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].Equipments;
            List<ICard> playedCards    = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards;

            foreach (ICard card in equipmentCards) {
                foreach (IEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Play) {
                        effect.Play(_CurrentPlayer);
                    }
                }
            }

            foreach (ICard card in playedCards) {
                foreach (IEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Passive)
                        effect.Play(_CurrentPlayer);
                }
            }

            foreach (ICard card in playedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Play)
                        effect.Play(_CurrentPlayer);
                }
            }
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
