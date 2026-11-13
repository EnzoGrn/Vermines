using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vermines.UI {

    /// <summary>
    /// Slider handle feedback: grows while grabbed and tilts in the drag direction.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class SliderHandleJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

        #region Attributes

        [Tooltip("Defaults to the slider handle rect.")]
        [SerializeField] private RectTransform _Handle;

        [Header("Feel")]
        [SerializeField] private float _PressScale = 1.15f;
        [SerializeField] private float _TiltPerUnit = 400f; // degrees per full slider length
        [SerializeField] private float _MaxTilt = 18f;

        private Slider _Slider;
        private Vector3 _BaseScale;
        private float _LastNormalized;

        private Tween _ScaleTween;
        private Tween _TiltTween;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _Slider = GetComponent<Slider>();

            if (_Handle == null)
                _Handle = _Slider.handleRect;
            _BaseScale = _Handle.localScale;
        }

        private void OnEnable()
        {
            _LastNormalized = _Slider.normalizedValue;

            _Slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _Slider.onValueChanged.RemoveListener(OnValueChanged);

            _ScaleTween?.Kill();
            _TiltTween?.Kill();

            _Handle.localScale    = _BaseScale;
            _Handle.localRotation = Quaternion.identity;
        }

        #endregion

        #region Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_Slider.IsInteractable())
                return;
            _LastNormalized = _Slider.normalizedValue;

            _ScaleTween?.Kill();
            _ScaleTween = _Handle.DOScale(_BaseScale * _PressScale, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _ScaleTween?.Kill();
            _ScaleTween = _Handle.DOScale(_BaseScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void OnValueChanged(float value)
        {
            float normalized = _Slider.normalizedValue;
            float delta      = normalized - _LastNormalized;

            _LastNormalized = normalized;

            float tilt = Mathf.Clamp(-delta * _TiltPerUnit, -_MaxTilt, _MaxTilt);

            _TiltTween?.Kill();

            _Handle.localRotation = Quaternion.Euler(0f, 0f, tilt);
            _TiltTween = _Handle.DOLocalRotate(Vector3.zero, 0.35f).SetEase(Ease.OutElastic, 1f, 0.5f).SetUpdate(true);
        }

        #endregion
    }
}
