using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;

    public interface IPhase {

        PhaseType Type { get; }

        void Run(PlayerRef player);

        void Reset();

        void OnPhaseEnding(PlayerRef player, bool logic);
    }

    public abstract class APhase : IPhase {

        public abstract PhaseType Type { get; }

        public abstract void Run(PlayerRef player);

        public virtual void Reset() {}

        /// <summary>
        /// Event that end the phase.
        /// The logic value here represent if the function is called by the logic of the game or by the player.
        /// - True: The function is called by the logic of the game.
        /// - False: The function is called by the player.
        /// </summary>
        /// <param name="logic">If the function is called by the logic of the game or by the player.</param>
        /// <param name="player">The player that end the phase.</param>
        public virtual void OnPhaseEnding(PlayerRef player, bool logic = false)
        {
            if (logic == true) {
                PhaseManager.Instance.RPC_PhaseCompleted();
            } else {
                if (player != PlayerController.Local.PlayerRef)
                    return;
                GameEvents.OnAttemptNextPhase.Invoke();
            }
        }
    }
}
