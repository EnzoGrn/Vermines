using Fusion;
using UnityEngine;

namespace Vermines.Gameplay.Phases
{
    using Vermines.Configuration.Network;
    using Vermines.Core;

    /// <summary>
    /// Server-authoritative turn countdown. Put it on the same GameObject as the
    /// PhaseManager (it needs its own NetworkObject registration, so it must be
    /// on a networked object).
    /// </summary>
    [RequireComponent(typeof(PhaseManager))]
    public class TurnTimer : NetworkBehaviour
    {
        [Networked]
        private TickTimer _Deadline { get; set; }

        private PhaseManager _PhaseManager;
        private int _LastShown = int.MinValue;

        public override void Spawned()
        {
            _PhaseManager = GetComponent<PhaseManager>();
        }

        /// <summary>Server only. Starts the countdown for the turn that just began.</summary>
        public void Begin()
        {
            if (!HasStateAuthority)
                return;

            int seconds = ReadSeconds();

            _Deadline = seconds > 0 ? TickTimer.CreateFromSeconds(Runner, seconds) : default;
        }

        /// <summary>Server only. Stops the countdown (turn ended, game over).</summary>
        public void Stop()
        {
            if (!HasStateAuthority)
                return;

            _Deadline = default;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_Deadline.IsRunning)
                return;

            // Never let the timer outlive the game.
            if (_PhaseManager.Context.GameplayMode.State != GameplayMode.GState.Active)
            {
                _Deadline = default;

                return;
            }

            if (!_Deadline.Expired(Runner))
                return;

            _Deadline = default;

            _PhaseManager.ForceEndTurn();
        }

        // Every client (host included) turns the networked deadline into
        // whole-second UI events. Only changes are raised.
        public override void Render()
        {
            int shown = -1;

            if (_Deadline.IsRunning)
            {
                float? remaining = _Deadline.RemainingTime(Runner);

                if (remaining.HasValue)
                    shown = Mathf.CeilToInt(Mathf.Max(0f, remaining.Value));
            }

            if (shown == _LastShown)
                return;

            _LastShown = shown;

            GameEvents.OnTurnTimerChanged.Invoke(shown);
        }

        private int ReadSeconds()
        {
            return Mathf.Max(0, _PhaseManager.Context.GameplayMode.TurnTimerSeconds);
        }
    }
}
