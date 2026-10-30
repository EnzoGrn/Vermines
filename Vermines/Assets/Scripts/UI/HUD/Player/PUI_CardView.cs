using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

namespace Vermines.UI {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;

    /// <summary>
    /// Player UI card view: one small card back shown in the <see cref="PUI_MiniDeckDrawer"/>.
    /// It only displays the type of the card (partisan, tool...), never its content.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class PUI_CardView : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Sprite of each card type.
        /// </summary>
        public CardSpriteDatabase CSDB;

        /// <summary>
        /// The card displayed by this view.
        /// </summary>
        public ICard Card { get; private set; }

        /// <summary>
        /// The type of the displayed card, <see cref="CardType.None"/> when no card is bound.
        /// </summary>
        public CardType Type => Card?.Data != null ? Card.Data.Type : CardType.None;

        /// <summary>
        /// Rect transform of the view, cached.
        /// </summary>
        public RectTransform Rect { get; private set; }

        /// <summary>
        /// Canvas group of the view, cached (used for the fades).
        /// </summary>
        public CanvasGroup Group { get; private set; }

        /// <summary>
        /// Dotted card shown while a new card is arriving.
        /// </summary>
        [SerializeField]
        private Sprite _DottedCard;

        /// <summary>
        /// Card shown when the card leaves the player's cards without going to the table (recycled...).
        /// </summary>
        [SerializeField]
        private Sprite _RecycledCard;

        /// <summary>
        /// Card shown when the card is sacrificed. Falls back on <see cref="_RecycledCard"/> when empty.
        /// </summary>
        [SerializeField]
        private Sprite _SacrificedCard;

        /// <summary>
        /// Image that displays the card sprite.
        /// </summary>
        [SerializeField]
        private Image _Background;

        /// <summary>
        /// Current slide tween, killed when a new position is requested.
        /// </summary>
        private Tween _MoveTween;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Rect  = GetComponent<RectTransform>();
            Group = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            _MoveTween?.Kill();

            DOTween.Kill(this);
        }

        #endregion

        #region Methods

        public void Bind(ICard card)
        {
            Card = card;

            SetGhost(false);
        }

        public void SetGhost(bool value)
        {
            if (value)
                _Background.sprite = _DottedCard;
            else
                _Background.sprite = CSDB != null ? CSDB.Get(Type) : null;
        }

        public void SetRecycled()
        {
            if (_RecycledCard != null)
                _Background.sprite = _RecycledCard;
        }

        public void SetSacrificed()
        {
            if (_SacrificedCard != null)
                _Background.sprite = _SacrificedCard;
            else
                SetRecycled();
        }

        /// <summary>
        /// Slides the card to its slot in the drawer.
        /// </summary>
        /// <param name="position">Anchored position of the slot.</param>
        /// <param name="duration">Duration of the slide, 0 to snap.</param>
        /// <param name="delay">Delay before the slide starts.</param>
        public void MoveTo(Vector2 position, float duration, float delay = 0f)
        {
            _MoveTween?.Kill();
            _MoveTween = null;

            if (duration <= 0f) {
                Rect.anchoredPosition = position;

                return;
            }

            if ((Rect.anchoredPosition - position).sqrMagnitude < 0.01f)
                return;
            _MoveTween = Rect.DOAnchorPos(position, duration).SetDelay(delay).SetEase(Ease.OutCubic).SetLink(gameObject);
        }

        /// <summary>
        /// Stops the slide tween, used when the card starts leaving.
        /// </summary>
        public void StopMove()
        {
            _MoveTween?.Kill();
            _MoveTween = null;
        }

        #endregion
    }
}
