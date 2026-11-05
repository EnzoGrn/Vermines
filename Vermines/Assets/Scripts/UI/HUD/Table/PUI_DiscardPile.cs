using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

using Random = System.Random;

namespace Vermines.UI.GameTable {

    using Vermines.CardSystem.Elements;
    using Vermines.UI.Card;
    using Vermines.Player;

    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Discard pile of the local player on the game table: every card of the discard and of the tool discard,
    /// stacked a little untidily (each card is slightly shifted and turned).
    /// </summary>
    /// <remarks>
    /// The piles are networked on the <see cref="PlayerController"/> and polled every frame while the table is open:
    /// a card that leaves the discard (shuffled back into the deck, taken back by an effect...) leaves the pile.
    /// A card dropped on the discard zone is shown at once (see <see cref="AddPending"/>) until the server confirms it.
    /// </remarks>
    public class PUI_DiscardPile : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Prefab of a card (the same as the table slots).
        /// </summary>
        [Header("References")]
        [SerializeField]
        private GameObject _CardDisplayPrefab;

        /// <summary>
        /// Parent of the cards (this transform when empty).
        /// </summary>
        [SerializeField]
        private RectTransform _Container;

        /// <summary>
        /// Optional text showing the number of cards in the pile.
        /// </summary>
        [SerializeField]
        private Text _CountText;

        /// <summary>
        /// Also stacks the tool discard.
        /// </summary>
        [Header("Content")]
        [SerializeField]
        private bool _IncludeToolDiscard = true;

        /// <summary>
        /// Number of cards actually drawn, from the top of the pile (0 = all of them).
        /// </summary>
        [SerializeField, Min(0)]
        private int _MaxVisibleCards = 0;

        /// <summary>
        /// Maximum shift of a card from the center of the pile.
        /// </summary>
        [Header("Untidy pile")]
        [SerializeField]
        private Vector2 _MaxOffset = new Vector2(9f, 7f);

        /// <summary>
        /// Maximum rotation of a card, in degrees.
        /// </summary>
        [SerializeField]
        private float _MaxRotation = 9f;

        /// <summary>
        /// Height from which a new card falls on the pile.
        /// </summary>
        [Header("Animations")]
        [SerializeField]
        private float _DropHeight = 45f;

        /// <summary>
        /// Duration of the fall of a new card.
        /// </summary>
        [SerializeField]
        private float _DropDuration = 0.3f;

        /// <summary>
        /// Delay between two cards falling together (cards discarded while the table was closed).
        /// </summary>
        [SerializeField]
        private float _DropStagger = 0.05f;

        /// <summary>
        /// Duration of the disappearance of a card leaving the discard.
        /// </summary>
        [SerializeField]
        private float _RemoveDuration = 0.25f;

        /// <summary>
        /// A card dropped on the discard zone that the server never confirms leaves the pile after this delay.
        /// </summary>
        [SerializeField]
        private float _PendingTimeout = 3f;

        #endregion

        private class Entry {

            public ICard Card;

            public RectTransform Rect;

            public CanvasGroup Group;

            public Vector2 Position;

            public float Rotation;

            public bool Pending;

            public float PendingSince;

            public bool Removing;
        }

        private readonly List<Entry> _Entries = new();

        private readonly List<ICard> _Cards = new();

        private readonly HashSet<int> _CardIds = new();

        private PlayerController _Player;

        private int _Signature;

