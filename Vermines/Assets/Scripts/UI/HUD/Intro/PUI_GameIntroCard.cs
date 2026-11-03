using System.Text;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

namespace Vermines.UI {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Characters;

    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Column of one player in the game intro: cultist in full figure on a glow of the family color,
    /// nickname on the scroll of the family, then the god banner, drawn face down and flipped by
    /// <see cref="FlipGod"/> to show its name and effects.
    /// </summary>
    /// <remarks>
    /// The cultist comes from the family folder (<see cref="PUI_PlayerBanner.GetCultistSprite"/>), the god from
    /// <see cref="God.BannerIcon"/> (masked inside a blank banner by the prefab), the scroll is the player banner
    /// scroll tinted with the family color. The texts come from the <see cref="PUI_GameIntro.TableName"/> table.
    /// Every reference is optional, <see cref="PUI_GameIntro"/> moves the column itself.
    /// </remarks>
    public class PUI_GameIntroCard : MonoBehaviour {

        #region Constants

        /// <summary>
        /// Folder of the intro art in a Resources folder.
        /// </summary>
        public const string ArtPath = "Sprites/UI/Intro/";

        /// <summary>
        /// Key of the tag shown under the local player.
        /// </summary>
        public const string LocalKey = "intro.local";

        /// <summary>
        /// Key of the first player badge.
        /// </summary>
        public const string FirstKey = "intro.first";

        /// <summary>
        /// Key of the text shown when the god has no effect.
        /// </summary>
        public const string NoEffectKey = "intro.no_effect";

        #endregion

        #region Attributes

        /// <summary>
        /// Fades the whole column.
        /// </summary>
        [Header("Player")]
        [SerializeField]
        private CanvasGroup _CanvasGroup;

        /// <summary>
        /// Glow behind the cultist, tinted with the family color.
        /// </summary>
        [SerializeField]
        private Image _FamilyGlow;

        /// <summary>
        /// Cultist in full figure.
        /// </summary>
        [SerializeField]
        private Image _Cultist;

        /// <summary>
        /// Scroll of the family under the nickname.
        /// </summary>
        [SerializeField]
        private Image _NameScroll;

        /// <summary>
        /// Nickname of the player.
        /// </summary>
        [SerializeField]
        private Text _Nickname;

        /// <summary>
        /// Shown on the column of the local player only.
        /// </summary>
        [SerializeField]
        private GameObject _LocalTag;

        /// <summary>
        /// Part flipped to reveal the god.
        /// </summary>
        [Header("God")]
        [SerializeField]
        private RectTransform _GodFlip;

        /// <summary>
        /// Back of the god banner, shown before the flip.
        /// </summary>
        [SerializeField]
        private GameObject _GodBack;

        /// <summary>
        /// Face of the god banner, shown after the flip.
        /// </summary>
        [SerializeField]
        private GameObject _GodFront;

        /// <summary>
        /// Banner of the god.
        /// </summary>
        [SerializeField]
        private Image _GodImage;

        /// <summary>
        /// Flash lit when the god is revealed.
        /// </summary>
        [SerializeField]
        private Image _GodFlash;

        /// <summary>
        /// Name of the god.
        /// </summary>
        [SerializeField]
        private Text _GodName;

        /// <summary>
        /// Effects of the god.
        /// </summary>
        [SerializeField]
        private Text _GodEffects;

        /// <summary>
        /// Fades the god name and effects in after the flip.
        /// </summary>
        [SerializeField]
        private CanvasGroup _GodTexts;

        /// <summary>
        /// Badge of the player who plays first.
        /// </summary>
        [Header("First player")]
        [SerializeField]
        private RectTransform _FirstBadge;

        /// <summary>
        /// Fades <see cref="_FirstBadge"/>.
        /// </summary>
        [SerializeField]
        private CanvasGroup _FirstBadgeGroup;

        /// <summary>
        /// Golden glow lit behind the column of the first player.
        /// </summary>
        [SerializeField]
        private Image _FirstGlow;

        /// <summary>
        /// Glow color while the family is unknown (the family colors come from the cultist database).
        /// </summary>
        [Header("Family")]
        [SerializeField]
        private Color _DefaultColor = new(0.6f, 0.1f, 0.1f, 1f);

        /// <summary>
        /// Alpha of the family glow.
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        private float _FamilyGlowAlpha = 0.6f;

        /// <summary>
        /// Height of the column lit by the first player glow when the player has no god (cultist, scroll and tag).
        /// </summary>
        [SerializeField]
        private float _NoGodGlowHeight = 580f;

