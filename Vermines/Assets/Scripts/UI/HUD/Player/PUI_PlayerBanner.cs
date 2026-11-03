using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;

namespace Vermines.UI {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Core.Player;
    using Vermines.Characters;
    using Vermines.UI.Utils;
    using Vermines.Player;
    using Vermines.Core;

    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Banner of one player: cultist (and family background), nickname, strip tinted with the insect family,
    /// mini deck drawer, eloquence and souls gauges. The banner slides to the right during the player's turn.
    /// </summary>
    /// <remarks>
    /// Shows the player given to <see cref="Bind"/>; see <see cref="PUI_LocalPlayerBanner"/> for the local one.
    /// The networked statistics are polled every frame, so the banner follows every change whatever its origin.
    /// </remarks>
    public class PUI_PlayerBanner : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Banner image tinted with the family color.
        /// </summary>
        [Header("References")]
        [SerializeField]
        private Image _Banner;

        /// <summary>
        /// Cultist portrait.
        /// </summary>
        [SerializeField]
        private Image _Cultist;

        /// <summary>
        /// Optional family background drawn behind the cultist (remote player banners).
        /// </summary>
        [SerializeField]
        private Image _FamilyBackground;

        /// <summary>
        /// Optional fitter of <see cref="_FamilyBackground"/>, its aspect ratio follows the sprite.
        /// </summary>
        [SerializeField]
        private AspectRatioFitter _FamilyBackgroundFitter;

        /// <summary>
        /// Nickname of the player.
        /// </summary>
        [SerializeField]
        private Text _Username;

        /// <summary>
        /// Mini deck drawer of the player.
        /// </summary>
        [SerializeField]
        private PUI_MiniDeckDrawer _DeckDrawer;

        /// <summary>
        /// Eloquence gauge.
        /// </summary>
        [SerializeField]
        private PUI_IconGauge _Eloquence;

        /// <summary>
        /// Souls gauge.
        /// </summary>
        [SerializeField]
        private PUI_IconGauge _Souls;

        /// <summary>
        /// Banner color while the family is unknown (the family colors come from the cultist database).
        /// </summary>
        [Header("Family")]
        [SerializeField]
        private Color _DefaultColor = Color.white;

        /// <summary>
        /// Part of the banner that slides with all its content. Its position in the prefab is the one of the
        /// player's turn: out of the turn it waits <see cref="_TurnOffset"/> pixels further left.
        /// </summary>
        [Header("Turn")]
        [SerializeField, FormerlySerializedAs("_TurnStrip")]
        private RectTransform _TurnSlide;

        /// <summary>
        /// Distance the banner moves to the right when the turn starts.
        /// </summary>
        [SerializeField, FormerlySerializedAs("_TurnExtraWidth")]
        private float _TurnOffset = 60f;

        /// <summary>
        /// Duration of the slide animation.
        /// </summary>
        [SerializeField]
        private float _TurnDuration = 0.45f;

        /// <summary>
        /// Optional element punched when the turn starts (the cultist pennant...).
        /// </summary>
        [SerializeField]
        private RectTransform _TurnPunch;

        /// <summary>
        /// Follows <c>GameEvents.OnTurnChanged</c> to know when the turn of the bound player starts.
        /// </summary>
        [SerializeField]
        private bool _FollowTurns = true;

        /// <summary>
        /// Maximum eloquence used when no gameplay mode is available.
        /// </summary>
        [Header("Fallback limits")]
        [SerializeField]
        private int _DefaultMaxEloquence = 20;

        /// <summary>
        /// Souls needed to win, used when no gameplay mode is available.
        /// </summary>
        [SerializeField]
        private int _DefaultSoulsLimit = 100;

        /// <summary>
        /// The player shown by the banner.
        /// </summary>
        public PlayerController Player => _Player;

        private PlayerController _Player;

        private CardFamily _ShownFamily = CardFamily.Count;

