using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// An action allowed during a step: an action ID chosen by the game
    /// ("play_card", "buy", "jump", "end_turn"...) and, optionally, what it applies to
    /// (the ID of the card, of the item, of the tile...).
    ///
    /// The game picks its IDs freely; the simplest way is to gather them
    /// in a class of constants:
    ///   public static class MyActions { public const string Buy = "buy"; }
    /// </summary>
    [Serializable]
    public struct AllowedAction {

        /// <summary>Accepts any action.</summary>
        public const string AnyAction = "*";

        [Tooltip("Action ID, as the game passes it to TutorialInputGate.IsAllowed. \"*\" = any action.")]
        public string Action;

        [Tooltip("Empty = any subject. Otherwise the ID passed by the game (e.g. the ID of the purchased card).")]
        public string Subject;

        public AllowedAction(string action, string subject = null)
        {
            Action  = action;
            Subject = subject;
        }

        public bool Allows(string requested, string requestedSubject)
        {
            if (Action != AnyAction && !string.Equals(Action, requested, StringComparison.OrdinalIgnoreCase))
                return false;
            return string.IsNullOrEmpty(Subject) || string.Equals(Subject, requestedSubject, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => string.IsNullOrEmpty(Subject) ? Action : $"{Action}:{Subject}";
    }

    /// <summary>
    /// Filters the human player's game actions during the tutorial.
    ///
    /// The dark overlay blocks clicks, but a game also has shortcuts,
    /// drag and drop, gamepads and actions triggered by code: so it is the
    /// game logic itself that must ask the question, at the moment the action is
    /// validated:
    ///
    ///   if (!TutorialInputGate.IsAllowed("buy", item.Id)) return;
    ///
    /// Outside the tutorial, or during a step without restrictions, everything is allowed.
    /// Actions of scripted agents do not go through this filter.
    /// </summary>
    public static class TutorialInputGate {

        private static readonly List<AllowedAction> _Allowed = new();

        public static bool IsRestricted { get; private set; }

        /// <summary>
        /// Raised when the player attempts a forbidden action (action, subject), so that the UI
        /// can repeat the instruction and the hints (TutorialHints) can react.
        /// </summary>
        public static event Action<string, string> OnActionRefused;

        public static IReadOnlyList<AllowedAction> Allowed => _Allowed;

        /// <summary>Restricts actions to the given list. An empty list forbids everything.</summary>
        public static void Restrict(IEnumerable<AllowedAction> allowed)
        {
            _Allowed.Clear();

            if (allowed != null)
                _Allowed.AddRange(allowed);
            IsRestricted = true;
        }

        public static void Release()
        {
            _Allowed.Clear();

            IsRestricted = false;
        }

        public static bool IsAllowed(string action, string subject = null)
        {
            if (WouldAllow(action, subject))
                return true;
            OnActionRefused?.Invoke(action, subject);

            return false;
        }

        /// <summary>
        /// Like IsAllowed, but without raising OnActionRefused: to grey out a button
        /// or decide whether an element is displayed as "playable".
        /// </summary>
        public static bool WouldAllow(string action, string subject = null)
        {
            if (!IsRestricted)
                return true;
            foreach (var a in _Allowed) {
                if (a.Allows(action, subject))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Reports a refusal decided by the game itself (a game rule, not the
        /// tutorial): the instruction is repeated and the hints react the same way.
        /// </summary>
        public static void ReportRefused(string action, string subject = null) => OnActionRefused?.Invoke(action, subject);

        /// <summary>Removes all listeners. Useful between two tests or on domain reload.</summary>
        public static void ClearAllListeners() => OnActionRefused = null;
    }
}