        /// <summary>
        /// Strength of the family color filter on the scroll (0 keeps the parchment, 1 multiplies by the full color).
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        private float _ScrollTint = 0.75f;

        /// <summary>
        /// Written before each god effect.
        /// </summary>
        [Header("Texts")]
        [SerializeField]
        private string _EffectPrefix = string.Empty;

        /// <summary>
        /// Shown when the god has no effect.
        /// </summary>
        [SerializeField]
        private string _NoEffectText = "intro.no_effect";

        /// <summary>
        /// The player shown by the column.
        /// </summary>
        public GameIntroPlayer Player => _Player;

        private GameIntroPlayer _Player;

        /// <summary>
        /// False when the player has no god: its banner and effects stay hidden.
        /// </summary>
        private bool _HasGod;

        /// <summary>
        /// Bottom offset of <see cref="_FirstGlow"/> in the prefab, restored when the player has a god.
        /// </summary>
        private float _FirstGlowBottom;

        private bool _HasFirstGlowBottom;

        #endregion

        #region Setup

        /// <summary>
        /// Shows <paramref name="player"/>, god face down and first player badge hidden.
        /// </summary>
        public void Setup(GameIntroPlayer player)
        {
            _Player = player;

            LocalizeTexts();
            SetFamily(player.Family);
            SetGod(player.God);

            if (_Nickname != null)
                _Nickname.text = string.IsNullOrEmpty(player.Nickname) ? "???" : player.Nickname;
            if (_LocalTag != null)
                _LocalTag.SetActive(player.IsLocal);
            ResetState();
        }

        /// <summary>
        /// Back to the state before the animation: god face down, texts, flash and badge hidden.
        /// </summary>
        public void ResetState()
        {
            if (_CanvasGroup != null)
                _CanvasGroup.alpha = 1f;
            if (_GodFlip != null)
                _GodFlip.localScale = Vector3.one;
            ShowGodFace(false);

            if (_GodTexts != null)
                _GodTexts.alpha = 0f;
            if (_GodFlash != null)
                _GodFlash.color = WithAlpha(_GodFlash.color, 0f);
            if (_FirstBadge != null)
                _FirstBadge.localScale = Vector3.zero;
            if (_FirstBadgeGroup != null)
                _FirstBadgeGroup.alpha = 0f;
            if (_FirstGlow != null)
                _FirstGlow.color = WithAlpha(_FirstGlow.color, 0f);
        }

        private void SetFamily(CardFamily family)
        {
            bool known = family != CardFamily.None && family != CardFamily.Count;

            if (_FamilyGlow != null)
                _FamilyGlow.color = WithAlpha(GetFamilyColor(family), _FamilyGlowAlpha);
            if (_Cultist != null) {
                Sprite cultist = known ? PUI_PlayerBanner.GetCultistSprite(family) : null;

                _Cultist.enabled = cultist != null;

                if (cultist != null)
                    _Cultist.sprite = cultist;
            }
            if (_NameScroll != null)
                _NameScroll.color = Color.Lerp(Color.white, GetFamilyColor(family), known ? _ScrollTint : 0f);
        }

        /// <summary>
        /// Shows <paramref name="god"/> face down. Without god, the banner, the flash and the effects are hidden.
        /// </summary>
        /// <remarks>
        /// A negative ID is the blank god of the tutorial (no power, no art): it counts as no god.
        /// </remarks>
        private void SetGod(God god)
        {
            _HasGod = god != null && god.ID >= 0;

            if (_GodFlip != null)
                _GodFlip.gameObject.SetActive(_HasGod);
            if (_GodTexts != null)
                _GodTexts.gameObject.SetActive(_HasGod);
            FitFirstGlow();

            if (!_HasGod)
                return;
            if (_GodImage != null) {
                Sprite divinity = god != null ? god.BannerIcon : null;

                _GodImage.enabled = divinity != null;

                if (divinity != null)
                    _GodImage.sprite = divinity;
            }

            if (_GodName != null)
                _GodName.text = god.Name;
            if (_GodEffects != null)
                _GodEffects.text = BuildEffectsText(god);
        }

        #endregion

        #region Animations

