using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vermines.UI
{
    /// <summary>
    /// Hover, press, selected and refused feedback of one nav bar button.
    /// Only the visuals are moved and scaled, never the button root, so the hit area stays still.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NavButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visuals (moved and scaled)")]
        [SerializeField] private RectTransform[] _Visuals;

        [Header("Feel")]
        [SerializeField] private float _HoverLift = 10f;
        [SerializeField] private float _SelectedLift = 26f;
        [SerializeField] private float _HoverScale = 1.06f;
        [SerializeField] private float _PressScale = 0.93f;
        [SerializeField] private float _Duration = 0.15f;
        [SerializeField] private float _ClickPunch = 0.12f;
        [SerializeField] private float _RefusedShake = 10f;

        private Vector2[] _BasePositions;
        private Vector3[] _BaseScales;

        private Button _Button;
        private bool _Hovered;
        private bool _Pressed;
        private bool _Selected;

        private void Awake()
        {
            _Button = GetComponent<Button>();

            if (_Visuals == null || _Visuals.Length == 0)
            {
                Debug.LogError("[NavButtonJuice] No visuals assigned.", this);
                _Visuals = new RectTransform[0];
            }

            _BasePositions = new Vector2[_Visuals.Length];
            _BaseScales = new Vector3[_Visuals.Length];

            for (int i = 0; i < _Visuals.Length; i++)
            {
                _BasePositions[i] = _Visuals[i].anchoredPosition;
                _BaseScales[i] = _Visuals[i].localScale;
            }
        }

        private void OnEnable()
        {
            Apply(true);
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
        }

        public void OnPointerEnter(PointerEventData eventData) { _Hovered = true; Apply(false); }
        public void OnPointerExit(PointerEventData eventData) { _Hovered = false; _Pressed = false; Apply(false); }
        public void OnPointerDown(PointerEventData eventData) { _Pressed = true; Apply(false); }
        public void OnPointerUp(PointerEventData eventData) { _Pressed = false; Apply(false); }

        public void SetSelected(bool selected)
        {
            if (_Selected == selected)
                return;

            _Selected = selected;

            Apply(false);
        }

        /// <summary>The action was accepted.</summary>
        public void PlayClick()
        {
            DOTween.Kill(this);

            for (int i = 0; i < _Visuals.Length; i++)
                _Visuals[i].DOPunchScale(_BaseScales[i] * _ClickPunch, 0.25f, 6, 0.8f)
                    .SetTarget(this).SetUpdate(true).SetLink(_Visuals[i].gameObject)
                    .OnComplete(() => Apply(false));
        }

        /// <summary>The action was refused (for example an effect is pending).</summary>
        public void PlayRefused()
        {
            DOTween.Kill(this);

            for (int i = 0; i < _Visuals.Length; i++)
                _Visuals[i].DOShakeAnchorPos(0.3f, new Vector2(_RefusedShake, 0f), 18, 90f, false, true)
                    .SetTarget(this).SetUpdate(true).SetLink(_Visuals[i].gameObject)
                    .OnComplete(() => Apply(false));
        }

        private void Apply(bool instant)
        {
            if (_BasePositions == null)
                return;

            DOTween.Kill(this);

            bool canHover = _Button != null && _Button.interactable;
            float lift = _Selected ? _SelectedLift : (_Hovered && canHover ? _HoverLift : 0f);
            float scale = _Pressed && canHover ? _PressScale : (_Hovered && canHover ? _HoverScale : 1f);

            for (int i = 0; i < _Visuals.Length; i++)
            {
                Vector2 position = _BasePositions[i] + Vector2.up * lift;
                Vector3 targetScale = _BaseScales[i] * scale;

                if (instant)
                {
                    _Visuals[i].anchoredPosition = position;
                    _Visuals[i].localScale = targetScale;

                    continue;
                }

                _Visuals[i].DOAnchorPos(position, _Duration).SetEase(Ease.OutBack)
                    .SetTarget(this).SetUpdate(true).SetLink(_Visuals[i].gameObject);
                _Visuals[i].DOScale(targetScale, _Duration * 0.8f).SetEase(Ease.OutQuad)
                    .SetTarget(this).SetUpdate(true).SetLink(_Visuals[i].gameObject);
            }
        }
    }
}
