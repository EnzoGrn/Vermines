using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Fusion;

namespace Vermines.Tutorial.Integration {

    using Vermines.CardSystem.Elements;

    /// <summary>
    /// What is true during a tutorial match, on the game side. Set by the bridge
    /// before the (offline) Fusion session starts, read by StandartGameplay to pick
    /// the scripted initializer, and filled by that initializer with the players.
    /// Index 0 is always the human player, 1+ are the scripted opponents.
    /// </summary>
    public static class TutorialMatch {

        #region Attributes

        public static ScriptedMatchSetup Setup { get; private set; }

        public static bool IsActive => Setup != null;

        /// <summary>
        /// True once the scripted initializer has built the board.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        private static readonly List<PlayerRef> _Players = new();

        public static IReadOnlyList<PlayerRef> Players => _Players;

        public static int PlayerCount => _Players.Count;

        public static PlayerRef HumanRef => _Players.Count > 0 ? _Players[0] : PlayerRef.None;

        #endregion

        #region Methods

        public static void Begin(ScriptedMatchSetup setup)
        {
            Setup         = setup;
            IsInitialized = false;

            _Players.Clear();
        }

        public static void End()
        {
            Setup         = null;
            IsInitialized = false;

            _Players.Clear();
        }

        internal static void MarkInitialized(List<PlayerRef> playersInSetupOrder)
        {
            _Players.Clear();
            _Players.AddRange(playersInSetupOrder);

            IsInitialized = true;
        }

        /// <summary>
        /// 0  = human,
        /// 1+ = scripted opponents,
        /// -1 = not part of the lesson.
        /// </summary>
        public static int IndexOf(PlayerRef player)
        {
            if (player == PlayerRef.None)
                return -1;
            return _Players.IndexOf(player);
        }

        public static PlayerRef RefOf(int index)
        {
            return index >= 0 && index < _Players.Count ? _Players[index] : PlayerRef.None;
        }

        #endregion
    }

    /// <summary>
    /// Card IDs used by the tutorial data (CSV, setups):
    ///     the card name, lowercase, without accents, words joined by '_'. "Pugionem Sacrificialem" becomes "pugionem_sacrificialem".
    /// 
    /// Runtime card IDs (ints) change every match, names don't.
    /// </summary>
    public static class TutorialCardIds {

        public static string Key(ICard card)
        {
            return card?.Data == null ? string.Empty : Key(card.Data.Name);
        }

        public static string Key(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;
            string     decomposed = name.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder builder = new(decomposed.Length);
            bool pendingSeparator = false;

            foreach (char c in decomposed) {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsLetterOrDigit(c)) {
                    if (pendingSeparator && builder.Length > 0)
                        builder.Append('_');
                    builder.Append(c);

                    pendingSeparator = false;
                } else {
                    pendingSeparator = true;
                }
            }

            return builder.ToString();
        }

        public static bool Is(ICard card, string id)
        {
            return card != null && !string.IsNullOrEmpty(id) && Key(card) == Key(id);
        }
    }
}
