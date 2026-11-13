using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI {

    /// <summary>
    /// Switch visual for a toggle: the handle slides along the track and swaps its sprite.
    /// Watches isOn every frame, so values loaded with SetIsOnWithoutNotify are picked up too.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitch : MonoBehaviour {

        #region Attributes

        [SerializeField] private RectTransform _Handle;
        [SerializeField] private Image _HandleImage;
        [SerializeField] private Sprite _OnSprite;
        [SerializeField] private Sprite _OffSprite;

        [Header("Positions (handle anchored X)")]
        [SerializeField] private float _OffX = -40f;
        [SerializeField] private float _OnX = 40f;

        [Header("Feel")]
        [SerializeField] private float _Duration = 0.25f;
        [SerializeField] private float _OffAlpha = 0.7f;

        private Toggle _Toggle;
        private bool _Shown;
        private Vector3 _BaseScale;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _Toggle    = GetComponent<Toggle>();
            _BaseScale = _Handle.localScale;
        }

        private void OnEnable()
        {
            Apply(_Toggle.isOn, true);
        }

        private void OnDisable()
        {
            DOTween.Kill(this);

            _Handle.localScale    = _BaseScale;
            _Handle.localRotation = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (_Toggle.isOn != _Shown)
                Apply(_Toggle.isOn, false);
        }

        #endregion

        #region Methods

        private void Apply(bool isOn, bool instant)
        {
            _Shown = isOn;

            DOTween.Kill(this);

            if (_HandleImage != null) {
                Sprite sprite = isOn ? _OnSprite : _OffSprite;

                if (sprite != null)
                    _HandleImage.sprite = sprite;
                Color color = _HandleImage.color;

                color.a            = isOn ? 1f : _OffAlpha;
                _HandleImage.color = color;
            }

            Vector2 position = new(isOn ? _OnX : _OffX, _Handle.anchoredPosition.y);

            if (instant) {
                _Handle.anchoredPosition = position;
                _Handle.localScale       = _BaseScale;
                _Handle.localRotation    = Quaternion.identity;

                return;
            }

            _Handle.localScale    = _BaseScale;
            _Handle.localRotation = Quaternion.identity;

            _Handle.DOAnchorPos(position, _Duration).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
            _Handle.DOPunchScale(_BaseScale * 0.15f, _Duration + 0.1f, 6, 0.6f).SetUpdate(true).SetTarget(this);
            _Handle.DOPunchRotation(new Vector3(0f, 0f, isOn ? -12f : 12f), _Duration + 0.15f, 8, 0.6f).SetUpdate(true).SetTarget(this);
        }

        #endregion
    }
}
