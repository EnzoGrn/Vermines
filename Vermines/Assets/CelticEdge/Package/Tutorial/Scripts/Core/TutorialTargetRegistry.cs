using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Registry of all active TutorialTargets, indexed by ID. The tutorial data
    /// (ScriptableObjects) cannot reference scene objects:
    /// they go through these text IDs.
    ///
    /// An ID missing from the registry can be resolved on demand, at the start of a step:
    ///   "go:Name"         the first active object in the scene with this name;
    ///   "path:Canvas/Hud" the object at this path (GameObject.Find);
    /// and any prefix added by the game with AddResolver.
    /// </summary>
    public static class TutorialTargetRegistry {

        private static readonly Dictionary<string, Func<string, GameObject>> _Resolvers = new(StringComparer.OrdinalIgnoreCase) {
            { "go"  , FindByName      },
            { "path", GameObject.Find }
        };

        private static readonly Dictionary<string, List<TutorialTarget>> _Targets   = new();
        private static readonly HashSet<string>                          _HiddenIds = new();

        public static void Register(string id, TutorialTarget target)
        {
            if (string.IsNullOrEmpty(id) || target == null)
                return;
            if (!_Targets.TryGetValue(id, out var list))
                _Targets[id] = list = new List<TutorialTarget>();
            if (!list.Contains(target))
                list.Add(target);
            if (_HiddenIds.Contains(id))
                target.SetHiddenByTutorial(true);
        }

        public static void Unregister(string id, TutorialTarget target)
        {
            if (string.IsNullOrEmpty(id) || !_Targets.TryGetValue(id, out var list))
                return;
            list.Remove(target);

            if (list.Count == 0)
                _Targets.Remove(id);
        }

        /// <summary>First visible object with this ID, or null.</summary>
        public static TutorialTarget Get(string id)
        {
            if (string.IsNullOrEmpty(id) || !_Targets.TryGetValue(id, out var list))
                return null;
            TutorialTarget fallback = null;

            foreach (var t in list) {
                if (t == null)
                    continue;
                if (t.IsVisible)
                    return t;
                fallback ??= t;
            }

            return fallback;
        }

        public static IReadOnlyList<TutorialTarget> GetAll(string id)
        {
            if (string.IsNullOrEmpty(id) || !_Targets.TryGetValue(id, out var list))
                return Array.Empty<TutorialTarget>();
            return list;
        }

        public static IEnumerable<string> AllIds => _Targets.Keys;

        /// <summary>
        /// Hides (or shows again) every object with this ID, including those that
        /// will appear later (an object created after the HideElement command).
        /// </summary>
        public static void SetHidden(string id, bool hidden)
        {
            if (string.IsNullOrEmpty(id))
                return;
            if (hidden)
                _HiddenIds.Add(id);
            else
                _HiddenIds.Remove(id);
            foreach (var t in GetAll(id)) {
                if (t != null)
                    t.SetHiddenByTutorial(hidden);
            }
        }

        /// <summary>Shows again everything the tutorial has hidden (end or abort of a lesson).</summary>
        public static void RevealAll()
        {
            var ids = new List<string>(_HiddenIds);

            foreach (var id in ids)
                SetHidden(id, false);
        }

        public static void Clear()
        {
            _Targets.Clear();
            _HiddenIds.Clear();
        }

        #region Resolution

        /// <summary>
        /// Adds (or replaces) a way to find an object from a "prefix:value" ID,
        /// for example "slot:3" → the 3rd inventory slot. resolve receives "3".
        /// </summary>
        public static void AddResolver(string prefix, Func<string, GameObject> resolve)
        {
            if (!string.IsNullOrEmpty(prefix) && resolve != null)
                _Resolvers[prefix] = resolve;
        }

        public static void RemoveResolver(string prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
                _Resolvers.Remove(prefix);
        }

        /// <summary>
        /// Like Get, but if the ID is not registered yet and starts with a known prefix,
        /// the object is looked up and receives a TutorialTarget with this ID.
        /// </summary>
        public static TutorialTarget GetOrResolve(string id)
        {
            var target = Get(id);

            if (target != null || string.IsNullOrEmpty(id))
                return target;
            int colon = id.IndexOf(':');

            if (colon <= 0 || !_Resolvers.TryGetValue(id.Substring(0, colon), out var resolve))
                return null;
            var found = resolve(id.Substring(colon + 1));

            return found == null ? null : Bind(found, id);
        }

        /// <summary>
        /// Adds (or reuses) the TutorialTarget of an object and gives it this ID. For
        /// objects created by the game without a TutorialTarget (see also TutorialTargetScanner).
        /// </summary>
        public static TutorialTarget Bind(GameObject go, string id)
        {
            if (go == null)
                return null;
            if (!go.TryGetComponent(out TutorialTarget target))
                target = go.AddComponent<TutorialTarget>();
            target.SetTargetId(id);

            return target;
        }

        private static GameObject FindByName(string objectName)
        {
            #if UNITY_2023_1_OR_NEWER
                var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            #else
                var all = UnityEngine.Object.FindObjectsOfType<Transform>();
            #endif

            foreach (var t in all) {
                if (t.name == objectName)
                    return t.gameObject;
            }

            return null;
        }

        #endregion
    }
}
