using DG.Tweening;
using UnityEngine;

namespace Vermines.UI.View {

    using Vermines.UI.Effects;

    /// <summary>
    /// Drives a scroll panel: dim fade, scroll rising from the bottom centre, staggered entries, logo pop.
    /// Pure presentation, owned by the view that decides when to call it.
    /// The pause and the gameplay settings share one parchment: when the settings open, the pause goes "aside",
    /// i.e. its entries leave and its parchment rises a bit more, while the settings content rides up on top of it.
    /// </summary>
    public class PausePanelAnimator : MonoBehaviour {

        #region Attributes

        [Header("Targets")]
        [Tooltip("Whole view (alpha + raycasts).")]
        [SerializeField] private CanvasGroup _Root;

        [Tooltip("Optional dim / blur group. Leave empty when another view already owns the dim.")]
        [SerializeField] private CanvasGroup _Dim;

        [Tooltip("The scroll, or the content riding on it. Pivot (0.5, 0), anchored bottom centre.")]
        [SerializeField] private RectTransform _Panel;

        [Tooltip("Optional. Faded with the panel (content that has no parchment of its own).")]
        [SerializeField] private CanvasGroup _PanelGroup;

        [Tooltip("Entrance objects (not the buttons, not the layout slots), top to bottom.")]
        [SerializeField] private CanvasGroup[] _Items;

        [Tooltip("Optional. Faded only (borders, ornaments), in with the entries, out with them.")]
        [SerializeField] private CanvasGroup[] _Decor;

        [Tooltip("Optional. Pops with a scale when no ink reveal is set.")]
        [SerializeField] private RectTransform _Logo;

        [Tooltip("Optional. Draws the logo like an ink blot instead of the scale pop.")]
        [SerializeField] private InkReveal _LogoInk;

        [Header("Feel")]
        [SerializeField] private float _RiseDistance = 1100f;
        [SerializeField] private float _HiddenScale  = 0.92f;
        [SerializeField] private float _DimAlpha     = 1f;
        [SerializeField] private float _ShowDuration = 0.5f;
        [SerializeField] private float _HideDuration = 0.3f;
        [SerializeField] private float _ItemsDelay   = 0.25f;
        [SerializeField] private float _ItemStagger  = 0.07f;
        [SerializeField] private float _ItemSlide    = 60f;

        [Tooltip("How far the panel goes past its rest pose before settling (OutBack). 0 = no bounce (OutCubic). "
               + "Keep it low for long rises: the overshoot grows with _RiseDistance.")]
        [Min(0f)]
        [SerializeField] private float _Overshoot = 1.1f;

        [Header("Shared parchment")]
        [Tooltip("Hide(keepDim) raises the panel by this much instead of sending it away (0 = sends it away).")]
        [SerializeField] private float _AsideRise;

        [Tooltip("Off for content riding on another parchment: hiding only fades it, the parchment does the moving.")]
        [SerializeField] private bool _HideMovesPanel = true;

        private bool _Initialized;

        private Vector2 _PanelRest;
        private Vector3 _PanelScale;
        private Vector2[] _ItemRest;
        private Vector3 _LogoScale;

        #endregion

        #region Getters & Setters

        /// <summary>
        /// Time for the scroll to settle, used by owners to start their own reveal.
        /// </summary>
        public float ShowDuration => _ShowDuration;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Instant state, no animation.
        /// </summary>
        public void Snap(bool visible)
        {
            Initialize();

            DOTween.Kill(this);

            _Root.alpha          = visible ? 1f : 0f;
            _Root.blocksRaycasts = visible;

            if (_Dim != null)
                _Dim.alpha = visible ? _DimAlpha : 0f;
            if (_PanelGroup != null)
                _PanelGroup.alpha = visible ? 1f : 0f;
            _Panel.anchoredPosition = visible ? _PanelRest : HiddenPanelPosition();
            _Panel.localScale       = visible ? _PanelScale : _PanelScale * _HiddenScale;

            for (int i = 0; i < _Items.Length; i++) {
                _Items[i].alpha = visible ? 1f : 0f;

                ItemRect(i).anchoredPosition = visible ? _ItemRest[i] : HiddenItemPosition(i);
            }

            foreach (CanvasGroup decor in _Decor)
                decor.alpha = visible ? 1f : 0f;
            if (_LogoInk != null)
                _LogoInk.Snap(visible);
            else if (_Logo != null)
                _Logo.localScale = visible ? _LogoScale : Vector3.zero;
        }