        private bool _Built;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_Container == null)
                _Container = transform as RectTransform;
        }

        private void OnEnable()
        {
            _Signature = 0;

            Refresh();
        }

        private void OnDisable()
        {
            for (int i = _Entries.Count - 1; i >= 0; i--) {
                Entry entry = _Entries[i];

                KillTweens(entry);

                if (entry.Removing) {
                    Destroy(entry.Rect.gameObject);
                    _Entries.RemoveAt(i);

                    continue;
                }
                SetRestPose(entry);
            }
        }

        private void Update()
        {
            Refresh();
            ExpirePending();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows <paramref name="card"/> on top of the pile before the server confirms the discard.
        /// </summary>
        public void AddPending(ICard card)
        {
            if (card == null || FindEntry(card.ID) != null)
                return;
            Entry entry = CreateEntry(card);

            if (entry == null)
                return;
            entry.Pending      = true;
            entry.PendingSince = Time.unscaledTime;

            PlayDrop(entry, 0f);
            UpdateVisibility();
        }

        /// <summary>
        /// Removes every card at once.
        /// </summary>
        public void Clear()
        {
            foreach (Entry entry in _Entries) {
                KillTweens(entry);
                Destroy(entry.Rect.gameObject);
            }
            _Entries.Clear();

            _Signature = 0;

            UpdateCount();
        }

        #endregion

        #region Refresh

        private void Refresh()
        {
            PlayerController local = PlayerController.Local;

            if (!IsAlive(local)) {
                if (_Player != null) {
                    _Player = null;

                    Clear();
                }
                return;
            }

            if (local != _Player) {
                _Player = local;
                _Built  = false;

                Clear();
            }
            int signature = ComputeSignature(local);

            if (signature == _Signature)
                return;
            _Signature = signature;

            Sync(local, _Built);

            _Built = true;
        }

        private void Sync(PlayerController player, bool animate)
        {
            _Cards.Clear();
            _CardIds.Clear();

            AddCards(player.Discard);

            if (_IncludeToolDiscard)
                AddCards(player.ToolDiscard);

            // Cards that left the discard.
            foreach (Entry entry in _Entries) {
                if (entry.Removing)
                    continue;
                if (_CardIds.Contains(entry.Card.ID))
                    entry.Pending = false;
                else if (!entry.Pending)
                    RemoveEntry(entry, animate);
            }

            // New cards, on top of the pile.
            int dropped = 0;

            foreach (ICard card in _Cards) {
                if (FindEntry(card.ID) != null)
                    continue;
                Entry entry = CreateEntry(card);

                if (entry == null)
                    continue;
                if (animate)
                    PlayDrop(entry, dropped++ * _DropStagger);
                else
                    SetRestPose(entry);
            }
            _Entries.RemoveAll(entry => entry.Rect == null);

            UpdateVisibility();
        }

        private void AddCards(IReadOnlyList<ICard> cards)
        {
            if (cards == null)
                return;
            foreach (ICard card in cards) {
                if (card != null && _CardIds.Add(card.ID))
                    _Cards.Add(card);
            }
        }

        private void ExpirePending()
        {
            foreach (Entry entry in _Entries) {
                if (!entry.Pending || entry.Removing || Time.unscaledTime - entry.PendingSince < _PendingTimeout)
                    continue;
                entry.Pending = false;

                if (!_CardIds.Contains(entry.Card.ID))
                    RemoveEntry(entry, true);
            }
        }

        private int ComputeSignature(PlayerController player)
        {
            unchecked {
                int hash = 17;

                hash = Hash(hash, player.Discard);

                if (_IncludeToolDiscard)
                    hash = Hash(hash * 31 + 7, player.ToolDiscard);
                return hash == 0 ? 1 : hash;
            }
        }

        private static int Hash(int hash, IReadOnlyList<ICard> cards)
        {
            unchecked {
                if (cards == null)
                    return hash * 31;
                hash = hash * 31 + cards.Count;

                foreach (ICard card in cards)
                    hash = hash * 31 + (card != null ? card.ID : -1);
                return hash;
            }
        }

        #endregion

        #region Entries

        private Entry FindEntry(int cardId)
        {
            foreach (Entry entry in _Entries) {
                if (!entry.Removing && entry.Card.ID == cardId)
                    return entry;
            }
            return null;
        }

        private Entry CreateEntry(ICard card)
        {
            if (_CardDisplayPrefab == null) {
                Debug.LogError("[PUI_DiscardPile] Missing card display prefab.", this);

                return null;
            }
            GameObject cardObject = Instantiate(_CardDisplayPrefab, _Container);
            DraggableCard draggable = cardObject.GetComponent<DraggableCard>();

            if (draggable != null)
                Destroy(draggable);
            CardDisplay display = cardObject.GetComponent<CardDisplay>();

            if (display != null)
                display.Display(card);
            // RectMask2D clips with an axis-aligned rectangle: on a turned card the description would be cut, so the pile turns it off.
            foreach (RectMask2D clip in cardObject.GetComponentsInChildren<RectMask2D>(true))
                clip.enabled = false;
            foreach (ScrollRect scroll in cardObject.GetComponentsInChildren<ScrollRect>(true))
                scroll.enabled = false;
            CanvasGroup group = cardObject.GetComponent<CanvasGroup>();

            if (group == null)
                group = cardObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable   = false;

            RectTransform rect = (RectTransform)cardObject.transform;
            Random random      = new Random(card.ID * 486187739 + 1013);
            Vector2 offset     = new Vector2(
                ((float)random.NextDouble() * 2f - 1f) * _MaxOffset.x,
                ((float)random.NextDouble() * 2f - 1f) * _MaxOffset.y
            );

            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            Entry entry = new Entry {
                Card     = card,
                Rect     = rect,
                Group    = group,
                Position = rect.anchoredPosition + offset,
                Rotation = ((float)random.NextDouble() * 2f - 1f) * _MaxRotation
            };
            _Entries.Add(entry);

            return entry;
        }

        private void RemoveEntry(Entry entry, bool animate)
        {
            entry.Removing = true;

            KillTweens(entry);

            if (!animate || !isActiveAndEnabled) {
                Destroy(entry.Rect.gameObject);
                entry.Rect = null;

                return;
            }
            RectTransform rect = entry.Rect;

            DOTween.Sequence().SetUpdate(true).SetTarget(rect)
                .Join(entry.Group.DOFade(0f, _RemoveDuration))
                .Join(rect.DOScale(0.85f, _RemoveDuration).SetEase(Ease.InQuad))
                .OnComplete(() => {
                    _Entries.Remove(entry);

                    if (rect != null)
                        Destroy(rect.gameObject);
                    UpdateCount();
                });
        }

        private void PlayDrop(Entry entry, float delay)
        {
            RectTransform rect = entry.Rect;

            KillTweens(entry);

            rect.anchoredPosition = entry.Position + Vector2.up * _DropHeight;
            rect.localEulerAngles = new Vector3(0f, 0f, entry.Rotation + (entry.Rotation >= 0f ? 14f : -14f));
            rect.localScale       = Vector3.one * 1.12f;
            entry.Group.alpha     = 0f;

            DOTween.Sequence().SetUpdate(true).SetTarget(rect).SetDelay(delay)
                .Join(rect.DOAnchorPos(entry.Position, _DropDuration).SetEase(Ease.OutCubic))
                .Join(rect.DOLocalRotate(new Vector3(0f, 0f, entry.Rotation), _DropDuration).SetEase(Ease.OutCubic))
                .Join(rect.DOScale(1f, _DropDuration).SetEase(Ease.OutBack))
                .Join(entry.Group.DOFade(1f, _DropDuration * 0.4f));
        }

        private static void SetRestPose(Entry entry)
        {
            entry.Rect.anchoredPosition = entry.Position;
            entry.Rect.localEulerAngles = new Vector3(0f, 0f, entry.Rotation);
            entry.Rect.localScale       = Vector3.one;
            entry.Group.alpha           = 1f;
        }

        private static void KillTweens(Entry entry)
        {
            if (entry.Rect == null)
                return;
            DOTween.Kill(entry.Rect);
            entry.Rect.DOKill();
            entry.Group.DOKill();
        }

        private void UpdateVisibility()
        {
            int visible = 0;

            for (int i = _Entries.Count - 1; i >= 0; i--) {
                Entry entry = _Entries[i];

                if (entry.Removing || entry.Rect == null)
                    continue;
                visible++;
                entry.Rect.gameObject.SetActive(_MaxVisibleCards <= 0 || visible <= _MaxVisibleCards);
            }
            UpdateCount();
        }

        private void UpdateCount()
        {
            if (_CountText == null)
                return;
            int count = 0;

            foreach (Entry entry in _Entries) {
                if (!entry.Removing)
                    count++;
            }
            _CountText.text = count.ToString();
        }

        private static bool IsAlive(PlayerController player)
        {
            return player != null && player.Object != null && player.Object.IsValid;
        }

        #endregion
    }
}
