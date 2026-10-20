using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Affichage d'une étape : voile + trous, popup (bulle, fenêtre ou narrateur),
    /// pointeur. Suit les cibles à chaque frame, puisque les cartes bougent.
    ///
    /// À placer sur un Canvas dédié au tutoriel, au-dessus de toute l'UI du jeu
    /// (Screen Space - Overlay, Sort Order élevé), avec son propre GraphicRaycaster.
    public class TutorialUIController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TutorialSpotlightOverlay overlay;
        [SerializeField] private TutorialPopupView bubblePopup;
        [SerializeField] private TutorialPopupView modalPopup;
        [SerializeField] private TutorialPopupView narratorPopup;
        [SerializeField] private TutorialPointerView pointer;

        [Tooltip("Caméra du plateau, pour les cibles 2D/3D. Vide = Camera.main.")]
        [SerializeField] private Camera worldCamera;

        public event Action NextPressed;
        public event Action SkipPressed;

        private TutorialStepData _step;
        // Une case par ID de cible : si l'objet est détruit (carte recréée), on le
        // retrouve par son ID à la frame suivante.
        private readonly List<string> _targetIds = new List<string>();
        private readonly List<TutorialTarget> _targets = new List<TutorialTarget>();
        private TutorialTarget _dropTarget;
        private readonly List<Rect> _rects = new List<Rect>();
        private TutorialPopupView _activePopup;

        public TutorialPopupView BubblePopup => bubblePopup;
        public TutorialPopupView ModalPopup => modalPopup;
        public TutorialPopupView NarratorPopup => narratorPopup;

        public Camera WorldCamera
        {
            get => worldCamera;
            set => worldCamera = value;
        }

        private Camera CanvasCamera =>
            canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        /// Pour construire l'interface par code (TutorialUIBuilder) : à appeler avant
        /// d'activer le GameObject.
        public void Configure(Canvas canvas, TutorialSpotlightOverlay overlay, TutorialPopupView bubble,
            TutorialPopupView modal, TutorialPopupView narrator, TutorialPointerView pointer)
        {
            this.canvas = canvas;
            this.overlay = overlay;
            bubblePopup = bubble;
            modalPopup = modal;
            narratorPopup = narrator;
            this.pointer = pointer;
        }

        private void Awake()
        {
            foreach (var popup in new[] { bubblePopup, modalPopup, narratorPopup })
            {
                if (popup == null)
                    continue;
                popup.NextClicked += () => NextPressed?.Invoke();
                popup.SkipClicked += () => SkipPressed?.Invoke();
            }
            if (overlay != null)
                overlay.OnBlockedClick += Nudge;
            TutorialInputGate.OnActionRefused += OnActionRefused;
        }

        private void OnDestroy() => TutorialInputGate.OnActionRefused -= OnActionRefused;

        private void OnActionRefused(GameActionType action, string cardId) => Nudge();

        public void Nudge()
        {
            if (_activePopup != null)
                _activePopup.Nudge();
        }

        /// Affiche une étape. targets peut contenir des null (cibles introuvables) : elles sont ignorées.
        public void ShowStep(TutorialStepData step, IReadOnlyList<TutorialTarget> targets, TutorialTarget dropTarget, bool showSkip)
        {
            _step = step;
            _targetIds.Clear();
            _targets.Clear();
            for (int i = 0; i < step.targetIds.Count; i++)
            {
                _targetIds.Add(step.targetIds[i]);
                _targets.Add(targets != null && i < targets.Count ? targets[i] : null);
            }
            _dropTarget = dropTarget;

            // La zone de dépôt d'un glisser doit être éclairée aussi, sinon le voile
            // intercepte le "drop".
            if (!string.IsNullOrEmpty(step.pointerDropTargetId) && !_targetIds.Contains(step.pointerDropTargetId))
            {
                _targetIds.Add(step.pointerDropTargetId);
                _targets.Add(dropTarget);
            }

            RefreshTargets();

            CollectRects();
            bool wantsOverlay = step.dimScreen || step.blockClicksOutsideTargets;
            if (overlay != null)
            {
                if (wantsOverlay)
                    overlay.Show(_rects, step.shape, step.padding, step.dimScreen, step.blockClicksOutsideTargets);
                else
                    overlay.Hide();
            }

            HidePopups();
            _activePopup = PopupFor(step.popupStyle);
            if (_activePopup != null)
            {
                bool showNext = step.advanceCondition == AdvanceCondition.WaitForButton;
                _activePopup.Show(step.title.Resolve(), step.body.Resolve(), step.portrait, showNext, showSkip);
            }

            if (pointer != null && step.pointer == PointerMode.None)
                pointer.Hide();

            LateUpdate();
        }

        public void HideStep()
        {
            _step = null;
            _targetIds.Clear();
            _targets.Clear();
            _dropTarget = null;
            HidePopups();
            if (pointer != null)
                pointer.Hide();
        }

        /// Retire aussi le voile (fin de leçon). Entre deux étapes on le garde, pour
        /// que le trou glisse d'une cible à l'autre au lieu de clignoter.
        public void HideAll()
        {
            HideStep();
            if (overlay != null)
                overlay.Hide();
        }

        private void HidePopups()
        {
            foreach (var popup in new[] { bubblePopup, modalPopup, narratorPopup })
                if (popup != null && popup.IsShown)
                    popup.Hide();
            _activePopup = null;
        }

        private TutorialPopupView PopupFor(PopupStyle style)
        {
            switch (style)
            {
                case PopupStyle.Bubble: return bubblePopup;
                case PopupStyle.Modal: return modalPopup != null ? modalPopup : bubblePopup;
                case PopupStyle.Narrator: return narratorPopup != null ? narratorPopup : bubblePopup;
                default: return null;
            }
        }

        private void RefreshTargets()
        {
            for (int i = 0; i < _targets.Count; i++)
                if (_targets[i] == null || !_targets[i].IsVisible)
                    _targets[i] = TutorialTargetRegistry.Get(_targetIds[i]);
            if (_step != null && !string.IsNullOrEmpty(_step.pointerDropTargetId) && (_dropTarget == null || !_dropTarget.IsVisible))
                _dropTarget = TutorialTargetRegistry.Get(_step.pointerDropTargetId);
        }

        private void CollectRects()
        {
            _rects.Clear();
            foreach (var t in _targets)
            {
                if (_rects.Count >= TutorialSpotlightOverlay.MaxHoles)
                    break;
                if (TutorialTargetLocator.TryGetScreenRect(t, worldCamera, out Rect r))
                    _rects.Add(r);
            }
        }

        private void LateUpdate()
        {
            if (_step == null)
                return;

            RefreshTargets();
            CollectRects();
            if (overlay != null && overlay.IsVisible)
                overlay.Track(_rects, _step.shape, _step.padding);

            Rect? main = _targets.Count > 0 && TutorialTargetLocator.TryGetScreenRect(_targets[0], worldCamera, out Rect mainRect)
                ? mainRect
                : (Rect?)null;
            if (_activePopup != null)
                _activePopup.SetAnchor(main, _step.placement, CanvasCamera);

            if (pointer != null && _step.pointer != PointerMode.None && main.HasValue)
            {
                Vector2 to = main.Value.center;
                if (_step.pointer == PointerMode.Drag && _dropTarget != null
                    && TutorialTargetLocator.TryGetScreenRect(_dropTarget, worldCamera, out Rect drop))
                    to = drop.center;
                pointer.Show(_step.pointer, main.Value.center, to, CanvasCamera);
            }
        }
    }
}
