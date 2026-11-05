using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;

using Random = System.Random;

namespace Vermines.UI.GameTable {

    /// <summary>
    /// Blood drawn on the play mat, under the cards. Two effects that can be combined:
    /// <list type="bullet">
    /// <item>stains that grow with the souls of the player (the mat shows the progression towards the victory);</item>
    /// <item>a splash, with drops, when a partisan is sacrificed. It disappears when the table closes.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Put it on a child of the mat that fills it. Give it a Mask using the mat sprite (see <see cref="PUI_PlayMat"/>)
    /// so the blood follows the torn edges of the mat. The stains are placed from a seed, so a player always gets the same ones.
    /// </remarks>
    public class PUI_PlayMatBlood : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Parent of the blood images (this transform when empty).
        /// </summary>
        [Header("References")]
        [SerializeField]
        private RectTransform _Container;

        /// <summary>
        /// Splash sprites (loaded from <see cref="_ResourcesFolder"/> when empty).
        /// </summary>
        [SerializeField]
        private Sprite[] _SplashSprites = Array.Empty<Sprite>();

        /// <summary>
        /// Drop sprites (loaded from <see cref="_ResourcesFolder"/> when empty).
        /// </summary>
        [SerializeField]
        private Sprite[] _DropSprites = Array.Empty<Sprite>();

        /// <summary>
        /// Resources folder holding Blood_Splash_* and Blood_Drop_* sprites.
        /// </summary>
        [SerializeField]
        private string _ResourcesFolder = "Sprites/UI/Table";

        /// <summary>
        /// Color of the blood (the sprites are light grey and get tinted).
        /// </summary>
        [SerializeField]
        private Color _Color = new Color(0.45f, 0.02f, 0.04f, 0.9f);

        /// <summary>
        /// Shows stains that grow with the souls of the player.
        /// </summary>
        [Header("Souls stains")]
        [SerializeField]
        private bool _ShowSoulsStains = true;

        /// <summary>
        /// Number of stains once the souls limit is reached.
        /// </summary>
        [SerializeField, Min(1)]
        private int _MaxStains = 9;

        /// <summary>
        /// Size of a stain, picked between x and y.
        /// </summary>
        [SerializeField]
        private Vector2 _StainSize = new Vector2(130f, 260f);

        /// <summary>
        /// Opacity of the stains (dried blood is a little lighter than a fresh splash).
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        private float _StainAlpha = 0.8f;

        /// <summary>
        /// Part of the mat (normalized, centered) kept free of stains because the cards hide it.
        /// </summary>
        [SerializeField]
        private Vector2 _CardsArea = new Vector2(0.7f, 0.45f);

        /// <summary>
        /// Duration of the appearance of a new stain.
        /// </summary>
        [SerializeField]
        private float _StainDuration = 0.6f;

        /// <summary>
        /// Splashes blood when a partisan is sacrificed.
        /// </summary>
        [Header("Sacrifice splash")]
        [SerializeField]
        private bool _ShowSacrificeSplash = true;

        /// <summary>
        /// Size of the splash.
        /// </summary>
        [SerializeField]
        private float _SplashSize = 320f;

        /// <summary>
        /// Duration of the splash impact.
        /// </summary>
        [SerializeField]
        private float _SplashDuration = 0.22f;

        /// <summary>
        /// Number of drops thrown around the splash.
        /// </summary>
        [SerializeField, Min(0)]
        private int _DropCount = 9;

        /// <summary>
        /// Distance travelled by a drop, picked between x and y.
        /// </summary>
        [SerializeField]
        private Vector2 _DropDistance = new Vector2(90f, 230f);

        /// <summary>
        /// Size of a drop, picked between x and y.
        /// </summary>
        [SerializeField]
        private Vector2 _DropSize = new Vector2(16f, 40f);

        /// <summary>
        /// The splashes disappear when the table closes.
        /// </summary>
        [SerializeField]
        private bool _ClearSplashesOnClose = true;

        #endregion

        private class Stain {

            public Vector2 Position;

            public float Size;

            public float Rotation;

            public int SpriteIndex;

            public Image Image;
        }

        private readonly List<Stain> _Stains = new();

        private readonly List<Image> _Splashes = new();

        private RectTransform _StainsRoot;

        private RectTransform _SplashesRoot;

        private int _Seed = int.MinValue;

        private int _ShownStains = -1;

        private Random _SplashRandom = new Random();

        private bool _Initialized;

        #region Unity Lifecycle

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// The mat may feed the layer before its Awake (parent enabled first).
        /// </summary>
        private void EnsureInitialized()
        {
            if (_Initialized)
                return;
            _Initialized = true;

            if (_Container == null)
                _Container = transform as RectTransform;
            LoadSprites();

            _StainsRoot   = CreateLayer("Stains");
            _SplashesRoot = CreateLayer("Splashes");
        }

        private void OnDisable()
        {
            // The table screen is hidden: the fresh blood goes with it.
            if (_ClearSplashesOnClose)
                ClearSplashes(true);
        }

