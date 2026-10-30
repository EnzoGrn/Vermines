using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Displaying a step: veil + holes, popup (speech bubble, window, or narrator), pointer.
    /// Follows targets every frame, since objects move.
    ///
    /// Place this on a Canvas dedicated to the tutorial, on top of the entire game UI
    /// (Screen Space - Overlay, high Sort Order), with its own GraphicRaycaster.
    ///
    /// The views are referenced through interfaces (ITutorialHighlight, ITutorialPopup, ITutorialPointer):
    /// drag any component implementing them. TutorialSpotlightOverlay, TutorialPopupView and TutorialPointerView
    /// are the default implementations, styled by your own prefabs. Every view is optional.
    /// </summary>
    public class TutorialUIController : MonoBehaviour {

        [SerializeField]
        private Canvas _Canvas;

        [SerializeField][Tooltip("Component implementing ITutorialHighlight (e.g. TutorialSpotlightOverlay).")]
        private MonoBehaviour _OverlayView;

        [SerializeField][Tooltip("Component implementing ITutorialPopup (e.g. TutorialPopupView), used by the \"Bubble\" style.")]
        private MonoBehaviour _BubbleView;

        [SerializeField][Tooltip("Component implementing ITutorialPopup, used by the \"Modal\" style. Empty = the bubble.")]
        private MonoBehaviour _ModalView;

        [SerializeField][Tooltip("Component implementing ITutorialPopup, used by the \"Narrator\" style. Empty = the bubble.")]
        private MonoBehaviour _NarratorView;

        [SerializeField][Tooltip("Component implementing ITutorialPointer (e.g. TutorialPointerView).")]
        private MonoBehaviour _PointerView;

        [SerializeField][Tooltip("Set camera, for 2D/3D targets. Empty = Camera.main.\n")]
        private Camera _WorldCamera;

        public event Action NextPressed;
        public event Action SkipPressed;

        private TutorialStepData _Step;

        // One cell per target ID: if the object is destroyed (map recreated), it's retrieved by its ID in the next frame.
        private readonly List<string>         _TargetIds = new();
        private readonly List<TutorialTarget> _Targets   = new();

        private TutorialTarget _DropTarget;

        private readonly List<Rect> _Rects = new();

        private ITutorialHighlight _Overlay;
        private ITutorialPopup     _BubblePopup;
        private ITutorialPopup     _ModalPopup;
        private ITutorialPopup     _NarratorPopup;
        private ITutorialPointer   _Pointer;
        private ITutorialPopup     _ActivePopup;

        public ITutorialHighlight Overlay       => _Overlay;
        public ITutorialPopup     BubblePopup   => _BubblePopup;
        public ITutorialPopup     ModalPopup    => _ModalPopup;
        public ITutorialPopup     NarratorPopup => _NarratorPopup;
        public ITutorialPointer   Pointer       => _Pointer;

        public Camera WorldCamera
        {
            get => _WorldCamera;
            set => _WorldCamera = value;
        }

        private Camera CanvasCamera => _Canvas == null || _Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _Canvas.worldCamera;

        /// <summary>
        /// To build the interface by code (see the demo's QuickStartUI): call this before activating the GameObject.
        /// Any view may be null.
        /// </summary>
        public void Configure(Canvas canvas, ITutorialHighlight overlay, ITutorialPopup bubble, ITutorialPopup modal, ITutorialPopup narrator, ITutorialPointer pointer)
        {
            _Canvas       = canvas;
            _OverlayView  = overlay  as MonoBehaviour;
            _BubbleView   = bubble   as MonoBehaviour;
            _ModalView    = modal    as MonoBehaviour;
            _NarratorView = narrator as MonoBehaviour;
            _PointerView  = pointer  as MonoBehaviour;

            _Overlay       = overlay;
            _BubblePopup   = bubble;
            _ModalPopup    = modal;
            _NarratorPopup = narrator;
            _Pointer       = pointer;
        }

        private void OnValidate()
        {
            _OverlayView  = Validate<ITutorialHighlight>(_OverlayView , nameof(_OverlayView));
            _BubbleView   = Validate<ITutorialPopup>    (_BubbleView  , nameof(_BubbleView));
            _ModalView    = Validate<ITutorialPopup>    (_ModalView   , nameof(_ModalView));
            _NarratorView = Validate<ITutorialPopup>    (_NarratorView, nameof(_NarratorView));
            _PointerView  = Validate<ITutorialPointer>  (_PointerView , nameof(_PointerView));
        }

        /// <summary>
        /// A field holding a component that does not implement the expected interface is emptied, with a warning.
        /// When a GameObject is dropped, Unity keeps its first MonoBehaviour: look for the right one on the same object.
        /// </summary>
        private MonoBehaviour Validate<T>(MonoBehaviour view, string field) where T : class
        {
            if (view == null || view is T)
                return view;
            if (view.TryGetComponent(out T found) && found is MonoBehaviour match)
                return match;
            Debug.LogWarning($"[Tutorial] {field}: \"{view.GetType().Name}\" does not implement {typeof(T).Name}.", this);

            return null;
        }

        private void Awake()
        {
            _Overlay       ??= _OverlayView  as ITutorialHighlight;
            _BubblePopup   ??= _BubbleView   as ITutorialPopup;
            _ModalPopup    ??= _ModalView    as ITutorialPopup;
            _NarratorPopup ??= _NarratorView as ITutorialPopup;
            _Pointer       ??= _PointerView  as ITutorialPointer;

            foreach (var popup in new[] {
                _BubblePopup, _ModalPopup, _NarratorPopup
            }) {
                if (popup == null)
                    continue;
                popup.NextClicked += () => NextPressed?.Invoke();
                popup.SkipClicked += () => SkipPressed?.Invoke();
            }

            if (_Overlay != null)
                _Overlay.OnBlockedClick += Nudge;
            TutorialInputGate.OnActionRefused += OnActionRefused;
        }

        private void OnDestroy() => TutorialInputGate.OnActionRefused -= OnActionRefused;

        private void OnActionRefused(string action, string subject) => Nudge();

        public void Nudge()
        {
            if (_ActivePopup != null)
                _ActivePopup.Nudge();
        }

        /// <summary>
        /// Displays a step.
        /// `targets` may contain null values (targets not found); these are ignored.
        /// </summary>
        public void ShowStep(TutorialStepData step, IReadOnlyList<TutorialTarget> targets, TutorialTarget dropTarget, bool showSkip)
        {
            _Step = step;

            _TargetIds.Clear();
            _Targets.Clear();

            for (int i = 0; i < step.TargetIds.Count; i++) {
                _TargetIds.Add(step.TargetIds[i]);
                _Targets.Add(targets != null && i < targets.Count ? targets[i] : null);
            }

            _DropTarget = dropTarget;

            // The drop zone for a drag-and-drop must also be highlighted, otherwise, the overlay intercepts the "drop".
            if (!string.IsNullOrEmpty(step.PointerDropTargetId) && !_TargetIds.Contains(step.PointerDropTargetId)) {
                _TargetIds.Add(step.PointerDropTargetId);
                _Targets.Add(dropTarget);
            }

            RefreshTargets();
            CollectRects();

            bool wantsOverlay = step.DimScreen || step.BlockClicksOutsideTargets;

            if (_Overlay != null) {
                if (wantsOverlay)
                    _Overlay.Show(_Rects, step.Shape, step.Padding, step.DimScreen, step.BlockClicksOutsideTargets);
                else
                    _Overlay.Hide();
            }

            HidePopups();

            _ActivePopup = PopupFor(step.PopupStyle);

            if (_ActivePopup != null) {
                bool showNext = step.AdvanceCondition == AdvanceCondition.WaitForButton;

                _ActivePopup.Show(step.Title.Resolve(), step.Body.Resolve(), step.Portrait, showNext, showSkip);
            }

            if (_Pointer != null && step.Pointer == PointerMode.None)
                _Pointer.Hide();
            LateUpdate();
        }

        public void HideStep()
        {
            _Step       = null;
            _DropTarget = null;

            _TargetIds.Clear();
            _Targets.Clear();

            HidePopups();

            if (_Pointer != null)
                _Pointer.Hide();
        }

        /// <summary>
        /// Also removes the overlay (end of lesson).
        /// Between steps, we keep it so that the hole slides from one target to another instead of flashing.
        /// </summary>
        public void HideAll()
        {
            HideStep();

            if (_Overlay != null)
                _Overlay.Hide();
        }

        private void HidePopups()
        {
            foreach (var popup in new[] {
                _BubblePopup, _ModalPopup, _NarratorPopup
            }) {
                if (popup != null && popup.IsShown)
                    popup.Hide();
            }

            _ActivePopup = null;
        }

        private ITutorialPopup PopupFor(PopupStyle style)
        {
            switch (style) {
                case PopupStyle.Bubble:
                    return _BubblePopup;
                case PopupStyle.Modal:
                    return _ModalPopup != null ? _ModalPopup : _BubblePopup;
                case PopupStyle.Narrator:
                    return _NarratorPopup != null ? _NarratorPopup : _BubblePopup;
                default:
                    return null;
            }
        }

        private void RefreshTargets()
        {
            for (int i = 0; i < _Targets.Count; i++) {
                if (_Targets[i] == null || !_Targets[i].IsVisible)
                    _Targets[i] = TutorialTargetRegistry.Get(_TargetIds[i]);
            }

            if (_Step != null && !string.IsNullOrEmpty(_Step.PointerDropTargetId) && (_DropTarget == null || !_DropTarget.IsVisible))
                _DropTarget = TutorialTargetRegistry.Get(_Step.PointerDropTargetId);
        }

        private void CollectRects()
        {
            _Rects.Clear();

            foreach (var t in _Targets) {
                if (_Rects.Count >= TutorialSpotlightOverlay.MaxHoles)
                    break;
                if (TutorialTargetLocator.TryGetScreenRect(t, _WorldCamera, out Rect r))
                    _Rects.Add(r);
            }
        }

        private void LateUpdate()
        {
            if (_Step == null)
                return;
            RefreshTargets();
            CollectRects();

            if (_Overlay != null && _Overlay.IsVisible)
                _Overlay.Track(_Rects, _Step.Shape, _Step.Padding);
            Rect? main = _Targets.Count > 0 && TutorialTargetLocator.TryGetScreenRect(_Targets[0], _WorldCamera, out Rect mainRect) ? mainRect : (Rect?)null;

            if (_ActivePopup != null)
                _ActivePopup.SetAnchor(main, _Step.Placement, CanvasCamera);
            if (_Pointer != null && _Step.Pointer != PointerMode.None && main.HasValue) {
                Vector2 to = main.Value.center;

                if (_Step.Pointer == PointerMode.Drag && _DropTarget != null && TutorialTargetLocator.TryGetScreenRect(_DropTarget, _WorldCamera, out Rect drop))
                    to = drop.center;
                _Pointer.Show(_Step.Pointer, main.Value.center, to, CanvasCamera);
            }
        }
    }
}
