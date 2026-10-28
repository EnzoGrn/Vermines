using System.Collections.Generic;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// A gameplay event published by the game: an ID ("card_played"),
    /// an optional argument (the ID of the card, of the button...) and an optional
    /// integer value (amount gained, player index...).
    /// </summary>
    public readonly struct TutorialEvent {

        public readonly string Id;
        public readonly string Arg;
        public readonly int    Value;

        public TutorialEvent(string id, string arg = null, int value = 0)
        {
            Id    = id;
            Arg   = arg;
            Value = value;
        }

        /// <summary>
        /// True if the event matches the expected ID, and the expected argument
        /// when one is given (empty = any argument).
        /// </summary>
        public bool Matches(string id, string arg)
        {
            if (!string.Equals(Id, id, StringComparison.Ordinal))
                return false;
            return string.IsNullOrEmpty(arg) || string.Equals(Arg, arg, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => $"{Id}({Arg}, {Value})";
    }

    /// <summary>
    /// Decoupled event bus: the game code publishes its actions without ever
    /// knowing about the tutorial, and the tutorial listens. Based on the idea of the
    /// TutorialSystem prototype, with an extra argument and value so that a step can wait for
    /// "the player opened the red door" and not just "the player opened a door".
    ///
    /// Game side:
    ///   TutorialEventBus.Publish("card_played", card.Id);
    ///   TutorialEventBus.Publish("gold_changed", null, player.Gold);
    /// </summary>
    public static class TutorialEventBus {

        public static event Action<TutorialEvent> OnEvent;

        public static void Publish(string eventId, string arg = null, int value = 0)
        {
            if (string.IsNullOrEmpty(eventId))
                return;
            OnEvent?.Invoke(new TutorialEvent(eventId, arg, value));
        }

        /// <summary>Removes all listeners. Useful between two tests or on domain reload.</summary>
        public static void ClearAllListeners() => OnEvent = null;
    }

    /// <summary>
    /// IDs of the events known to the tutorial itself. The game publishes its own events
    /// with its own IDs ("card_played", "enemy_killed", "door_opened"...):
    /// steps reference them by their text value (e.g. the "event" column of the table).
    /// </summary>
    public static class TutorialEvents {

        // Interface, published by the tutorial (Arg = ID of the button, target or step).
        public const string ButtonClicked = "button_clicked";
        public const string TargetClicked = "target_clicked";
        public const string PopupClosed   = "popup_closed";

        // Recommended conventions for the game (nothing forces it to publish them).

        /// <summary>Arg = player index as text ("0" = human player), Value = turn number.</summary>
        public const string TurnStarted = "turn_started";
        public const string TurnEnded   = "turn_ended";
        public const string GameStarted = "game_started";

        /// <summary>Expected by lessons that end with the game (LessonEndCondition.OnGameEnd).</summary>
        public const string GameEnded = "game_ended";

        private static readonly HashSet<string> _Passive = new(StringComparer.Ordinal) {
            ButtonClicked, TargetClicked, PopupClosed, TurnStarted, TurnEnded, GameStarted, GameEnded
        };

        /// <summary>
        /// Declares an event that happens without any player action (start of the opponent's
        /// turn, end of an animation...). A step waiting for it may forbid all actions
        /// without the editor reporting it as an error.
        /// </summary>
        public static void RegisterPassiveEvent(string eventId)
        {
            if (!string.IsNullOrEmpty(eventId))
                _Passive.Add(eventId);
        }

        public static bool IsPassive(string eventId) => !string.IsNullOrEmpty(eventId) && _Passive.Contains(eventId);
    }
}
