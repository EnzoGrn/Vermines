using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.Tutorial
{
    /// Un popup d'explication : bulle accrochée à une cible, fenêtre centrée ou
    /// narrateur dans un coin, selon le prefab. Le même script sert aux trois ; seul
    /// "anchorToTarget" change.
    ///
    /// Hiérarchie conseillée du prefab :
    ///   Popup (RectTransform, CanvasGroup, Image de fond, VerticalLayoutGroup + ContentSizeFitter)
    ///     Portrait (Image)          optionnel
    ///     Title (TMP_Text)          optionnel
    ///     Body (TMP_Text)
    ///     Buttons
    ///       Next (Button)           "Suivant"
    ///       Skip (Button)           "Passer la leçon", optionnel
    ///     Arrow (RectTransform)     optionnel, pointe vers le haut dans le prefab
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialPopupView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private RectTransform arrow;

        [Header("Placement")]
        [Tooltip("Vrai pour une bulle qui suit la cible. Faux pour une fenêtre à position fixe (modal, narrateur).")]
        [SerializeField] private bool anchorToTarget = true;
        [Tooltip("Distance entre la cible et la bulle.")]
        [SerializeField] private float gap = 24f;
        [Tooltip("Marge minimale avec les bords de l'écran.")]
        [SerializeField] private float screenMargin = 16f;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float nudgeAmplitude = 10f;

        private CanvasGroup _group;
        private bool _shown;
        private float _nudgeTime = -1f;
        private Vector2 _restPosition;
        private Vector2 _nudgeOffset;

        public event Action NextClicked;
        public event Action SkipClicked;

        public bool IsShown => _shown;

        /// Pour construire le popup par code (TutorialUIBuilder) : à appeler avant
        /// d'activer le GameObject.
        public void Configure(RectTransform root, TMP_Text title, TMP_Text body, Image portrait,
            Button next, Button skip, RectTransform arrow, bool anchorToTarget)
        {
            this.root = root;
            titleText = title;
            bodyText = body;
            portraitImage = portrait;
            nextButton = next;
            skipButton = skip;
            this.arrow = arrow;
            this.anchorToTarget = anchorToTarget;
        }

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            if (root == null)
                root = (RectTransform)transform;
            if (nextButton != null)
                nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            if (skipButton != null)
                skipButton.onClick.AddListener(() => SkipClicked?.Invoke());
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _restPosition = root.anchoredPosition;
        }

        public void Show(string title, string body, Sprite portrait, bool showNext, bool showSkip)
        {
            gameObject.SetActive(true);
            _shown = true;

            if (titleText != null)
            {
                titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
                titleText.text = title ?? string.Empty;
            }
            if (bodyText != null)
                bodyText.text = body ?? string.Empty;
            if (portraitImage != null)
            {
                portraitImage.gameObject.SetActive(portrait != null);
                portraitImage.sprite = portrait;
            }
            if (nextButton != null)
                nextButton.gameObject.SetActive(showNext);
            if (skipButton != null)
                skipButton.gameObject.SetActive(showSkip);
            if (arrow != null)
                arrow.gameObject.SetActive(false);

            _group.blocksRaycasts = true;
            _group.interactable = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            if (!anchorToTarget)
                root.anchoredPosition = _restPosition;
        }

        public void Hide()
        {
            _shown = false;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        /// Déplace un popup à position fixe (fenêtre, narrateur) : ancre, pivot et position.
        public void SetRestPosition(Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition)
        {
            var rt = root != null ? root : (RectTransform)transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = _restPosition = anchoredPosition;
        }

        /// Petite secousse quand le joueur tente une action interdite.
        public void Nudge() => _nudgeTime = 0f;

        /// Place la bulle à côté de la cible (rectangle écran). Appelé à chaque frame
        /// par le TutorialUIController pour suivre les cibles qui bougent.
        public void SetAnchor(Rect? targetScreenRect, PopupPlacement placement, Camera canvasCamera)
        {
            if (!anchorToTarget || !_shown)
                return;

            var parent = root.parent as RectTransform;
            if (parent == null)
                return;

            if (targetScreenRect == null)
            {
                root.anchoredPosition = _restPosition;
                if (arrow != null)
                    arrow.gameObject.SetActive(false);
                return;
            }

            Rect sr = targetScreenRect.Value;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sr.min, canvasCamera, out Vector2 tMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sr.max, canvasCamera, out Vector2 tMax);
            Rect target = Rect.MinMaxRect(tMin.x, tMin.y, tMax.x, tMax.y);
            Rect bounds = parent.rect;
            Vector2 size = root.rect.size;

            PopupPlacement side = placement == PopupPlacement.Auto ? ChooseSide(target, bounds, size) : placement;
            Vector2 center;
            switch (side)
            {
                case PopupPlacement.Above: center = new Vector2(target.center.x, target.yMax + gap + size.y * 0.5f); break;
                case PopupPlacement.Left: center = new Vector2(target.xMin - gap - size.x * 0.5f, target.center.y); break;
                case PopupPlacement.Right: center = new Vector2(target.xMax + gap + size.x * 0.5f, target.center.y); break;
                default: center = new Vector2(target.center.x, target.yMin - gap - size.y * 0.5f); break;
            }

            // Garde la bulle dans l'écran.
            center.x = Mathf.Clamp(center.x, bounds.xMin + screenMargin + size.x * 0.5f, bounds.xMax - screenMargin - size.x * 0.5f);
            center.y = Mathf.Clamp(center.y, bounds.yMin + screenMargin + size.y * 0.5f, bounds.yMax - screenMargin - size.y * 0.5f);

            // Position en coordonnées du parent → anchoredPosition, quel que soit le pivot/ancrage du prefab.
            Vector2 pivotOffset = (root.pivot - new Vector2(0.5f, 0.5f)) * size;
            Vector3 world = parent.TransformPoint(center + pivotOffset);
            root.position = world;
            _restPosition = root.anchoredPosition;
            root.anchoredPosition += _nudgeOffset;

            PlaceArrow(side, target, center, size);
        }

        private static PopupPlacement ChooseSide(Rect target, Rect bounds, Vector2 size)
        {
            float below = target.yMin - bounds.yMin;
            float above = bounds.yMax - target.yMax;
            float left = target.xMin - bounds.xMin;
            float right = bounds.xMax - target.xMax;

            // Au-dessus/en dessous en priorité (lecture plus naturelle), sinon le côté le plus large.
            if (above >= size.y + 40f && above >= below) return PopupPlacement.Above;
            if (below >= size.y + 40f) return PopupPlacement.Below;
            if (above >= size.y + 40f) return PopupPlacement.Above;
            return right >= left ? PopupPlacement.Right : PopupPlacement.Left;
        }

        private void PlaceArrow(PopupPlacement side, Rect target, Vector2 popupCenter, Vector2 size)
        {
            if (arrow == null)
                return;
            arrow.gameObject.SetActive(true);

            // La flèche se place sur le bord de la bulle qui fait face à la cible,
            // décalée vers le centre de la cible mais sans sortir de la bulle.
            Vector2 half = size * 0.5f;
            Vector2 local;
            float angle;
            switch (side)
            {
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
            Vector2 pivotOffset = (root.pivot - new Vector2(0.5f, 0.5f)) * size;
            arrow.localPosition = local - pivotOffset;
            arrow.localRotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float speed = fadeDuration > 0 ? dt / fadeDuration : 1f;
            _group.alpha = Mathf.MoveTowards(_group.alpha, _shown ? 1f : 0f, speed);
            if (!_shown && _group.alpha <= 0f)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_nudgeTime >= 0f)
            {
                _nudgeTime += dt;
                float t = _nudgeTime / 0.35f;
                float offset = t < 1f ? Mathf.Sin(t * Mathf.PI * 6f) * nudgeAmplitude * (1f - t) : 0f;
                _nudgeOffset = new Vector2(offset, 0f);
                if (t >= 1f)
                    _nudgeTime = -1f;
                // Les bulles accrochées appliquent le décalage dans SetAnchor.
                if (!anchorToTarget)
                    root.anchoredPosition = _restPosition + _nudgeOffset;
            }
        }
    }
}
