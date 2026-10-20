using System.Collections.Generic;

namespace Vermines.Tutorial
{
    /// Registre de tous les TutorialTarget actifs, indexés par ID. Les données du
    /// tutoriel (ScriptableObjects) ne peuvent pas référencer d'objets de scène :
    /// elles passent par ces IDs texte.
    public static class TutorialTargetRegistry
    {
        private static readonly Dictionary<string, List<TutorialTarget>> _targets =
            new Dictionary<string, List<TutorialTarget>>();
        private static readonly HashSet<string> _hiddenIds = new HashSet<string>();

        public static void Register(string id, TutorialTarget target)
        {
            if (string.IsNullOrEmpty(id) || target == null)
                return;
            if (!_targets.TryGetValue(id, out var list))
                _targets[id] = list = new List<TutorialTarget>();
            if (!list.Contains(target))
                list.Add(target);
            if (_hiddenIds.Contains(id))
                target.SetHiddenByTutorial(true);
        }

        public static void Unregister(string id, TutorialTarget target)
        {
            if (string.IsNullOrEmpty(id) || !_targets.TryGetValue(id, out var list))
                return;
            list.Remove(target);
            if (list.Count == 0)
                _targets.Remove(id);
        }

        /// Premier objet visible portant cet ID, ou null.
        public static TutorialTarget Get(string id)
        {
            if (string.IsNullOrEmpty(id) || !_targets.TryGetValue(id, out var list))
                return null;
            TutorialTarget fallback = null;
            foreach (var t in list)
            {
                if (t == null)
                    continue;
                if (t.IsVisible)
                    return t;
                fallback = fallback ?? t;
            }
            return fallback;
        }

        public static IReadOnlyList<TutorialTarget> GetAll(string id)
        {
            if (string.IsNullOrEmpty(id) || !_targets.TryGetValue(id, out var list))
                return System.Array.Empty<TutorialTarget>();
            return list;
        }

        public static IEnumerable<string> AllIds => _targets.Keys;

        /// Cache (ou réaffiche) tous les objets portant cet ID, y compris ceux qui
        /// apparaîtront plus tard (une carte piochée après la commande HideElement).
        public static void SetHidden(string id, bool hidden)
        {
            if (string.IsNullOrEmpty(id))
                return;
            if (hidden)
                _hiddenIds.Add(id);
            else
                _hiddenIds.Remove(id);
            foreach (var t in GetAll(id))
                if (t != null)
                    t.SetHiddenByTutorial(hidden);
        }

        /// Réaffiche tout ce que le tutoriel a caché (fin ou abandon de leçon).
        public static void RevealAll()
        {
            var ids = new List<string>(_hiddenIds);
            foreach (var id in ids)
                SetHidden(id, false);
        }

        public static void Clear()
        {
            _targets.Clear();
            _hiddenIds.Clear();
        }
    }
}
