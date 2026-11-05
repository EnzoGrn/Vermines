using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;

namespace Vermines.UI.GameTable {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Core.Player;
    using Vermines.UI.Utils;
    using Vermines.Player;

    /// <summary>
    /// Play mat of the local player on the game table: shows the mat of the player's family
    /// (Sprites/Card/{Family}/PlayMat), greys the table out while it is not the player's turn
    /// and feeds the blood layer with the player's souls.
    /// </summary>
    /// <remarks>
    /// The networked statistics are polled every frame while the table is open, so the mat follows every change
    /// whatever its origin (family chosen late, turn ended by the timer, tutorial bot...).
    /// </remarks>
    public class PUI_PlayMat : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Image showing the play mat.
        /// </summary>
        [Header("References")]
        [SerializeField]
        private Image _Mat;

        /// <summary>
        /// Optional images that copy the mat sprite (the mask of the blood layer, so the blood follows the torn edges).
        /// </summary>
        [SerializeField]
        private Image[] _MatCopies = Array.Empty<Image>();

        /// <summary>
        /// Optional blood layer drawn on the mat, under the cards.
        /// </summary>
        [SerializeField]
        private PUI_PlayMatBlood _Blood;

        /// <summary>
        /// Name of the mat sprite in the family folder (Sprites/Card/{Family}/).
        /// </summary>
        [Header("Family")]
        [SerializeField]
        private string _SpriteName = "PlayMat";

        /// <summary>
        /// Mat shown while the family is unknown (the sprite set in the scene is kept when empty).
        /// </summary>
        [SerializeField]
        private Sprite _DefaultSprite;

        /// <summary>
        /// Greys the table out while it is not the local player's turn (off by default: the table keeps its colors and an action
        /// tried out of turn is explained by a message instead, see CardDropHandler).
        /// </summary>
        [Header("Turn lock")]
        [SerializeField]
        private bool _GreyOutWhenLocked = false;

        /// <summary>
        /// Areas faded out and blocked while it is not the local player's turn (cards on the table, discard pile...).
        /// A CanvasGroup is added to them when they have none. Only used by <see cref="_GreyOutWhenLocked"/>.
        /// </summary>
        [SerializeField]
        private RectTransform[] _LockedAreas = Array.Empty<RectTransform>();

        /// <summary>
        /// Color of the mat while the table is locked.
        /// </summary>
        [SerializeField]
        private Color _LockedTint = new Color(0.55f, 0.55f, 0.55f, 1f);

        /// <summary>
        /// Alpha of the locked areas while the table is locked.
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        private float _LockedAlpha = 0.6f;

        /// <summary>
        /// The locked areas ignore the pointer while the table is locked (no drop, no click).
        /// </summary>
        [SerializeField]
        private bool _BlockRaycastsWhenLocked = true;

        /// <summary>
        /// Duration of the lock / unlock fade.
        /// </summary>
        [SerializeField]
        private float _LockDuration = 0.3f;

        /// <summary>
        /// Souls needed to win, used when no gameplay mode is available.
        /// </summary>
        [Header("Fallback limits")]
        [SerializeField]
        private int _DefaultSoulsLimit = 100;

        /// <summary>
        /// Raised when the table gets locked (true) or unlocked (false).
        /// </summary>
        public event Action<bool> LockChanged;

        /// <summary>
        /// True while it is not the local player's turn.
        /// </summary>
        public bool IsLocked => _Locked;

        /// <summary>
        /// Blood layer of the mat (may be null).
        /// </summary>
        public PUI_PlayMatBlood Blood => _Blood;

        private CardFamily _ShownFamily = CardFamily.Count;

        private CanvasGroup[] _LockedGroups;

        private Color _UnlockedTint = Color.white;

        private bool _Locked;

        private bool _LockKnown;

