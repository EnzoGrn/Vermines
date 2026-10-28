using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// An explanatory popup: a speech bubble attached to a target, a centered window, or a narrator in a corner, depending on the prefab.
    /// The same script is used for all three. Only "anchorToTarget" changes.
    ///
    /// Recommended prefab hierarchy:
    ///   Popup (RectTransform, CanvasGroup, Background Image, VerticalLayoutGroup + ContentSizeFitter)
    ///     Portrait (Image)          optional
    ///     Title (TMP_Text)          optional
    ///     Body (TMP_Text)
    ///     Buttons
    ///       Next (Button)           "Next"
    ///       Skip (Button)           "Skip Lesson", optional
    ///     Arrow (RectTransform)     optional, points upward in the prefab
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialPopupView : MonoBehaviour, ITutorialPopup {

        [SerializeField]
        private RectTransform _Root;

        [SerializeField]
        private TMP_Text _TitleText;

        [SerializeField]
        private TMP_Text _BodyText;

        [SerializeField]
        private Image _PortraitImage;

        [SerializeField]
        private Button _NextButton;

        [SerializeField]
        private Button _SkipButton;

        [SerializeField]
        private RectTransform _Arrow;

        [Header("Location")]

        [SerializeField][Tooltip("True for a bubble that follows its target. False for a window with a fixed position (modal, narrator).")]
        private bool _AnchorToTarget = true;
        
        [SerializeField][Tooltip("Distance between the target and the bubble.")]
        private float _Gap = 24f;
        
        [SerializeField][Tooltip("Minimum margin from the edges of the screen.")]
        private float _ScreenMargin = 16f;

        [Header("Animation")]

        [SerializeField]
        private float _FadeDuration = 0.2f;

        [SerializeField]
        private float _NudgeAmplitude = 10f;

        private CanvasGroup _Group;
        private bool        _Shown;
        private float       _NudgeTime = -1f;
        private Vector2     _RestPosition;
        private Vector2     _NudgeOffset;

        public event Action NextClicked;
        public event Action SkipClicked;

        public bool IsShown => _Shown;

        /// <summary>
        /// To create the popup by code (see the demo's QuickStartUI): call this before activating the GameObject.
        /// </summary>
        public void Configure(RectTransform root, TMP_Text title, TMP_Text body, Image portrait, Button next, Button skip, RectTransform arrow, bool anchorToTarget)
        {
            _Root           = root;
            _TitleText      = title;
            _BodyText       = body;
            _PortraitImage  = portrait;
            _NextButton     = next;
            _SkipButton     = skip;
            _Arrow          = arrow;
            _AnchorToTarget = anchorToTarget;
        }

        private void Awake()
        {
            _Group = GetComponent<CanvasGroup>();

            if (_Root == null)
                _Root = (RectTransform)transform;
            if (_NextButton != null)
                _NextButton.onClick.AddListener(() => NextClicked?.Invoke());
            if (_SkipButton != null)
                _SkipButton.onClick.AddListener(() => SkipClicked?.Invoke());
            _Group.alpha          = 0f;
            _Group.blocksRaycasts = false;
            _Group.interactable   = false;
            _RestPosition         = _Root.anchoredPosition;
        }

        public void Show(string title, string body, Sprite portrait, bool showNext, bool showSkip)
        {
            gameObject.SetActive(true);

            _Shown = true;

            if (_TitleText != null) {
                _TitleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
                _TitleText.text = title ?? string.Empty;
            }

            if (_BodyText != null)
                _BodyText.text = body ?? string.Empty;
            if (_PortraitImage != null) {
                _PortraitImage.gameObject.SetActive(portrait != null);
                _PortraitImage.sprite = portrait;
            }

            if (_NextButton != null)
                _NextButton.gameObject.SetActive(showNext);
            if (_SkipButton != null)
                _SkipButton.gameObject.SetActive(showSkip);
            if (_Arrow != null)
                _Arrow.gameObject.SetActive(false);
            _Group.blocksRaycasts = true;
            _Group.interactable   = true;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_Root);

            if (!_AnchorToTarget)
                _Root.anchoredPosition = _RestPosition;
        }

        public void Hide()
        {
            _Shown                = false;
            _Group.blocksRaycasts = false;
            _Group.interactable   = false;
        }

        /// <summary>
        /// Moves a fixed-position popup (window, narrator): anchor, pivot, and position.
        /// </summary>
        public void SetRestPosition(Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition)
        {
            var rt = _Root != null ? _Root : (RectTransform)transform;

            rt.anchorMin        = rt.anchorMax = anchor;
            rt.pivot            = pivot;
            rt.anchoredPosition = _RestPosition = anchoredPosition;
        }

        /// <summary>
        /// A slight jolt when the player attempts a prohibited action.
        /// </summary>
        public void Nudge() => _NudgeTime = 0f;

        /// <summary>
        /// Places the bubble next to the target (screen rectangle).
        /// Called every frame by the TutorialUIController to track moving targets.
        /// </summary>
        public void SetAnchor(Rect? targetScreenRect, PopupPlacement placement, Camera canvasCamera)
        {
            if (!_AnchorToTarget || !_Shown)
                return;
            var parent = _Root.parent as RectTransform;

            if (parent == null)
                return;
            if (targetScreenRect == null) {
                _Root.anchoredPosition = _RestPosition;

                if (_Arrow != null)
                    _Arrow.gameObject.SetActive(false);
                return;
            }

            Rect sr = targetScreenRect.Value;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sr.min, canvasCamera, out Vector2 tMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sr.max, canvasCamera, out Vector2 tMax);

            Rect         target = Rect.MinMaxRect(tMin.x, tMin.y, tMax.x, tMax.y);
            Rect         bounds = parent.rect;
            Vector2        size = _Root.rect.size;
            PopupPlacement side = placement == PopupPlacement.Auto ? ChooseSide(target, bounds, size) : placement;

            Vector2 center;

            switch (side) {
                case PopupPlacement.Above:
                    center = new Vector2(target.center.x, target.yMax + _Gap + size.y * 0.5f);
                    
                    break;
                case PopupPlacement.Left:
                    center = new Vector2(target.xMin - _Gap - size.x * 0.5f, target.center.y);
                    
                    break;
                case PopupPlacement.Right:
                    center = new Vector2(target.xMax + _Gap + size.x * 0.5f, target.center.y);
                    
                    break;
                default:
                    center = new Vector2(target.center.x, target.yMin - _Gap - size.y * 0.5f);
                    
                    break;
            }

            // Keeps the bubble on the screen.
            center.x = Mathf.Clamp(center.x, bounds.xMin + _ScreenMargin + size.x * 0.5f, bounds.xMax - _ScreenMargin - size.x * 0.5f);
            center.y = Mathf.Clamp(center.y, bounds.yMin + _ScreenMargin + size.y * 0.5f, bounds.yMax - _ScreenMargin - size.y * 0.5f);

            // Position in parent coordinates -> anchoredPosition, regardless of the prefab's pivot/anchor.
            Vector2 pivotOffset = (_Root.pivot - new Vector2(0.5f, 0.5f)) * size;
            Vector3 world       = parent.TransformPoint(center + pivotOffset);

            _Root.position          = world;
            _RestPosition           = _Root.anchoredPosition;
            _Root.anchoredPosition += _NudgeOffset;

            PlaceArrow(side, target, center, size);
        }

        private static PopupPlacement ChooseSide(Rect target, Rect bounds, Vector2 size)
        {
            float below = target.yMin - bounds.yMin;
            float above = bounds.yMax - target.yMax;
            float left  = target.xMin - bounds.xMin;
            float right = bounds.xMax - target.xMax;

            // Above/below first (more natural reading order), otherwise the wider side.
            if (above >= size.y + 40f && above >= below)
                return PopupPlacement.Above;
            else if (below >= size.y + 40f)
                return PopupPlacement.Below;
            else if (above >= size.y + 40f)
                return PopupPlacement.Above;
            return right >= left ? PopupPlacement.Right : PopupPlacement.Left;
        }

        private void PlaceArrow(PopupPlacement side, Rect target, Vector2 popupCenter, Vector2 size)
        {
            if (_Arrow == null)
                return;
            _Arrow.gameObject.SetActive(true);

            // The arrow is positioned on the edge of the bubble facing the target, shifted toward the center of the target but without extending beyond the bubble.
            Vector2 half = size * 0.5f;

            Vector2 local;
            float   angle;

            switch (side) {
                case PopupPlacement.Above:
                    local = new Vector2(Mathf.Clamp(target.center.x - popupCenter.x, -half.x + 20f, half.x - 20f), -half.y);
                    angle = 180f;

                    break;
                case PopupPlacement.Left:
                    local = new Vector2(half.x, Mathf.Clamp(target.center.y - popupCenter.y, -half.y + 20f, half.y - 20f));
                    angle = -90f;

                    break;
                case PopupPlacement.Right:
                    local = new Vector2(-half.x, Mathf.Clamp(target.center.y - popupCenter.y, -half.y + 20f, half.y - 20f));
                    angle = 90f;

                    break;
                default:
                    local = new Vector2(Mathf.Clamp(target.center.x - popupCenter.x, -half.x + 20f, half.x - 20f), half.y);
                    angle = 0f;

                    break;
            }

            Vector2 pivotOffset = (_Root.pivot - new Vector2(0.5f, 0.5f)) * size;

            _Arrow.localPosition = local - pivotOffset;
            _Arrow.localRotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            float dt    = Time.unscaledDeltaTime;
            float speed = _FadeDuration > 0 ? dt / _FadeDuration : 1f;

            _Group.alpha = Mathf.MoveTowards(_Group.alpha, _Shown ? 1f : 0f, speed);

            if (!_Shown && _Group.alpha <= 0f) {
                gameObject.SetActive(false);

                return;
            }

            if (_NudgeTime >= 0f) {
                _NudgeTime += dt;

                float t      = _NudgeTime / 0.35f;
                float offset = t < 1f ? Mathf.Sin(t * Mathf.PI * 6f) * _NudgeAmplitude * (1f - t) : 0f;

                _NudgeOffset = new Vector2(offset, 0f);

                if (t >= 1f)
                    _NudgeTime = -1f;
                // The anchored bubbles apply the offset in SetAnchor.
                if (!_AnchorToTarget)
                    _Root.anchoredPosition = _RestPosition + _NudgeOffset;
            }
        }
    }
}
