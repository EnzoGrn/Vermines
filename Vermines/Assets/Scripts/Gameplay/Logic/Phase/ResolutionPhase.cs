using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases {
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.ShopSystem.Commands;

    public class ResolutionPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Resolution;

        #endregion

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");

            //ICommand fillShopCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

            //CommandInvoker.ExecuteCommand(fillShopCommand);

            // TODO: Check if that cause a problem when client & server are simulated the turn of someone else.

            OnPhaseEnding(player, true); // Here true, because everyone know that the phase is over.
        }

        #endregion
    }
}
