using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vermines.UI {

    /// <summary>
    /// Slide + scale on hover, squash on press, punch on click, optional highlight band.
    /// Move a child visual, not an object placed by a layout group, or the layout will fight the tweens.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

        #region Attributes

        [Tooltip("Moved and scaled. Defaults to this RectTransform.")]
        [SerializeField] private RectTransform _Visual;

        [Tooltip("Optional brush band behind the label, faded in on hover or while pinned.")]
        [SerializeField] private CanvasGroup _Highlight;

        [Header("Feel")]
        [SerializeField] private Vector2 _HoverSlide = new(14f, 0f);
        [SerializeField] private float _HoverScale = 1.05f;
        [SerializeField] private float _PressScale = 0.96f;

        private Button _Button;
        private Vector2 _BasePos;
        private Vector3 _BaseScale;
        private bool _Ready;
        private bool _Hovered;
        private bool _Pinned;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _Button = GetComponent<Button>();

            if (_Visual == null)
                _Visual = (RectTransform)transform;
        }

        // Captured on enable: layouts and entrance tweens have settled the rest pose by then.
        private void OnEnable()
        {
            _BasePos   = _Visual.anchoredPosition;
            _BaseScale = _Visual.localScale;
            _Ready     = true;
            _Hovered   = false;

            _Button.onClick.AddListener(Punch);

            if (_Highlight != null)
                _Highlight.alpha = _Pinned ? 1f : 0f;
        }

        private void OnDisable()
        {
            _Button.onClick.RemoveListener(Punch);

            DOTween.Kill(this);

            if (_Ready) {
                _Visual.anchoredPosition = _BasePos;
                _Visual.localScale       = _BaseScale;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Keeps the highlight on (for example an expanded settings section).
        /// </summary>
        public void SetPinned(bool pinned)
        {
            _Pinned = pinned;

            FadeHighlight(0.15f);
        }

        private void Punch()
        {
            DOTween.Kill(this);

            _Visual.localScale = _BaseScale;
            _Visual.DOPunchScale(_BaseScale * 0.08f, 0.25f, 6, 0.6f).SetUpdate(true).SetTarget(this);

            FadeHighlight(0f);
        }

        private void To(Vector2 pos, Vector3 scale, float duration)
        {
            if (!_Ready)
                return;
            DOTween.Kill(this);

            _Visual.DOAnchorPos(pos, duration).SetEase(Ease.OutQuad).SetUpdate(true).SetTarget(this);
            _Visual.DOScale(scale, duration).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);

            FadeHighlight(duration);
        }

        private void FadeHighlight(float duration)
        {
            if (_Highlight == null)
                return;
            float alpha = _Pinned || (_Hovered && _Button.interactable) ? 1f : 0f;

            if (duration <= 0f || !isActiveAndEnabled) {
                _Highlight.alpha = alpha;

                return;
            }

            _Highlight.DOFade(alpha, duration).SetUpdate(true).SetTarget(this);
        }

        #endregion

        #region Events

        public void OnPointerEnter(PointerEventData eventData)
        {
            _Hovered = true;

            if (_Button.interactable)
                To(_BasePos + _HoverSlide, _BaseScale * _HoverScale, 0.15f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _Hovered = false;

            To(_BasePos, _BaseScale, 0.2f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_Button.interactable)
                To(_BasePos + _HoverSlide, _BaseScale * _PressScale, 0.07f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_Hovered && _Button.interactable)
                To(_BasePos + _HoverSlide, _BaseScale * _HoverScale, 0.1f);
        }

        #endregion
    }
}
