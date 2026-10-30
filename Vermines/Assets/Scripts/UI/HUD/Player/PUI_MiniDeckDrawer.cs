using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Vermines.UI {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Shows the size of a player's cards (hand + deck + discard):
    /// one small card back per partisan and per tool, so every player can estimate the deck of the others.
    /// </summary>
    /// <remarks>
    /// The piles are networked on the <see cref="PlayerController"/>,so the drawer polls them every frame and works the same on every client,
    /// for every player.It binds itself to the player of a parent <see cref="PlayerBannerUI"/>, to the local player, or to the player given to <see cref="Bind"/>.
    /// </remarks>
    public class PUI_MiniDeckDrawer : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Prefab of one card.
        /// </summary>
        [SerializeField]
        private PUI_CardView _CardPrefab;

        /// <summary>
        /// Parent of the cards. The cards are placed from its left edge, vertically centered.
        /// </summary>
        [SerializeField]
        private RectTransform _Container;

        /// <summary>
        /// Optional text showing the number of partisans.
        /// </summary>
        [SerializeField]
        private Text _PartisanCount;

        /// <summary>
        /// Optional text showing the number of tools.
        /// </summary>
        [SerializeField]
        private Text _ToolCount;

        /// <summary>
        /// Binds the drawer to the local player when no player is given and no parent banner exists.
        /// </summary>
        [Header("Binding")]
        [SerializeField]
        private bool _BindLocalPlayer = false;

        /// <summary>
        /// Counts the tools discarded this turn (they go back to the discard at the end of the turn).
        /// </summary>
        [SerializeField]
        private bool _IncludeToolDiscard = true;

        /// <summary>
        /// Distance between two cards.
        /// </summary>
        [Header("Layout")]
        [SerializeField]
        private float _Step = 12f;

        /// <summary>
        /// Extra distance between the partisans and the tools.
        /// </summary>
        [SerializeField]
        private float _GroupGap = 10f;

        /// <summary>
        /// Duration of the slide when the cards make room or close a gap.
        /// </summary>
        [Header("Animation")]
        [SerializeField]
        private float _MoveDuration = 0.25f;

        /// <summary>
        /// Time before a new card reveals its type.
        /// </summary>
        [SerializeField]
        private float _RevealDelay = 0.8f;

        /// <summary>
        /// The player whose cards are shown.
        /// </summary>
        public PlayerController Player => _Player;

        private PlayerController _Player;

        private PlayerBannerUI _Banner;

        private Func<ICard, PUI_CardRemoveReason> _ReasonResolver;

        private bool _HasSignature;

        private int _LastSignature;

        private readonly Dictionary<int, PUI_CardView> _Views = new();

        private readonly HashSet<PUI_CardView> _Leaving = new();

        private readonly List<ICard> _Buffer = new();

        private readonly List<Vector2> _Positions = new();

        private readonly HashSet<int> _CurrentIds = new();

        private readonly List<int> _RemovedIds = new();

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (_Container == null)
                _Container = transform as RectTransform;
            _Banner = GetComponentInParent<PlayerBannerUI>(true);
        }

        private void LateUpdate()
        {
            if (_Player == null)
                TryAutoBind();
            if (_Player == null)
                return;
            if (!IsAlive(_Player)) {
                Unbind();

                return;
            }

            GatherCards(_Player, _Buffer);

            int signature = ComputeSignature(_Buffer);

            if (_HasSignature && signature == _LastSignature)
                return;
            Apply(_Buffer, !_HasSignature);

            _HasSignature  = true;
            _LastSignature = signature;
        }

        private void OnDestroy()
        {
            Clear();
        }

        #endregion

        #region Binding

        /// <summary>
        /// Shows the cards of <paramref name="player"/>. The current cards appear without animation.
        /// </summary>
        public void Bind(PlayerController player)
        {
            if (_Player == player)
                return;
            Clear();

            _Player       = player;
            _HasSignature = false;
        }

        /// <summary>
        /// Stops following the player and removes every card.
        /// </summary>
        public void Unbind()
        {
            Clear();

            _Player       = null;
            _HasSignature = false;
        }

        private void TryAutoBind()
        {
            if (_Banner != null) {
                PlayerController bannerPlayer = _Banner.GetPlayer();

                if (IsAlive(bannerPlayer))
                    Bind(bannerPlayer);
                return;
            }

            if (_BindLocalPlayer && IsAlive(PlayerController.Local))
                Bind(PlayerController.Local);
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Shows <paramref name="cards"/> without any player bound (tests, offline previews).
        /// </summary>
        /// <param name="cards">Every card to show.</param>
        /// <param name="instant">True to skip the animations.</param>
        public void Refresh(IEnumerable<ICard> cards, bool instant = false)
        {
            Refresh(cards, null, instant);
        }

        /// <summary>
        /// Shows <paramref name="cards"/> without any player bound, <paramref name="reasonResolver"/> tells
        /// where each removed card went (null: recycled).
        /// </summary>
        public void Refresh(IEnumerable<ICard> cards, Func<ICard, PUI_CardRemoveReason> reasonResolver, bool instant = false)
        {
            _Buffer.Clear();
            _Buffer.AddRange(cards);

            _ReasonResolver = reasonResolver;

            Apply(_Buffer, instant);

            _ReasonResolver = null;
        }

        private void GatherCards(PlayerController player, List<ICard> cards)
        {
            cards.Clear();
            cards.AddRange(player.Hand);
            cards.AddRange(player.Deck);
            cards.AddRange(player.Discard);

            if (_IncludeToolDiscard)
                cards.AddRange(player.ToolDiscard);
        }

        /// <summary>
        /// Order independent hash of the card ids, a reshuffle or a move between two counted piles changes nothing.
        /// </summary>
        private static int ComputeSignature(List<ICard> cards)
        {
            unchecked {
                int sum = 0;
                int xor = 0;

                foreach (ICard card in cards) {
                    if (card == null)
                        continue;
                    int h = card.ID * (int)2654435761u;

                    sum += h;
                    xor ^= h;
                }
                return (sum * 31 + xor) * 31 + cards.Count;
            }
        }

        private void Apply(List<ICard> cards, bool instant)
        {
            List<ICard> sorted = CardSortUtility.SortDeckPerType(cards.FindAll(card => card?.Data != null && IsCounted(card.Data.Type)));

            // Duplicate ids should never happen, but they would break the dictionary.
            _CurrentIds.Clear();
            sorted.RemoveAll(card => !_CurrentIds.Add(card.ID));

            // Cards that left.
            _RemovedIds.Clear();

            foreach (int id in _Views.Keys) {
                if (!_CurrentIds.Contains(id))
                    _RemovedIds.Add(id);
            }

            foreach (int id in _RemovedIds) {
                PUI_CardView view = _Views[id];

                _Views.Remove(id);

                if (instant) {
                    Destroy(view.gameObject);

                    continue;
                }
                _Leaving.Add(view);

                PUI_CardAnimationController.PlayRemove(view, ResolveRemoveReason(view.Card), () => {
                    _Leaving.Remove(view);

                    if (view != null)
                        Destroy(view.gameObject);
                });
            }

            // Layout of the remaining and new cards.
            PUI_CardLayoutSolver.Solve(sorted, _Positions, CardWidth(), _Container.rect.width, _Step, _GroupGap);

            float moveDelay = _RemovedIds.Count > 0 ? 0.25f : 0f;

            for (int i = 0; i < sorted.Count; i++) {
                ICard card = sorted[i];

                if (_Views.TryGetValue(card.ID, out PUI_CardView view)) {
                    view.MoveTo(_Positions[i], instant ? 0f : _MoveDuration, moveDelay);

                    continue;
                }
                view = CreateView(card);

                view.MoveTo(_Positions[i], 0f);

                _Views.Add(card.ID, view);

                if (!instant)
                    PUI_CardAnimationController.PlayAdd(view, _RevealDelay);
            }

            // The leftmost card is drawn on top, the leaving cards above everything.
            for (int i = sorted.Count - 1; i >= 0; i--)
                _Views[sorted[i].ID].transform.SetAsLastSibling();
            foreach (PUI_CardView view in _Leaving) {
                if (view != null)
                    view.transform.SetAsLastSibling();
            }

            UpdateCounters(sorted, instant);
        }

        private PUI_CardView CreateView(ICard card)
        {
            PUI_CardView view = Instantiate(_CardPrefab, _Container);
            RectTransform rt  = view.Rect != null ? view.Rect : (RectTransform)view.transform;

            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot     = new Vector2(0f, 0.5f);

            view.Bind(card);

            return view;
        }

        private PUI_CardRemoveReason ResolveRemoveReason(ICard card)
        {
            if (_ReasonResolver != null && card != null)
                return _ReasonResolver(card);
            if (_Player == null || card == null)
                return PUI_CardRemoveReason.Recycled;
            if (Contains(_Player.Graveyard, card.ID))
                return PUI_CardRemoveReason.Sacrificed;
            if (Contains(_Player.PlayedCards, card.ID) || Contains(_Player.Equipments, card.ID))
                return PUI_CardRemoveReason.ToTable;
            return PUI_CardRemoveReason.Recycled;
        }

        private void UpdateCounters(List<ICard> sorted, bool instant)
        {
            if (_PartisanCount == null && _ToolCount == null)
                return;
            int partisans = 0;
            int tools     = 0;

            foreach (ICard card in sorted) {
                if (card.Data.Type == CardType.Partisan)
                    partisans++;
                else
                    tools++;
            }

            SetCounter(_PartisanCount, partisans, instant);
            SetCounter(_ToolCount, tools, instant);
        }

        private static void SetCounter(Text text, int value, bool instant)
        {
            if (text == null)
                return;
            string str = value.ToString();

            if (text.text == str)
                return;
            text.text = str;

            if (instant)
                return;
            text.rectTransform.DOKill(true);
            text.rectTransform.DOPunchScale(Vector3.one * 0.3f, 0.25f, 6, 0.6f).SetLink(text.gameObject);
        }

        private void Clear()
        {
            foreach (PUI_CardView view in _Views.Values) {
                if (view != null)
                    Destroy(view.gameObject);
            }

            foreach (PUI_CardView view in new List<PUI_CardView>(_Leaving)) {
                if (view != null)
                    Destroy(view.gameObject);
            }

            _Views.Clear();
            _Leaving.Clear();
        }

        #endregion

        #region Helpers

        private static bool IsAlive(PlayerController player)
        {
            return player != null && player.Object != null && player.Object.IsValid;
        }

        private static bool IsCounted(CardType type)
        {
            return type == CardType.Partisan || type == CardType.Tools || type == CardType.Equipment;
        }

        private static bool Contains(IReadOnlyList<ICard> cards, int id)
        {
            for (int i = 0; i < cards.Count; i++) {
                if (cards[i] != null && cards[i].ID == id)
                    return true;
            }
            return false;
        }

        private float CardWidth()
        {
            RectTransform rt = (RectTransform)_CardPrefab.transform;

            return rt.rect.width > 0f ? rt.rect.width : rt.sizeDelta.x;
        }

        #endregion
    }
}
