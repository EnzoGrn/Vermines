using Vermines.Player;

namespace Vermines.Core {

    public abstract class GameplayMode : ContextBehaviour {

        public GameplayType Type = GameplayType.Standart;

        public void Activate() {}

        public void PlayerLeft(PlayerController player) {}
    }
}