        /// <summary>
        /// Brings the panel to its rest pose from wherever it is (hidden below, or raised aside).
        /// </summary>
        public Sequence Show()
        {
            Initialize();

            DOTween.Kill(this);

            _Root.alpha          = 1f;
            _Root.blocksRaycasts = true;

            Sequence s = DOTween.Sequence().SetTarget(this).SetUpdate(true);

            if (_Dim != null)
                s.Join(_Dim.DOFade(_DimAlpha, 0.25f));
            if (_PanelGroup != null)
                s.Join(_PanelGroup.DOFade(1f, 0.3f));
            s.Join(_Panel.DOAnchorPos(_PanelRest, _ShowDuration).SetEase(Ease.OutBack, _Overshoot));
            s.Join(_Panel.DOScale(_PanelScale, _ShowDuration).SetEase(Ease.OutBack, _Overshoot));

            for (int i = 0; i < _Items.Length; i++) {
                float at = _ItemsDelay + i * _ItemStagger;

                s.Insert(at, _Items[i].DOFade(1f, 0.2f));
                s.Insert(at, ItemRect(i).DOAnchorPos(_ItemRest[i], 0.3f).SetEase(Ease.OutCubic));
            }

            foreach (CanvasGroup decor in _Decor)
                s.Insert(_ItemsDelay, decor.DOFade(1f, 0.3f));
            float logoAt = _ItemsDelay + _Items.Length * _ItemStagger;

            if (_LogoInk != null)
                s.Insert(logoAt, _LogoInk.Reveal()); // hides the logo now, draws it at logoAt
            else if (_Logo != null)
                s.Insert(logoAt, _Logo.DOScale(_LogoScale, 0.35f).SetEase(Ease.OutBack));
            return s;
        }

        /// <param name="keepDim">True when another view opens on top: keeps the dim and the root visible,
        /// and raises the panel when _AsideRise is set.</param>
        /// <remarks>Uses no OnComplete itself, so callers are free to chain one.</remarks>
        public Sequence Hide(bool keepDim = false)
        {
            Initialize();

            DOTween.Kill(this);

            _Root.blocksRaycasts = false;

            Sequence s = DOTween.Sequence().SetTarget(this).SetUpdate(true);

            for (int i = 0; i < _Items.Length; i++) {
                s.Join(_Items[i].DOFade(0f, 0.12f));
                s.Join(ItemRect(i).DOAnchorPos(_ItemRest[i] + Vector2.left * (_ItemSlide * 0.5f), 0.15f));
            }

            foreach (CanvasGroup decor in _Decor)
                s.Join(decor.DOFade(0f, 0.15f));
            if (_LogoInk != null)
                s.Join(_LogoInk.Fade());
            else if (_Logo != null)
                s.Join(_Logo.DOScale(0f, 0.15f).SetEase(Ease.InBack));
            if (_PanelGroup != null)
                s.Join(_PanelGroup.DOFade(0f, 0.15f));
            bool raiseAside = keepDim && _AsideRise > 0f;

            if (raiseAside) {
                // Same duration and ease as Show(), so content showing up on top rides along with the parchment.
                s.Join(_Panel.DOAnchorPos(_PanelRest + Vector2.up * _AsideRise, _ShowDuration).SetEase(Ease.OutBack, _Overshoot));
            } else if (_HideMovesPanel) {
                float duration = keepDim ? _HideDuration * 0.8f : _HideDuration;

                s.Insert(0.05f, _Panel.DOAnchorPos(HiddenPanelPosition(), duration).SetEase(Ease.InBack));
                s.Insert(0.05f, _Panel.DOScale(_PanelScale * _HiddenScale, duration).SetEase(Ease.InQuad));
            }

            if (!keepDim) {
                if (_Dim != null)
                    s.Insert(0.15f, _Dim.DOFade(0f, 0.25f));
                s.AppendCallback(() => _Root.alpha = 0f);
            }

            return s;
        }

        private void Initialize()
        {
            if (_Initialized)
                return;
            _Initialized = true;

            _Items ??= new CanvasGroup[0];
            _Decor ??= new CanvasGroup[0];

            _PanelRest  = _Panel.anchoredPosition;
            _PanelScale = _Panel.localScale;
            _ItemRest   = new Vector2[_Items.Length];

            for (int i = 0; i < _Items.Length; i++)
                _ItemRest[i] = ItemRect(i).anchoredPosition;
            if (_Logo != null)
                _LogoScale = _Logo.localScale;
        }

        private RectTransform ItemRect(int index)
        {
            return (RectTransform)_Items[index].transform;
        }

        private Vector2 HiddenPanelPosition()
        {
            return _PanelRest + Vector2.down * _RiseDistance;
        }

        private Vector2 HiddenItemPosition(int index)
        {
            return _ItemRest[index] + Vector2.left * _ItemSlide;
        }

        #endregion
    }
}
