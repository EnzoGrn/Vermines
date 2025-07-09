using Fusion;

namespace Vermines.Gameplay.Phases {

    using Vermines.Gameplay.Phases.Enumerations;
    using UnityEngine;

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
        /// The logic value here represent if the function is called by the logic of the game in trivial case, every player run this function with logic = ture.Add commentMore actions
        /// - True: The function is called by the logic of the game, so every player run it.
        /// - False: The function is called by the player, who the to play is.
        /// </summary>
        /// <param name="logic">If the function is called by the logic of the game (every player) or just the current player to play</param>
        /// <param name="player">The player that end the phase.</param>
        public virtual void OnPhaseEnding(PlayerRef player, bool logic = false)
        {
            Debug.Log($"[Server]: ({Type}) OnPhaseEnding by logic {logic} processing");
            if (logic == true) {
                PhaseManager.Instance.PhaseCompleted();
            } else {
                GameEvents.OnAttemptNextPhase.Invoke();
            }
        }
    }
}
