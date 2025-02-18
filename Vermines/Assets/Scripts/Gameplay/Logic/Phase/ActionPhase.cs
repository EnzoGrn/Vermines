using Fusion;
using UnityEngine;

namespace Vermines.Gameplay.Phases {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;

    public class ActionPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Action;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");

            // TODO: Check if that cause a problem when client & server are simulated the turn of someone else.
        }

        #endregion
    }
}
