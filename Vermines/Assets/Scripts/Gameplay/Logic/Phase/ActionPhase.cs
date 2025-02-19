using Fusion;
using UnityEngine;

namespace Vermines.Gameplay.Phases {
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    public class ActionPhase : APhase {

        #region Type

        public override PhaseType Type => PhaseType.Action;

        #endregion

        public ActionPhase()
        {
            GameEvents.OnCardBought.AddListener(OnCardBought);
        }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            Debug.Log($"Phase {Type} is now running");

            // TODO: Check if that cause a problem when client & server are simulated the turn of someone else.
        }

        private void OnCardBought(ShopType type, int id)
        {
            PlayerController.Local.OnBuy(type, id);
        }

        #endregion
    }
}
