using System;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Un texte du tutoriel : une clé de localisation optionnelle et le texte brut
    /// (français) utilisé tant que la clé n'est pas traduite ou absente.
    [Serializable]
    public struct TutorialText
    {
        [Tooltip("Clé de localisation (optionnelle). Vide = on affiche le texte ci-dessous.")]
        public string key;

        [TextArea(2, 6)]
        public string text;

        public TutorialText(string text, string key = null)
        {
            this.text = text;
            this.key = key;
        }

        public bool IsEmpty => string.IsNullOrEmpty(key) && string.IsNullOrEmpty(text);

        public string Resolve() => TutorialLocalization.Resolve(this);

        public override string ToString() => Resolve();
    }

    /// Point de branchement vers le système de localisation du jeu. Par défaut le
    /// texte brut est affiché ; le jeu peut brancher sa propre traduction :
    ///   TutorialLocalization.Resolver = key => MyLoc.Get(key);
    public static class TutorialLocalization
    {
        /// Renvoie la traduction de la clé, ou null si elle est introuvable.
        public static Func<string, string> Resolver;

        public static string Resolve(TutorialText t)
        {
            if (!string.IsNullOrEmpty(t.key) && Resolver != null)
            {
                string translated = Resolver(t.key);
                if (!string.IsNullOrEmpty(translated))
                    return translated;
            }
            return t.text ?? string.Empty;
        }
    }
}