        private void OnDestroy()
        {
            foreach (Stain stain in _Stains) {
                if (stain.Image != null)
                    stain.Image.DOKill();
            }
            ClearSplashes(true);
        }

        #endregion

        #region Souls stains

        /// <summary>
        /// Shows as many stains as <paramref name="souls"/> deserve. The stains are placed from <paramref name="seed"/>.
        /// </summary>
        public void SetSouls(int souls, int soulsLimit, int seed)
        {
            EnsureInitialized();

            if (seed != _Seed)
                BuildStains(seed);
            int count = 0;

            if (_ShowSoulsStains && souls > 0) {
                float progress = Mathf.Clamp01(souls / (float)Mathf.Max(1, soulsLimit));

                count = Mathf.Clamp(Mathf.CeilToInt(progress * _MaxStains), 1, _MaxStains);
            }

            if (count == _ShownStains)
                return;
            bool animate = _ShownStains >= 0 && isActiveAndEnabled;

            for (int i = 0; i < _Stains.Count; i++)
                ShowStain(_Stains[i], i < count, animate);
            _ShownStains = count;
        }

        private void BuildStains(int seed)
        {
            foreach (Stain stain in _Stains) {
                if (stain.Image != null) {
                    stain.Image.DOKill();
                    Destroy(stain.Image.gameObject);
                }
            }
            _Stains.Clear();

            _Seed        = seed;
            _ShownStains = -1;

            if (_SplashSprites.Length == 0)
                return;
            Random random = new Random(seed);

            for (int i = 0; i < _MaxStains; i++) {
                // Later stains are bigger: the mat gets bloodier as the player gets closer to the victory.
                float growth = Mathf.Lerp(0.7f, 1.15f, i / (float)Mathf.Max(1, _MaxStains - 1));

                _Stains.Add(new Stain {
                    Position    = PickStainPosition(random),
                    Size        = Mathf.Lerp(_StainSize.x, _StainSize.y, (float)random.NextDouble()) * growth,
                    Rotation    = (float)random.NextDouble() * 360f,
                    SpriteIndex = random.Next(_SplashSprites.Length)
                });
            }
        }

