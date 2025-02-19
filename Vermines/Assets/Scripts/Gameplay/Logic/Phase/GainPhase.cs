using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.HUD;
    using Vermines.Player;

    public class GainPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Gain;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");


            ExecutePlayedCardsEffect(player);

            // EarnCommand call a function of GameDataStorage that give an amount of value to the player.
            // It's important to know that if the StateAuthority is the server, the value will be set to the player, if it's a client, nothing will happend.
            ICommand earnCommand = new EarnCommand(player, GameManager.Instance.Config.NumberOfEloquencesToStartTheTurnWith.Value, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            GameDataStorage.Instance.PlayerData.TryGet(player, out Vermines.Player.PlayerData playerData);

            HUDManager.instance.UpdateSpecificPlayer(playerData);

            OnPhaseEnding(player, true);
        }

        private void ExecutePlayedCardsEffect(PlayerRef player)
        {
            // TODO: Let the player interact with the effects of played cards.

            // Check if the effect is a passive effect or an active effect.
            // If it's a passive effect, the effect is already applied to the player.

            if (player != PlayerController.Local.PlayerRef)
                return;

            // If it's an active effect, the player can choose to activate it or not.

            return;
        }

        #endregion
    }
}
