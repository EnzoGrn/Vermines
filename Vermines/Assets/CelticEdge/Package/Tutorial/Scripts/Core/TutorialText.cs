using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// A tutorial text: an optional localization key and the raw text
    /// (in the source language, e.g. French) used as long as the key is missing or not translated.
    /// </summary>
    [Serializable]
    public struct TutorialText {

        [Tooltip("Localization key (optional). Empty = the text below is displayed.")]
        public string Key;

        [TextArea(2, 6)]
        public string Text;

        public TutorialText(string text, string key = null)
        {
            Text = text;
            Key  = key;
        }

        public bool IsEmpty => string.IsNullOrEmpty(Key) && string.IsNullOrEmpty(Text);

        public string Resolve() => TutorialLocalization.Resolve(this);

        public override string ToString() => Resolve();
    }

    /// <summary>
    /// Hook into the game's localization system. By default the
    /// raw text is displayed; the game can plug in its own translation:
    ///   TutorialLocalization.Resolver = key => MyLoc.Get(key);
    /// </summary>
    public static class TutorialLocalization {

        /// <summary>Returns the translation of the key, or null if it cannot be found.</summary>
        public static Func<string, string> Resolver;

        public static string Resolve(TutorialText t)
        {
            if (!string.IsNullOrEmpty(t.Key) && Resolver != null) {
                string translated = Resolver(t.Key);

                if (!string.IsNullOrEmpty(translated))
                    return translated;
            }

            return t.Text ?? string.Empty;
        }
    }
}