        private Tween _LockTween;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_Mat == null)
                _Mat = GetComponent<Image>();
            if (_Mat != null) {
                _UnlockedTint = _Mat.color;

                if (_DefaultSprite == null)
                    _DefaultSprite = _Mat.sprite;
            }
            _LockedGroups = new CanvasGroup[_LockedAreas.Length];

            for (int i = 0; i < _LockedAreas.Length; i++) {
                if (_LockedAreas[i] == null)
                    continue;
                _LockedGroups[i] = _LockedAreas[i].GetComponent<CanvasGroup>();

                if (_LockedGroups[i] == null)
                    _LockedGroups[i] = _LockedAreas[i].gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            _ShownFamily = CardFamily.Count;
            _LockKnown   = false;

            Refresh(true);
        }

        private void OnDisable()
        {
            _LockTween?.Kill();
            _LockTween = null;
        }

        private void Update()
        {
            Refresh(false);
        }

        #endregion

        #region Refresh

        private void Refresh(bool instant)
        {
            PlayerController local = PlayerController.Local;

            if (!IsAlive(local)) {
                SetLocked(false, instant);

                return;
            }
            PlayerStatistics stats = local.Statistics;

            if (stats.Family != _ShownFamily)
                SetFamily(stats.Family);
            SetLocked(!IsMyTurn(local), instant);

            if (_Blood != null)
                _Blood.SetSouls(stats.Souls, SoulsLimit(local), (int)stats.Family * 7919 + local.Object.InputAuthority.PlayerId);
        }

        /// <summary>
        /// Shows the mat of <paramref name="family"/>.
        /// </summary>
        public void SetFamily(CardFamily family)
        {
            _ShownFamily = family;

            Sprite sprite = GetPlayMatSprite(family, _SpriteName);

            if (sprite == null)
                sprite = _DefaultSprite;
            if (sprite == null)
                return;
            if (_Mat != null)
                _Mat.sprite = sprite;
            foreach (Image copy in _MatCopies) {
                if (copy != null)
                    copy.sprite = sprite;
            }
        }

        /// <summary>
        /// Play mat of <paramref name="family"/> (Sprites/Card/{Family}/PlayMat), null when the family has none.
        /// </summary>
        public static Sprite GetPlayMatSprite(CardFamily family, string spriteName = "PlayMat")
        {
            if (family == CardFamily.None || family == CardFamily.Count)
                return null;
            return UISpriteLoader.GetDefaultSprite(CardType.Partisan, family, spriteName);
        }

        #endregion

        #region Turn lock

        /// <summary>
        /// Greys the table out and blocks it (<paramref name="locked"/> = true), or gives it back.
        /// </summary>
        public void SetLocked(bool locked, bool instant = false)
        {
            if (_LockKnown && _Locked == locked)
                return;
            _Locked    = locked;
            _LockKnown = true;

            _LockTween?.Kill();

            bool greyed = locked && _GreyOutWhenLocked;
            Color tint  = greyed ? _LockedTint : _UnlockedTint;
            float alpha = greyed ? _LockedAlpha : 1f;

            foreach (CanvasGroup group in _LockedGroups) {
                if (group == null)
                    continue;
                group.interactable   = !greyed;
                group.blocksRaycasts = !greyed || !_BlockRaycastsWhenLocked;
            }

            if (instant || _LockDuration <= 0f) {
                if (_Mat != null)
                    _Mat.color = tint;
                foreach (CanvasGroup group in _LockedGroups) {
                    if (group != null)
                        group.alpha = alpha;
                }
            } else {
                Sequence sequence = DOTween.Sequence().SetUpdate(true);

                if (_Mat != null)
                    sequence.Join(_Mat.DOColor(tint, _LockDuration));
                foreach (CanvasGroup group in _LockedGroups) {
                    if (group != null)
                        sequence.Join(group.DOFade(alpha, _LockDuration));
                }
                _LockTween = sequence;
            }

            LockChanged?.Invoke(locked);
        }

        #endregion

        #region Blood

        /// <summary>
        /// Splashes blood on the mat where <paramref name="target"/> stands (the slot of the sacrificed partisan).
        /// </summary>
        public void PlaySacrificeSplash(RectTransform target)
        {
            if (_Blood != null)
                _Blood.PlaySplash(target);
        }

        /// <summary>
        /// Called when the table closes: the fresh splashes disappear.
        /// </summary>
        public void OnTableClosed()
        {
            if (_Blood != null)
                _Blood.ClearSplashes();
        }

        #endregion

        #region Helpers

        private bool IsMyTurn(PlayerController player)
        {
            if (player.Context == null || player.Context.GameplayMode == null)
                return true;
            return player.Context.GameplayMode.IsMyTurn;
        }

        private int SoulsLimit(PlayerController player)
        {
            return player.Context != null && player.Context.GameplayMode != null ? player.Context.GameplayMode.SoulsLimit : _DefaultSoulsLimit;
        }

        private static bool IsAlive(PlayerController player)
        {
            return player != null && player.Object != null && player.Object.IsValid;
        }

        #endregion
    }
}
