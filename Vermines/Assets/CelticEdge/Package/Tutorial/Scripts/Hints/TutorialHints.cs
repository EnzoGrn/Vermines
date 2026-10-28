using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// A hint: When its trigger occurs, a sub-sequence of the lesson is played (over the current step), then the step resumes.
    ///
    /// Recognized triggers:
    ///   "refused:buy"        the player attempts the "buy" action even though it is prohibited;
    ///   "refused:buy:sword"  same as above, but only for the 'sword' item ("refused:*" = any refusal);
    ///   "event:gold_changed" the game publishes this event ("event:id:arg" for a specific argument);
    /// any other text sent by the game using TutorialHints.Trigger("...").
    /// </summary>
    [Serializable]
    public class TutorialHint {

        public string Trigger;

        [Tooltip("Supplementary segment of the lesson to be played.")]
        public string InterludeId;

        [Tooltip("Show it only the first time.")]
        public bool Once;

        [Min(0)][Tooltip("Periods (see TutorialHintSet.PeriodEventId) before it can be displayed again. 0 = no wait.")]
        public int CooldownPeriods = 1;

        [Min(0)][Tooltip("Seconds until it can be displayed again. 0 = no wait.")]
        public float CooldownSeconds;

        public TutorialHint() {}

        public TutorialHint(string trigger, string interludeId, bool once = false, int cooldownPeriods = 1)
        {
            Trigger         = trigger;
            InterludeId     = interludeId;
            Once            = once;
            CooldownPeriods = cooldownPeriods;
        }
    }

    [Serializable]
    public class TutorialHintSet {

        public List<TutorialHint> Hints = new();

        [Tooltip("An event that marks the transition to a new funding cycle (e.g., turn_started). Empty = the cycles never end.")]
        public string PeriodEventId = TutorialEvents.TurnStarted;

        [Tooltip("Parameter for this event (e.g., \"0\" = the human player's turn). Empty = any.")]
        public string PeriodEventArg = "0";
    }

    /// <summary>
    /// Plays the hints for the current lesson.
    /// Created and controlled by the TutorialManager (TutorialManager.Instance.Hints). The game simply needs to send its own triggers:
    ///   TutorialManager.Instance.Hints.Trigger("not_enough_gold");
    ///
    /// Hints that play the same side sequence share their cooldown.
    /// </summary>
    public sealed class TutorialHints {

        private readonly TutorialManager _Manager;

        private readonly Dictionary<string, int> _ShownPeriod = new();
        private readonly Dictionary<string, float> _ShownTime = new();

        private TutorialHintSet _Set;

        private int _Period;

        /// <summary>
        /// Additional condition set by the game (e.g., only during the human player's turn, so that an AI error does not trigger a hint).
        /// </summary>
        public Func<bool> CanShow { get; set; }

        /// <summary>Number of periods that have elapsed since the start of the lesson.</summary>
        public int Period => _Period;

        /// <summary>Resolves when a support card is played (trigger, support).</summary>
        public event Action<string, TutorialHint> HintShown;

        internal TutorialHints(TutorialManager manager)
        {
            _Manager = manager;
        }

        internal void Begin(TutorialHintSet set)
        {
            End();

            _Set = set;

            if (_Set == null || _Set.Hints.Count == 0)
                return;
            TutorialEventBus.OnEvent          += OnEvent;
            TutorialInputGate.OnActionRefused += OnActionRefused;
        }

        internal void End()
        {
            TutorialEventBus.OnEvent          -= OnEvent;
            TutorialInputGate.OnActionRefused -= OnActionRefused;

            _Set    = null;
            _Period = 0;

            _ShownPeriod.Clear();
            _ShownTime.Clear();
        }

        /// <summary>Sends a trigger. Returns true if a help card has been played.</summary>
        public bool Trigger(string trigger)
        {
            if (_Set == null || string.IsNullOrEmpty(trigger))
                return false;
            foreach (var hint in _Set.Hints) {
                if (hint != null && string.Equals(hint.Trigger, trigger, StringComparison.OrdinalIgnoreCase) && TryShow(trigger, hint))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Ignore the fact that a tooltip has already been displayed (so it can be displayed again immediately).
        /// </summary>
        public void ResetHint(string interludeId)
        {
            _ShownPeriod.Remove(interludeId);
            _ShownTime.Remove(interludeId);
        }

        public bool WasShown(string interludeId) => _ShownPeriod.ContainsKey(interludeId);

        private bool TryShow(string trigger, TutorialHint hint)
        {
            string key = hint.InterludeId;

            if (string.IsNullOrEmpty(key) || (CanShow != null && !CanShow()))
                return false;
            if (_ShownPeriod.TryGetValue(key, out int period)) {
                if (hint.Once || _Period - period < hint.CooldownPeriods)
                    return false;
                if (hint.CooldownSeconds > 0f && _ShownTime.TryGetValue(key, out float time) && Time.unscaledTime - time < hint.CooldownSeconds)
                    return false;
            }

            if (!_Manager.PlayInterlude(key))
                return false;
            _ShownPeriod[key] = _Period;
            _ShownTime[key]   = Time.unscaledTime;

            HintShown?.Invoke(trigger, hint);

            return true;
        }

        private void OnActionRefused(string action, string subject)
        {
            if (!string.IsNullOrEmpty(subject) && Trigger($"refused:{action}:{subject}"))
                return;
            if (!Trigger($"refused:{action}"))
                Trigger("refused:*");
        }

        private void OnEvent(TutorialEvent e)
        {
            if (_Set == null)
                return;
            if (!string.IsNullOrEmpty(_Set.PeriodEventId) && e.Matches(_Set.PeriodEventId, _Set.PeriodEventArg))
                _Period++;
            if (!string.IsNullOrEmpty(e.Arg) && Trigger($"event:{e.Id}:{e.Arg}"))
                return;
            Trigger($"event:{e.Id}");
        }
    }
}