        private string _ShownNickname;

        private bool _FirstValues;

        private bool _HasSlideOrigin;

        private float _SlideTurnX;

        /// <summary>
        /// True while it is the turn of the shown player.
        /// </summary>
        public bool IsTurnActive { get; private set; }

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            CacheSlideOrigin();

            // Out of the turn by default.
            SetTurnActive(false, true);
        }

        protected virtual void OnEnable()
        {
            if (_FollowTurns)
                GameEvents.OnTurnChanged.AddListener(OnTurnChanged);
        }

        protected virtual void OnDisable()
        {
            GameEvents.OnTurnChanged.RemoveListener(OnTurnChanged);
        }

        protected virtual void LateUpdate()
        {
            if (!IsAlive(_Player)) {
                PlayerController target = GetAutoBindTarget();

                if (IsAlive(target))
                    Bind(target);
                return;
            }
            Refresh(false);
        }

        #endregion

        /// <summary>
        /// Player bound automatically when none is shown (none by default).
        /// </summary>
        protected virtual PlayerController GetAutoBindTarget()
        {
            return null;
        }

        #region Binding

        /// <summary>
        /// Shows <paramref name="player"/>.
        /// </summary>
        public void Bind(PlayerController player)
        {
            _Player        = player;
            _ShownFamily   = CardFamily.Count;
            _ShownNickname = null;
            _FirstValues   = true;

            if (_DeckDrawer != null)
                _DeckDrawer.Bind(player);
            if (IsAlive(player))
                Refresh(true);
        }

        private void Refresh(bool instant)
        {
            PlayerStatistics stats = _Player.Statistics;

            if (stats.Family != _ShownFamily)
                SetFamily(stats.Family);
            string nickname = _Player.NetworkedNickname.Value;

            if (nickname != _ShownNickname)
                SetNickname(nickname);
            SetStatistics(stats.Eloquence, MaxEloquence(), stats.Souls, SoulsLimit(), instant || _FirstValues);

            _FirstValues = false;
        }

        #endregion

        #region Display

        /// <summary>
        /// Tints the banner and shows the cultist of <paramref name="family"/>.
        /// </summary>
        public void SetFamily(CardFamily family)
        {
            _ShownFamily = family;

            if (_Banner != null)
                _Banner.color = GetFamilyColor(family);
            if (_FamilyBackground != null) {
                Sprite background = GetFamilySprite(family, "Background");

                _FamilyBackground.enabled = background != null;

                if (background != null) {
                    _FamilyBackground.sprite = background;

                    if (_FamilyBackgroundFitter != null)
                        _FamilyBackgroundFitter.aspectRatio = background.rect.width / background.rect.height;
                }
            }
            if (_Cultist == null)
                return;
            Sprite cultist = GetCultistSprite(family);

            _Cultist.enabled = cultist != null;

            if (cultist != null)
                _Cultist.sprite = cultist;
        }

        /// <summary>
        /// Shows <paramref name="nickname"/>.
        /// </summary>
        public void SetNickname(string nickname)
        {
            _ShownNickname = nickname;

            if (_Username != null)
                _Username.text = nickname;
        }

        /// <summary>
        /// Updates both gauges.
        /// </summary>
        public void SetStatistics(int eloquence, int maxEloquence, int souls, int soulsLimit, bool instant = false)
        {
            if (_Eloquence != null)
                _Eloquence.SetValue(eloquence, maxEloquence, instant);
            if (_Souls != null)
                _Souls.SetValue(souls, soulsLimit, instant);
        }

        /// <summary>
        /// Slides the banner to the right when it is the player's turn, back to the left otherwise.
        /// </summary>
        public void SetTurnActive(bool active, bool instant = false)
        {
            if (active == IsTurnActive && !instant)
                return;
            IsTurnActive = active;

            if (_TurnSlide == null)
                return;
            CacheSlideOrigin();

            float target = _SlideTurnX - (active ? 0f : _TurnOffset);

            _TurnSlide.DOKill();

            if (instant || !isActiveAndEnabled) {
                _TurnSlide.anchoredPosition = new Vector2(target, _TurnSlide.anchoredPosition.y);

                return;
            }
            _TurnSlide.DOAnchorPosX(target, _TurnDuration)
                .SetEase(active ? Ease.OutBack : Ease.InOutCubic)
                .SetTarget(_TurnSlide)
                .SetLink(gameObject);

            if (active && _TurnPunch != null) {
                _TurnPunch.DOKill(true);
                _TurnPunch.DOPunchScale(Vector3.one * 0.08f, 0.4f, 6, 0.6f).SetLink(_TurnPunch.gameObject);
            }
        }

        /// <summary>
        /// Banner color of <paramref name="family"/>, read from the cultist database.
        /// </summary>
        public Color GetFamilyColor(CardFamily family)
        {
            return TryGetCultistColor(family, out Color color) ? color : _DefaultColor;
        }

        #endregion

        #region Helpers

        private void CacheSlideOrigin()
        {
            if (_HasSlideOrigin || _TurnSlide == null)
                return;
            _HasSlideOrigin = true;
            _SlideTurnX     = _TurnSlide.anchoredPosition.x;
        }

        private void OnTurnChanged(int turnIndex)
        {
            if (!IsAlive(_Player) || _Player.Context == null || _Player.Context.GameplayMode == null)
                return;
            var order = _Player.Context.GameplayMode.PlayerTurnOrder;

            if (order.Length == 0)
                return;
            SetTurnActive(order.Get(((turnIndex % order.Length) + order.Length) % order.Length) == _Player.Object.InputAuthority);
        }

        private static Sprite GetFamilySprite(CardFamily family, string spriteName)
        {
            if (family == CardFamily.None || family == CardFamily.Count)
                return null;
            return UISpriteLoader.GetDefaultSprite(CardType.Partisan, family, spriteName);
        }

        /// <summary>
        /// Cultist portrait of <paramref name="family"/> (also used by the game intro).
        /// </summary>
        public static Sprite GetCultistSprite(CardFamily family)
        {
            if (family == CardFamily.None || family == CardFamily.Count)
                return null;
            Cultist cultist = GetCultist(family);

            if (cultist != null && cultist.CultistSprite != null)
                return cultist.CultistSprite;
            return UISpriteLoader.GetDefaultSprite(CardType.Partisan, family, "Cultist");
        }

        /// <summary>
        /// Color of the cultist of <paramref name="family"/> in the cultist database (also used by the game intro).
        /// </summary>
        public static bool TryGetCultistColor(CardFamily family, out Color color)
        {
            Cultist cultist = GetCultist(family);

            color = cultist != null ? cultist.Color : Color.white;

            return cultist != null;
        }

        /// <summary>
        /// Cultist of <paramref name="family"/> in <see cref="Global.Settings"/>, null when unknown.
        /// </summary>
        public static Cultist GetCultist(CardFamily family)
        {
            if (family == CardFamily.None || family == CardFamily.Count)
                return null;
            if (Global.Settings == null || Global.Settings.Cultists == null)
                return null;
            return Global.Settings.Cultists.GetCultistByFamily(family);
        }

        private int MaxEloquence()
        {
            return _Player.Context != null && _Player.Context.GameplayMode != null ? _Player.Context.GameplayMode.MaxEloquence : _DefaultMaxEloquence;
        }

        private int SoulsLimit()
        {
            return _Player.Context != null && _Player.Context.GameplayMode != null ? _Player.Context.GameplayMode.SoulsLimit : _DefaultSoulsLimit;
        }

        private static bool IsAlive(PlayerController player)
        {
            return player != null && player.Object != null && player.Object.IsValid;
        }

        #endregion
    }
}
