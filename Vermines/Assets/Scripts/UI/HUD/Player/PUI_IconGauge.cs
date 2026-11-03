using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

namespace Vermines.UI {

    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Round icon that fills like a jar (eloquence, souls), with its "value/max" counter.
    /// Layers, back to front: the shadow disc, a <see cref="Mask"/> disc holding the liquid, then the hollow icon.
    /// </summary>
    /// <remarks>
    /// The liquid is a sliced sprite: its top border holds the surface glow, the center is the plain liquid color.
    /// Its height follows the value, and it tilts a little when the value changes.
    /// </remarks>
    public class PUI_IconGauge : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Disc that clips the liquid.
        /// </summary>
        [SerializeField]
        private RectTransform _Mask;

        /// <summary>
        /// Liquid image (sliced, anchored at the bottom center of the mask).
        /// </summary>
        [SerializeField]
        private Image _Fill;

        /// <summary>
        /// Icon drawn above the liquid, punched with the mask and the background when the value changes
        /// (the three scale together so the liquid never shows outside the icon).
        /// </summary>
        [SerializeField]
        private RectTransform _Icon;

        /// <summary>
        /// Shadow disc behind the liquid, punched with the icon.
        /// </summary>
        [SerializeField]
        private RectTransform _Background;

        /// <summary>
        /// Counter under the icon.
        /// </summary>
        [SerializeField]
        private Text _Count;

        /// <summary>
        /// Counter format, {0} is the value and {1} the maximum.
        /// </summary>
        [SerializeField]
        private string _Format = "{0:00}/{1}";

        /// <summary>
        /// Width of the liquid compared to the mask, wider so the tilt never shows the edges.
        /// </summary>
        [Header("Liquid")]
        [SerializeField]
        private float _FillWidthFactor = 1.4f;

        /// <summary>
        /// Height of the glow above the surface, as a fraction of the liquid sprite width.
        /// </summary>
        [SerializeField]
        private float _SurfaceRatio = 0.088f;

        /// <summary>
        /// Value under which the liquid is hidden.
        /// </summary>
        [SerializeField]
        private float _EmptyThreshold = 0.001f;

        /// <summary>
        /// Duration of the fill and counter animation.
        /// </summary>
        [Header("Animation")]
        [SerializeField]
        private float _Duration = 0.6f;

        /// <summary>
        /// Tilt of the liquid when the value changes, in degrees.
        /// </summary>
        [SerializeField]
        private float _SloshAngle = 8f;

        /// <summary>
        /// Bubble sprite (white, tinted with <see cref="_LiquidColor"/>).
        /// </summary>
        [Header("Bubbles")]
        [SerializeField]
        private Sprite _BubbleSprite;

        /// <summary>
        /// Color of the bubbles and of the overflow drips.
        /// </summary>
        [SerializeField]
        private Color _LiquidColor = Color.white;

        /// <summary>
        /// Size range of a bubble when it leaves the surface.
        /// </summary>
        [SerializeField]
        private Vector2 _BubbleSize = new(5f, 10f);

        /// <summary>
        /// Height range a bubble climbs before vanishing.
        /// </summary>
        [SerializeField]
        private Vector2 _BubbleRise = new(45f, 80f);

        /// <summary>
        /// Lifetime range of a bubble.
        /// </summary>
        [SerializeField]
        private Vector2 _BubbleLifetime = new(1.1f, 1.8f);

        /// <summary>
        /// Smallest and biggest number of bubbles for one gain.
        /// </summary>
        [SerializeField]
        private Vector2Int _BubblesPerGain = new(3, 10);

        /// <summary>
        /// The value can go over the maximum (souls during the last turn): the gauge then overflows.
        /// </summary>
        [Header("Overflow")]
        [SerializeField]
        private bool _AllowOverflow = false;

        /// <summary>
        /// Delay between two bubbles while overflowing.
        /// </summary>
        [SerializeField]
        private float _OverflowBubbleDelay = 0.3f;

        /// <summary>
        /// Delay between two drips running down the icon while overflowing.
        /// </summary>
        [SerializeField]
        private float _OverflowDripDelay = 0.45f;

