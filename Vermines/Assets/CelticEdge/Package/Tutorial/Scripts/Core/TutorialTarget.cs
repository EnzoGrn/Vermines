using UnityEngine.EventSystems;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Put this on anything a step must be able to highlight, point at or hide:
    /// "End turn" button, gold counter, shop, a specific card in the hand...
    ///
    /// For objects created on the fly, the game gives the ID when the object
    /// enters a zone, for example:
    ///   target.SetTargetId($"hand:{card.Id}");   // "hand:sword"
    ///   target.SetTargetId($"shop:{item.Id}");   // "shop:potion"
    /// (or TutorialTargetScanner, without touching the code of these objects).
    /// Several objects can share the same ID (two swords in hand): the
    /// tutorial takes the first visible one.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialTarget : MonoBehaviour, IPointerClickHandler {

        [SerializeField][Tooltip("ID referenced by the steps (\"targets\" column of the table).")]
        private string _TargetId;

        [SerializeField][Tooltip("Area to highlight if it differs from this object (e.g. the frame of a card rather than its shadow).")]
        private RectTransform _HighlightRectOverride;

        [SerializeField][Tooltip("Publishes TutorialEvents.TargetClicked with this ID when the object is clicked (through the EventSystem).")]
        private bool _PublishClicks = true;

        private CanvasGroup _CanvasGroup;
        private bool        _TutorialHidden;

        public string TargetId            => _TargetId;
        public RectTransform HighlightRect => _HighlightRectOverride != null ? _HighlightRectOverride : transform as RectTransform;
        public bool IsHiddenByTutorial    => _TutorialHidden;

        public bool IsVisible => isActiveAndEnabled && !_TutorialHidden;

        private void OnEnable()  => TutorialTargetRegistry.Register(_TargetId, this);
        private void OnDisable() => TutorialTargetRegistry.Unregister(_TargetId, this);

        private void Reset()
        {
            if (string.IsNullOrEmpty(_TargetId))
                _TargetId = gameObject.name;
        }

        /// <summary>Changes the ID at runtime (instantiated cards, moved from one zone to another).</summary>
        public void SetTargetId(string newId)
        {
            if (newId == _TargetId)
                return;
            if (isActiveAndEnabled)
                TutorialTargetRegistry.Unregister(_TargetId, this);
            if (_TutorialHidden)
                SetHiddenByTutorial(false);
            _TargetId = newId;

            if (isActiveAndEnabled)
                TutorialTargetRegistry.Register(_TargetId, this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_PublishClicks)
                PublishInteraction();
        }

        /// <summary>
        /// Hook this to a Button.onClick, or call it from your own input handling
        /// (OnMouseDown, Input System...) if the EventSystem does not see the object.
        /// </summary>
        public void PublishInteraction() => TutorialEventBus.Publish(TutorialEvents.TargetClicked, _TargetId);

        /// <summary>
        /// Hides or shows the element again for the tutorial, without deactivating it (it stays
        /// registered and the game keeps driving it). Used to reveal the interface
        /// little by little, as in the first Hearthstone missions.
        /// </summary>
        public void SetHiddenByTutorial(bool hidden)
        {
            _TutorialHidden = hidden;

            if (transform is RectTransform) {
                if (_CanvasGroup == null && !TryGetComponent(out _CanvasGroup))
                    _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                _CanvasGroup.alpha          = hidden ? 0f : 1f;
                _CanvasGroup.interactable   = !hidden;
                _CanvasGroup.blocksRaycasts = !hidden;

                return;
            }

            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = !hidden;
            foreach (var c in GetComponentsInChildren<Collider>(true))
                c.enabled = !hidden;
            foreach (var c in GetComponentsInChildren<Collider2D>(true))
                c.enabled = !hidden;
        }
    }
}
