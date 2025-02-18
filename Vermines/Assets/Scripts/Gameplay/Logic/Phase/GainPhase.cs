using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Phases.Enumerations;

    public class GainPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Gain;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");

            // EarnCommand call a function of GameDataStorage that give an amount of value to the player.
            // It's important to know that if the StateAuthority is the server, the value will be set to the player, if it's a client, nothing will happend.
            ICommand earnCommand = new EarnCommand(player, GameManager.Instance.Config.NumberOfEloquencesToStartTheTurnWith.Value, DataType.Eloquence);

            CommandInvoker.ExecuteCommand(earnCommand);

            OnPhaseEnding(player, true);
        }

        #endregion
    }
}