        private Vector2 PickStainPosition(Random random)
        {
            Vector2 position = Vector2.zero;

            // Prefers the borders of the mat: the middle is hidden by the cards.
            for (int attempt = 0; attempt < 24; attempt++) {
                position = new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f) * 0.92f;

                if (Mathf.Abs(position.x) > _CardsArea.x * 0.5f || Mathf.Abs(position.y) > _CardsArea.y * 0.5f)
                    break;
            }
            return position;
        }

        private void ShowStain(Stain stain, bool visible, bool animate)
        {
            if (!visible && stain.Image == null)
                return;
            if (stain.Image == null) {
                Rect rect = _Container.rect;

                stain.Image = CreateBlood("Stain", _StainsRoot, _SplashSprites[stain.SpriteIndex], stain.Size);
                stain.Image.rectTransform.anchoredPosition = new Vector2(stain.Position.x * rect.width, stain.Position.y * rect.height);
                stain.Image.rectTransform.localEulerAngles = new Vector3(0f, 0f, stain.Rotation);
                SetAlpha(stain.Image, 0f);
            }
            Image image = stain.Image;
            float alpha = visible ? _Color.a * _StainAlpha : 0f;

            image.DOKill();
            image.rectTransform.DOKill();

            if (!animate) {
                SetAlpha(image, alpha);
                image.rectTransform.localScale = Vector3.one;
                image.gameObject.SetActive(visible);

                return;
            }
            image.gameObject.SetActive(true);

            if (visible) {
                image.rectTransform.localScale = Vector3.one * 0.5f;
                image.rectTransform.DOScale(1f, _StainDuration).SetEase(Ease.OutCubic).SetUpdate(true);
            }
            image.DOFade(alpha, _StainDuration).SetUpdate(true).OnComplete(() => {
                if (!visible)
                    image.gameObject.SetActive(false);
            });
        }

        #endregion

        #region Sacrifice splash

        /// <summary>
        /// Splashes blood where <paramref name="target"/> stands (the center of the mat when null).
        /// </summary>
        public void PlaySplash(RectTransform target)
        {
            EnsureInitialized();

            if (!_ShowSacrificeSplash || _SplashSprites.Length == 0 || !isActiveAndEnabled)
                return;
            Vector2 center = Vector2.zero;

            if (target != null) {
                Vector3 world = target.TransformPoint(target.rect.center);

                center = _SplashesRoot.InverseTransformPoint(world);
            }
            PlaySplashAt(center);
        }

        /// <summary>
        /// Splashes blood at <paramref name="center"/> (local position in the blood layer).
        /// </summary>
        public void PlaySplashAt(Vector2 center)
        {
            if (!_ShowSacrificeSplash || _SplashSprites.Length == 0 || !isActiveAndEnabled)
                return;
            Sprite sprite = _SplashSprites[_SplashRandom.Next(_SplashSprites.Length)];
            Image splash  = CreateBlood("Splash", _SplashesRoot, sprite, _SplashSize * Mathf.Lerp(0.9f, 1.1f, (float)_SplashRandom.NextDouble()));

            splash.rectTransform.anchoredPosition = center;
            splash.rectTransform.localEulerAngles = new Vector3(0f, 0f, (float)_SplashRandom.NextDouble() * 360f);
            splash.rectTransform.localScale       = Vector3.one * 0.2f;

            DOTween.Sequence().SetUpdate(true).SetTarget(splash)
                .Append(splash.rectTransform.DOScale(1.08f, _SplashDuration * 0.6f).SetEase(Ease.OutQuad))
                .Append(splash.rectTransform.DOScale(1f, _SplashDuration * 0.4f).SetEase(Ease.InOutSine));
            _Splashes.Add(splash);

            if (_DropSprites.Length == 0)
                return;
            for (int i = 0; i < _DropCount; i++)
                ThrowDrop(center);
        }

        private void ThrowDrop(Vector2 center)
        {
            Sprite sprite = _DropSprites[_SplashRandom.Next(_DropSprites.Length)];
            float size    = Mathf.Lerp(_DropSize.x, _DropSize.y, (float)_SplashRandom.NextDouble());
            Image drop    = CreateBlood("Drop", _SplashesRoot, sprite, size);
            float angle   = (float)_SplashRandom.NextDouble() * Mathf.PI * 2f;
            float dist    = Mathf.Lerp(_DropDistance.x, _DropDistance.y, (float)_SplashRandom.NextDouble());
            float delay   = (float)_SplashRandom.NextDouble() * 0.06f;
            float time    = _SplashDuration * Mathf.Lerp(1.1f, 1.8f, (float)_SplashRandom.NextDouble());

            drop.rectTransform.anchoredPosition = center;
            drop.rectTransform.localScale       = Vector3.one * 0.3f;
            drop.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle * Mathf.Rad2Deg);

            DOTween.Sequence().SetUpdate(true).SetTarget(drop).SetDelay(delay)
                .Join(drop.rectTransform.DOAnchorPos(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist, time).SetEase(Ease.OutCubic))
                .Join(drop.rectTransform.DOScale(1f, time * 0.5f).SetEase(Ease.OutQuad));
            _Splashes.Add(drop);
        }

        /// <summary>
        /// Removes the sacrifice splashes (fades them out unless <paramref name="instant"/>).
        /// </summary>
        public void ClearSplashes(bool instant = false)
        {
            bool fade = !instant && isActiveAndEnabled;

            foreach (Image splash in _Splashes) {
                if (splash == null)
                    continue;
                DOTween.Kill(splash);
                splash.rectTransform.DOKill();

                if (!fade) {
                    Destroy(splash.gameObject);

                    continue;
                }
                Image target = splash;

                target.DOFade(0f, 0.35f).SetUpdate(true).OnComplete(() => {
                    if (target != null)
                        Destroy(target.gameObject);
                });
            }
            _Splashes.Clear();
        }

        #endregion

        #region Helpers

        private void LoadSprites()
        {
            if (_SplashSprites.Length > 0 && _DropSprites.Length > 0)
                return;
            Sprite[] sprites = Resources.LoadAll<Sprite>(_ResourcesFolder);
            List<Sprite> splashes = new();
            List<Sprite> drops = new();

            foreach (Sprite sprite in sprites) {
                if (sprite.name.StartsWith("Blood_Splash", StringComparison.OrdinalIgnoreCase))
                    splashes.Add(sprite);
                else if (sprite.name.StartsWith("Blood_Drop", StringComparison.OrdinalIgnoreCase))
                    drops.Add(sprite);
            }
            splashes.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            drops.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            if (_SplashSprites.Length == 0)
                _SplashSprites = splashes.ToArray();
            if (_DropSprites.Length == 0)
                _DropSprites = drops.ToArray();
            if (_SplashSprites.Length == 0)
                Debug.LogWarning($"[PUI_PlayMatBlood] No Blood_Splash sprite found in Resources/{_ResourcesFolder}.", this);
        }

        private RectTransform CreateLayer(string layerName)
        {
            GameObject layer = new GameObject(layerName, typeof(RectTransform));
            RectTransform rect = (RectTransform)layer.transform;

            layer.layer = gameObject.layer;
            rect.SetParent(_Container, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return rect;
        }

        private Image CreateBlood(string objectName, RectTransform parent, Sprite sprite, float size)
        {
            GameObject blood = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Image image = blood.GetComponent<Image>();

            blood.layer = gameObject.layer;
            image.rectTransform.SetParent(parent, false);
            image.rectTransform.sizeDelta = new Vector2(size, size);
            image.sprite         = sprite;
            image.color          = _Color;
            image.preserveAspect = true;
            image.raycastTarget  = false;

            return image;
        }

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;

            color.a     = alpha;
            image.color = color;
        }

        #endregion
    }
}
