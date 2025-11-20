using Fusion;
using UnityEngine;
using Vermines.Core.Scene;
using Vermines.Gameplay.Phases.Enumerations;

namespace Vermines.Gameplay.Phases
{
    public interface IPhase
    {
        PhaseType Type { get; }

        void Run(PlayerRef player);
        void Deinitialize();
        void OnPhaseEnding(PlayerRef player, bool logic);
    }

    public abstract class PhaseAsset : ScriptableObject, IPhase
    {
        [SerializeField]
        private PhaseType type;

        [HideInInspector]
        public PhaseType Type => type;

        public string PhaseNameKey => type.ToString();

        protected PlayerRef _CurrentPlayer = default;

        protected SceneContext _Context;
        protected PhaseManager _PhaseManager;

        public void Initialize(SceneContext context, PhaseManager manager)
        {
            _Context = context;
            _PhaseManager = manager;
        }

        public virtual void Run(PlayerRef player)
        {
            _CurrentPlayer = player;
        }

        public virtual void Deinitialize()
        {
            _CurrentPlayer = default;
        }

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

            if (logic)
                _PhaseManager.PhaseCompleted();
            else
                GameEvents.OnAttemptNextPhase.Invoke();
        }
    }
}
