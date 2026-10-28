using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Adds TutorialTargets to the game's interface at runtime, so that no
    /// prefab has to be touched: useful for objects created on the fly (cards in hand,
    /// inventory rows, enemies...).
    ///
    /// The game registers providers, called regularly while a lesson is running:
    ///   TutorialTargetScanner.AddProvider(scanner => {
    ///       foreach (var view in hand.CardViews)
    ///           scanner.Bind(view.gameObject, $"hand:{view.Card.Id}");
    ///   });
    ///
    /// The TutorialManager creates one automatically if there is none in the scene.
    /// </summary>
    public class TutorialTargetScanner : MonoBehaviour {

        private static readonly List<Action<TutorialTargetScanner>> _Providers = new();

        [SerializeField][Tooltip("Seconds between two runs of the providers.")]
        private float _ScanInterval = 0.2f;

        [SerializeField][Tooltip("Logs every newly assigned ID to the console (to find the IDs to put in the table).")]
        private bool _LogNewIds;

        private readonly HashSet<string> _LoggedIds = new();

        private float _NextScan;

        public bool LogNewIds
        {
            get => _LogNewIds;
            set => _LogNewIds = value;
        }

        /// <summary>False outside lessons: the providers are not called.</summary>
        public bool Scanning { get; set; }

        public static void AddProvider(Action<TutorialTargetScanner> provider)
        {
            if (provider != null && !_Providers.Contains(provider))
                _Providers.Add(provider);
        }

        public static void RemoveProvider(Action<TutorialTargetScanner> provider) => _Providers.Remove(provider);

        public static void ClearProviders() => _Providers.Clear();

        private void Update()
        {
            if (!Scanning || Time.unscaledTime < _NextScan)
                return;
            _NextScan = Time.unscaledTime + _ScanInterval;

            ScanNow();
        }

        /// <summary>Calls all the providers right away (start of a step, return from a side sequence...).</summary>
        public void ScanNow()
        {
            for (int i = 0; i < _Providers.Count; i++) {
                try {
                    _Providers[i](this);
                } catch (Exception e) {
                    Debug.LogException(e, this);
                }
            }
        }

        /// <summary>Gives this ID to the object (adding a TutorialTarget to it if needed).</summary>
        public TutorialTarget Bind(GameObject go, string id)
        {
            if (go == null || string.IsNullOrEmpty(id))
                return null;
            if (go.TryGetComponent(out TutorialTarget existing) && existing.TargetId == id)
                return existing;
            if (_LogNewIds && _LoggedIds.Add(id))
                Debug.Log($"[Tutorial] Target \"{id}\" assigned to {go.name}.", go);
            return TutorialTargetRegistry.Bind(go, id);
        }

        public TutorialTarget Bind(Component component, string id) => component == null ? null : Bind(component.gameObject, id);
    }
}