        /// <summary>
        /// Counter color while overflowing.
        /// </summary>
        [SerializeField]
        private Color _OverflowCountColor = new(1f, 0.84f, 0.35f, 1f);

        /// <summary>
        /// True while the value is over the maximum.
        /// </summary>
        public bool IsOverflowing => _AllowOverflow && _HasValue && _Value > _Max;

        /// <summary>
        /// Current value.
        /// </summary>
        public int Value => _Value;

        /// <summary>
        /// Current maximum.
        /// </summary>
        public int Max => _Max;

        private int _Value = -1;

        private int _Max = 1;

        private float _DisplayedValue;

        private bool _HasValue;

        private Tween _ValueTween;

        private RectTransform _BubbleLayer;

        private readonly Stack<Image> _BubblePool = new();

        private float _NextOverflowBubble;

        private float _NextOverflowDrip;

        private float _PunchEnd;

        private bool _WasOverflowing;

        private Color _CountColor = Color.white;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (_Count != null)
                _CountColor = _Count.color;
            SetupFill();
            ApplyDisplayed(0f);
        }

        private void Update()
        {
            bool overflowing = IsOverflowing;

            if (overflowing != _WasOverflowing)
                OnOverflowChanged(overflowing);
            if (!overflowing)
                return;
            float now = Time.unscaledTime;

            if (now >= _NextOverflowBubble) {
                _NextOverflowBubble = now + _OverflowBubbleDelay * Random.Range(0.6f, 1.4f);

                SpawnBubble(true);
            }
            if (now >= _NextOverflowDrip) {
                _NextOverflowDrip = now + _OverflowDripDelay * Random.Range(0.6f, 1.4f);

                SpawnDrip();
            }
            // Slow "boiling" pulse of the jar, paused while a punch plays.
            if (now >= _PunchEnd) {
                Vector3 scale = Vector3.one * (1f + 0.045f * Mathf.Sin(now * 6f));

                SetJarScale(scale);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_HasValue)
                ApplyDisplayed(_DisplayedValue);
        }

        private void OnDestroy()
        {
            _ValueTween?.Kill();

            DOTween.Kill(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows <paramref name="value"/> out of <paramref name="max"/>.
        /// </summary>
        /// <param name="instant">True to skip the animation (first display).</param>
        public void SetValue(int value, int max, bool instant = false)
        {
            max = Mathf.Max(1, max);

            if (_HasValue && value == _Value && max == _Max)
                return;
            bool raised = value > _Value;

            _Value = value;
            _Max   = max;

            _ValueTween?.Kill();

            if (instant || !_HasValue || !isActiveAndEnabled) {
                _HasValue = true;

                ApplyDisplayed(value);

                return;
            }
            int gain = value - Mathf.RoundToInt(_DisplayedValue);

            _ValueTween = DOTween.To(() => _DisplayedValue, ApplyDisplayed, value, _Duration).SetEase(Ease.OutCubic).SetTarget(this).SetLink(gameObject);

            PlayChangeFeedback(raised);

            if (gain > 0)
                PlayGainBubbles(gain);
        }

        private void SetupFill()
        {
            if (_Fill == null || _Mask == null)
                return;
            RectTransform rt = _Fill.rectTransform;

            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);

            _Fill.type = Image.Type.Sliced;

            float width = _Mask.rect.width * _FillWidthFactor;

            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);

            // Scales the sliced border with the liquid width, whatever the sprite resolution.
            if (_Fill.sprite != null && width > 0f)
                _Fill.pixelsPerUnitMultiplier = _Fill.sprite.rect.width / width * (100f / _Fill.sprite.pixelsPerUnit);
        }

        private void ApplyDisplayed(float value)
        {
            _DisplayedValue = value;

            if (_Fill != null && _Mask != null) {
                float level  = Mathf.Clamp01(value / _Max);
                float height = _Mask.rect.height;
                float glow   = _Fill.rectTransform.sizeDelta.x * _SurfaceRatio;

                _Fill.enabled = level > _EmptyThreshold;
                _Fill.rectTransform.anchoredPosition = Vector2.zero;
                _Fill.rectTransform.sizeDelta = new Vector2(_Fill.rectTransform.sizeDelta.x, level * height + glow);
            }
            if (_Count != null)
                _Count.text = string.Format(_Format, Mathf.RoundToInt(value), _Max);
        }

        private void PlayChangeFeedback(bool raised)
        {
            if (_Fill != null) {
                RectTransform fill = _Fill.rectTransform;

                fill.DOKill(true);
                fill.localRotation = Quaternion.identity;
                fill.DOPunchRotation(new Vector3(0f, 0f, raised ? _SloshAngle : -_SloshAngle), _Duration * 1.5f, 5, 0.6f).SetLink(fill.gameObject);
            }
            Vector3 punch = Vector3.one * (raised ? 0.15f : -0.1f);

            _PunchEnd = Time.unscaledTime + 0.3f;

            Punch(_Icon, punch);
            Punch(_Mask, punch);
            Punch(_Background, punch);
            if (_Count != null) {
                _Count.rectTransform.DOKill(true);
                _Count.rectTransform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6, 0.6f).SetLink(_Count.gameObject);
            }
        }

        private void SetJarScale(Vector3 scale)
        {
            if (_Icon != null)
                _Icon.localScale = scale;
            if (_Mask != null)
                _Mask.localScale = scale;
            if (_Background != null)
                _Background.localScale = scale;
        }

        private static void Punch(RectTransform target, Vector3 punch)
        {
            if (target == null)
                return;
            target.DOKill(true);
            target.localScale = Vector3.one;
            target.DOPunchScale(punch, 0.3f, 6, 0.6f).SetLink(target.gameObject);
        }

        #endregion

        #region Bubbles & Overflow

        private void OnOverflowChanged(bool overflowing)
        {
            _WasOverflowing = overflowing;

            if (_Count != null)
                _Count.color = overflowing ? _OverflowCountColor : _CountColor;
            if (!overflowing) {
                SetJarScale(Vector3.one);

                return;
            }
            // Burst when the jar starts to overflow.
            _NextOverflowBubble = Time.unscaledTime + 0.5f;
            _NextOverflowDrip   = Time.unscaledTime + 0.2f;

            for (int i = 0; i < 3; i++)
                DOVirtual.DelayedCall(0.1f + i * 0.12f, SpawnDrip).SetTarget(this).SetLink(gameObject);
        }

        private void PlayGainBubbles(int gain)
        {
            int count = Mathf.Clamp(Mathf.CeilToInt(gain / (float)_Max * 40f) + 2, _BubblesPerGain.x, _BubblesPerGain.y);

            for (int i = 0; i < count; i++)
                DOVirtual.DelayedCall(i * 0.08f, () => SpawnBubble(false)).SetTarget(this).SetLink(gameObject);
        }

        /// <summary>
        /// One bubble leaves the liquid surface (or the top of the icon when overflowing), grows, floats up and fades.
        /// </summary>
        private void SpawnBubble(bool fromTop)
        {
            if (_BubbleSprite == null || _Mask == null || !isActiveAndEnabled)
                return;
            Image bubble = GetBubble();
            RectTransform rt = bubble.rectTransform;

            float radius = _Mask.rect.width * 0.5f;
            float level  = Mathf.Clamp01(_DisplayedValue / _Max);
            float y      = fromTop ? radius * 0.85f : Mathf.Lerp(-radius, radius, level);
            float chord  = Mathf.Sqrt(Mathf.Max(0f, radius * radius - y * y));
            float size   = Random.Range(_BubbleSize.x, _BubbleSize.y);
            float life   = Random.Range(_BubbleLifetime.x, _BubbleLifetime.y);
            float rise   = Random.Range(_BubbleRise.x, _BubbleRise.y);
            float drift  = Random.Range(-14f, 14f);

            rt.anchoredPosition = new Vector2(Random.Range(-chord, chord) * 0.7f, y);
            rt.sizeDelta        = Vector2.one * size;
            rt.localScale       = Vector3.one * 0.3f;
            bubble.color        = new Color(_LiquidColor.r, _LiquidColor.g, _LiquidColor.b, 0f);

            Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(bubble.gameObject);

            seq.Append(bubble.DOFade(0.9f, 0.15f));
            seq.Join(rt.DOScale(1f, life * 0.5f).SetEase(Ease.OutBack));
            seq.Insert(0f, rt.DOAnchorPosY(y + rise, life).SetEase(Ease.OutSine));
            seq.Insert(0f, rt.DOAnchorPosX(rt.anchoredPosition.x + drift, life).SetEase(Ease.InOutSine));
            seq.Insert(life * 0.5f, rt.DOScale(1.6f, life * 0.5f));
            seq.Insert(life * 0.6f, bubble.DOFade(0f, life * 0.4f));
            seq.OnKill(() => ReleaseBubble(bubble));
        }

        /// <summary>
        /// One drop spills over the rim and runs down the outside of the icon.
        /// </summary>
        private void SpawnDrip()
        {
            if (_BubbleSprite == null || _Mask == null || !isActiveAndEnabled)
                return;
            Image drop = GetBubble();
            RectTransform rt = drop.rectTransform;

            float radius = _Mask.rect.width * 0.5f + 3f;
            float side   = Random.value < 0.5f ? -1f : 1f;
            float from   = side * Random.Range(5f, 25f);
            float to     = side * Random.Range(70f, 110f);
            float life   = Random.Range(0.8f, 1.2f);
            float size   = Random.Range(4f, 7f);

            rt.sizeDelta   = new Vector2(size, size * 1.4f);
            rt.localScale  = Vector3.one * 0.4f;
            drop.color     = new Color(_LiquidColor.r, _LiquidColor.g, _LiquidColor.b, 0f);

            PlaceOnRim(rt, radius, from);

            Sequence seq = DOTween.Sequence().SetTarget(this).SetLink(drop.gameObject);

            seq.Append(drop.DOFade(1f, 0.1f));
            seq.Join(rt.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Insert(0.1f, DOTween.To(() => from, angle => PlaceOnRim(rt, radius, angle), to, life).SetEase(Ease.InQuad));
            seq.Insert(0.1f + life * 0.6f, drop.DOFade(0f, life * 0.4f));
            seq.OnKill(() => ReleaseBubble(drop));
        }

        private static void PlaceOnRim(RectTransform rt, float radius, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;

            rt.anchoredPosition = new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);
            rt.localRotation    = Quaternion.Euler(0f, 0f, -angle);
        }

        private Image GetBubble()
        {
            EnsureBubbleLayer();

            Image bubble = _BubblePool.Count > 0 ? _BubblePool.Pop() : CreateBubble();

            bubble.gameObject.SetActive(true);
            bubble.rectTransform.SetAsLastSibling();

            return bubble;
        }

        private Image CreateBubble()
        {
            GameObject go = new("Bubble", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            go.layer = gameObject.layer;
            go.transform.SetParent(_BubbleLayer, false);

            Image image = go.GetComponent<Image>();

            image.sprite        = _BubbleSprite;
            image.raycastTarget = false;

            return image;
        }

        private void ReleaseBubble(Image bubble)
        {
            if (bubble == null)
                return;
            bubble.gameObject.SetActive(false);

            _BubblePool.Push(bubble);
        }

        /// <summary>
        /// Layer above the icon, centered on the jar, that holds the bubbles and drips.
        /// </summary>
        private void EnsureBubbleLayer()
        {
            if (_BubbleLayer == null) {
                GameObject go = new("Bubbles", typeof(RectTransform));

                go.layer     = gameObject.layer;
                _BubbleLayer = (RectTransform)go.transform;

                _BubbleLayer.SetParent(_Mask.parent, false);
                _BubbleLayer.sizeDelta = Vector2.zero;
            }
            _BubbleLayer.localPosition = _Mask.localPosition;

            if (_Icon != null && _Icon.parent == _BubbleLayer.parent)
                _BubbleLayer.SetSiblingIndex(_Icon.GetSiblingIndex() + 1);
        }

        #endregion
    }
}