        /// <summary>
        /// Flips the god banner face up with a flash, then fades its name and effects in.
        /// </summary>
        public Sequence FlipGod(float duration)
        {
            Sequence sequence = DOTween.Sequence();
            float half = duration * 0.5f;

            // Nothing to reveal without god.
            if (!_HasGod)
                return sequence;

            if (_GodFlip != null) {
                sequence.Append(_GodFlip.DOScaleX(0f, half).SetEase(Ease.InQuad));
                sequence.AppendCallback(() => ShowGodFace(true));
                sequence.Append(_GodFlip.DOScaleX(1f, half).SetEase(Ease.OutBack));
                sequence.Append(_GodFlip.DOPunchScale(Vector3.one * 0.08f, 0.35f, 6, 0.6f));
            } else {
                sequence.AppendCallback(() => ShowGodFace(true));
            }
            if (_GodFlash != null) {
                sequence.Insert(half, _GodFlash.DOFade(1f, 0.08f));
                sequence.Insert(half + 0.08f, _GodFlash.DOFade(0f, 0.6f).SetEase(Ease.OutQuad));
            }
            if (_GodTexts != null)
                sequence.Insert(duration * 0.8f, _GodTexts.DOFade(1f, 0.4f));
            return sequence;
        }

        /// <summary>
        /// Drops the first player badge and lights the column.
        /// </summary>
        public Sequence RevealFirst(float duration)
        {
            Sequence sequence = DOTween.Sequence();

            if (_FirstBadge != null) {
                Vector2 position = _FirstBadge.anchoredPosition;

                _FirstBadge.localScale       = Vector3.one;
                _FirstBadge.anchoredPosition = position + Vector2.up * 120f;

                sequence.Join(_FirstBadge.DOAnchorPos(position, duration).SetEase(Ease.OutBounce));
            }
            if (_FirstBadgeGroup != null)
                sequence.Join(_FirstBadgeGroup.DOFade(1f, duration * 0.4f));
            if (_FirstGlow != null) {
                sequence.Join(_FirstGlow.DOFade(1f, duration));
                sequence.Append(_FirstGlow.DOFade(0.55f, 0.6f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));
            }
            sequence.Insert(0f, transform.DOPunchScale(Vector3.one * 0.05f, duration, 5, 0.5f));

            return sequence;
        }

        /// <summary>
        /// Pushes the column back (used for the players who do not start).
        /// </summary>
        public Tween Dim(float alpha, float duration)
        {
            Sequence sequence = DOTween.Sequence();

            if (_CanvasGroup != null)
                sequence.Join(_CanvasGroup.DOFade(alpha, duration));
            sequence.Join(transform.DOScale(transform.localScale * 0.96f, duration));

            return sequence;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Without god, the golden glow of the first player only lights the cultist and the nickname, not the empty
        /// place of the god (it stops <see cref="_NoGodGlowHeight"/> under the top of the column).
        /// </summary>
        private void FitFirstGlow()
        {
            if (_FirstGlow == null)
                return;
            RectTransform glow = _FirstGlow.rectTransform;

            if (!_HasFirstGlowBottom) {
                _HasFirstGlowBottom = true;
                _FirstGlowBottom    = glow.offsetMin.y;
            }
            float bottom = _FirstGlowBottom;

            if (!_HasGod)
                bottom += Mathf.Max(0f, ((RectTransform)transform).rect.height - _NoGodGlowHeight);
            glow.offsetMin = new Vector2(glow.offsetMin.x, bottom);
        }

        private void ShowGodFace(bool front)
        {
            if (_GodBack != null)
                _GodBack.SetActive(!front);
            if (_GodFront != null)
                _GodFront.SetActive(front);
        }

        private string BuildEffectsText(God god)
        {
            if (god == null)
                return string.Empty;
            StringBuilder builder = new();

            if (god.Effects != null) {
                foreach (AEffect effect in god.Effects) {
                    if (effect == null || string.IsNullOrWhiteSpace(effect.Description))
                        continue;
                    if (builder.Length > 0)
                        builder.Append('\n');
                    builder.Append(_EffectPrefix).Append(effect.Description.Trim());
                }
            }

            return builder.Length > 0 ? builder.ToString() : PUI_GameIntro.Localize(NoEffectKey, _NoEffectText);
        }

        /// <summary>
        /// Glow color of <paramref name="family"/>, read from the cultist database.
        /// </summary>
        public Color GetFamilyColor(CardFamily family)
        {
            return PUI_PlayerBanner.TryGetCultistColor(family, out Color color) ? color : _DefaultColor;
        }

        /// <summary>
        /// Writes the local tag and the first player badge in the current language.
        /// </summary>
        private void LocalizeTexts()
        {
            Text localTag = _LocalTag != null ? _LocalTag.GetComponentInChildren<Text>(true) : null;
            Text badge = _FirstBadge != null ? _FirstBadge.GetComponentInChildren<Text>(true) : null;

            if (localTag != null)
                localTag.text = PUI_GameIntro.Localize(LocalKey, localTag.text);
            if (badge != null)
                badge.text = PUI_GameIntro.Localize(FirstKey, badge.text);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;

            return color;
        }

        #endregion
    }
}
