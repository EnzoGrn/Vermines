using UnityEngine;

namespace Vermines.Core {

    using Vermines.Player;

    public abstract class GameplayMode : ContextBehaviour {

        public GameplayType Type = GameplayType.Standart;

        public void Activate() {}

        public void PlayerLeft(PlayerController player) {}

        #region Methods

        public override void Spawned()
        {
            Debug.LogError($"Spawned() GameplayMode");

            Context.GameplayMode = this;
        }

        #endregion
    }
}