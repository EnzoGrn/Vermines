using Fusion;
using OMGG.DesignPattern;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Cards.Effect;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Player;
    using Vermines.UI;
    using Vermines.UI.Screen;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Phases.Data;

    [CreateAssetMenu(menuName = "Vermines/Phases/GainPhase")]
    public class GainPhaseAsset : PhaseAsset {

        #region Attributes

        public int EloquenceToEarn = 2;

        private GainSummaryData _gainSummary;

        private bool isFirstTurn = true;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef playerRef)
        {
            Debug.Log($"[GAIN.Run] local={_Context.Runner.LocalPlayer} current={_CurrentPlayer} uiNull={GameObject.FindAnyObjectByType<GameplayUIController>() == null} t={Time.frameCount}");
            GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
            
            if (_Context.GameplayMode.State != Vermines.Core.GameplayMode.GState.Active || gameplayUIController == null) {
                _Context.GameplayMode.PhaseManager.CurrentPhase = Enumerations.PhaseType.None;

                return;
            }

            base.Run(playerRef);

            PlayerController player = _Context.NetworkGame.GetPlayer(playerRef);

            if (isFirstTurn) {
                List<PlayerController> players = _Context.Runner.GetAllBehaviours<PlayerController>();

                foreach (var p in players) {
                    foreach (var effect in p.God.Effects) {
                        if ((effect.Type & EffectType.OnGameStart) != 0)
                            effect.Play(p.Object.InputAuthority);
                    }
                }

                isFirstTurn = false;
            }

            ExecuteCardEffect(player);

            _gainSummary.BaseValue     = EloquenceToEarn;
            _gainSummary.FollowerValue = CalculateFollowerBonus(player.Deck.PlayedCards);

            ICommand earnCommand = new EarnCommand(player, _gainSummary.BaseValue, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameEvents.OnPlayerUpdated.Invoke(player);

            // Who is the local player? If it's the current player, show the gain summary screen. Otherwise, skip it.
            if (_CurrentPlayer == _Context.Runner.LocalPlayer) {
                Debug.Log($"[GAIN.SHOW] current={_CurrentPlayer} phase={_Context.GameplayMode.PhaseManager.CurrentPhase} turnIdx={_Context.GameplayMode.CurrentPlayerIndex} totalTurns={_Context.GameplayMode.TotalTurnPlayed} frame={Time.frameCount}"); gameplayUIController.GetActiveScreen(out GameplayUIScreen lastScreen);
                gameplayUIController.ShowWithParams<GameplayUIGainSummary, GainSummaryData>(_gainSummary, lastScreen);
            }
        }

        #endregion

        #region Methods

        private int CalculateFollowerBonus(List<ICard> deck)
        {
            int total = 0;

            foreach (ICard card in deck) {
                foreach (IEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Play) != 0 && effect.SubEffect != null && effect.SubEffect is EarnEffect eloquenceBonus && eloquenceBonus.DataToEarn == DataType.Eloquence)
                        total += eloquenceBonus.Amount;
                }
            }

            return total;
        }

        private void ExecuteCardEffect(PlayerController player)
        {
            List<ICard> equipmentCards = player.Deck.Equipments;
            List<ICard> playedCards    = player.Deck.PlayedCards;

            foreach (ICard card in equipmentCards) {
                foreach (IEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Play) != 0)
                        effect.Play(_CurrentPlayer);
                }
            }

            foreach (ICard card in playedCards) {
                foreach (IEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Passive) != 0)
                        effect.Play(_CurrentPlayer);
                }
            }

            foreach (ICard card in playedCards) {
                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Play) != 0)
                        effect.Play(_CurrentPlayer);
                }
            }
        }

        #endregion
    }
}
