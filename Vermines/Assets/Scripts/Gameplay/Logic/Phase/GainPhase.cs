using OMGG.DesignPattern;
using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Cards.Effect;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.UI;
    using Vermines.UI.Screen;

    public struct GainSummaryData
    {
        public int BaseValue;
        public int FollowerValue;
        public int EquipmentValue;
        public int Total => BaseValue + FollowerValue + EquipmentValue;
    }

    public class GainPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Gain;

        #endregion

        private PlayerRef _CurrentPlayer;
        private GainSummaryData _gainSummary;

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            // Check if the game is currently initialized
            if (player == PlayerRef.None || PlayerController.Local == null || GameDataStorage.Instance.PlayerDeck == null || GameDataStorage.Instance.PlayerDeck.TryGetValue(player, out PlayerDeck _) == false)
                return;
            _CurrentPlayer = player;

            ResetEveryEffectsThatWasActivatedDuringTheLastRound();

            ExecuteCardEffect();

            _gainSummary.BaseValue = GameManager.Instance.SettingsData.NumberOfEloquencesEarnInGainPhase;
            _gainSummary.FollowerValue = CalculateFollowerBonus(player);

            ICommand earnCommand = new EarnCommand(_CurrentPlayer, _gainSummary.BaseValue, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameDataStorage.Instance.PlayerData.TryGet(_CurrentPlayer, out PlayerData playerData);

            GameEvents.OnPlayerUpdated.Invoke(playerData);

            if (_CurrentPlayer == PlayerController.Local.PlayerRef) {
                GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();

                if (gameplayUIController != null) {
                    gameplayUIController.GetActiveScreen(out GameplayUIScreen lastScreen);
                    gameplayUIController.ShowWithParams<GameplayUIGainSummary, GainSummaryData>(_gainSummary,lastScreen);
                }
            }
        }

        private int CalculateFollowerBonus(PlayerRef player)
        {
            int total = 0;
            var deck = GameDataStorage.Instance.PlayerDeck[player];
            foreach (ICard card in deck.PlayedCards)
            {
                foreach (IEffect effect in card.Data.Effects)
                {
                    if (effect.Type == EffectType.Play && 
                        effect.SubEffect != null &&
                        effect.SubEffect is EarnEffect eloquenceBonus &&
                        eloquenceBonus.DataToEarn == DataType.Eloquence)
                    {
                        Debug.LogFormat("AAAAa");
                        total += eloquenceBonus.Amount;
                    }
                }
            }
            return total;
        }

        public override void Reset()
        {
            base.Reset();

            _CurrentPlayer = PlayerRef.None;
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
