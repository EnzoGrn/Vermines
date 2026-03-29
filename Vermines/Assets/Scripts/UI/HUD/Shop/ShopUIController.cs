using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Vermines.ShopSystem.Enumerations;
using System.Collections.Generic;
using Vermines.UI.Plugin;
using Vermines.UI.Card;
using System;

namespace Vermines.UI.Shop
{
    public class ShopUIController : GameplayScreenPlugin
    {
        [Header("UI Texts")]
        public TMP_Text areaName;
        public TMP_Text areaDescription;

        [Header("Portraits")]
        public Image portraitLeft;
        public Image portraitRight;

        [Header("Dialogue Bubbles")]
        [SerializeField] private DialogueBubble bubbleLeft;
        [SerializeField] private DialogueBubble bubbleRight;

        [Header("Common UI")]
        public Transform cardSlotRoot;

        [Header("Pagination")]
        [SerializeField] private Button _PrevButton;
        [SerializeField] private Button _NextButton;
        [SerializeField] private TMP_Text _PageIndicator;

        [Header("Dependencies")]
        [SerializeField] private CardSlotPool _CardPool;

        public ShopType ShopType { get; private set; }

        private ShopUIConfig _config;

        private List<Vermines.UI.Screen.ShopCardEntry> _currentEntries = new();
        private List<ShopCardSlot> _activeSlots = new();

        private int _currentPage = 0;
        private int SlotsPerPage => _config?.slotsPerPage ?? 5;
        private int TotalPages => _currentEntries.Count == 0
            ? 1
            : Mathf.CeilToInt((float)_currentEntries.Count / SlotsPerPage);

        #region Override Methods

        /// <summary>
        /// Shows the parent screen.
        /// </summary>
        /// <param name="screen">The parent screen that this plugin is attached to.</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        /// <summary>
        /// Hides the parent screen.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            GameEvents.OnShopUpdated.RemoveListener(HandleShopUpdate);
        }

        #endregion

        public void Init(List<Vermines.UI.Screen.ShopCardEntry> entries, ShopUIConfig configSet)
        {
            _config = configSet;
            _CardPool = CardSlotPool.Instance;

            if (_config == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "[{0}] Init called but config is null.",
                    nameof(ShopUIController)
                );
                return;
            }

            ShopType = _config.shopType;

            _currentPage = 0;

            SetupUI();

            GameEvents.OnShopUpdated.RemoveListener(HandleShopUpdate);
            GameEvents.OnShopUpdated.AddListener(HandleShopUpdate);

            SetEntries(entries);
        }

        private void SetupUI()
        {
            areaName.text = _config.shopName;
            areaDescription.text = _config.shopDescription;

            SetupDialogueBubbles();
            SetupPortrait(portraitLeft, _config.portraitLeft, _config.flipLeft);
            SetupPortrait(portraitRight, _config.portraitRight, _config.flipRight);

            // Pagination buttons
            if (_PrevButton != null)
            {
                _PrevButton.onClick.RemoveAllListeners();
                _PrevButton.onClick.AddListener(GoToPreviousPage);
            }

            if (_NextButton != null)
            {
                _NextButton.onClick.RemoveAllListeners();
                _NextButton.onClick.AddListener(GoToNextPage);
            }
        }

        private void SetupDialogueBubbles()
        {
            if (bubbleLeft != null)
            {
                bool hasLeft = !string.IsNullOrWhiteSpace(_config.leftDialogue)
                    && _config.portraitLeft != null;
                bubbleLeft.gameObject.SetActive(false);
                if (hasLeft)
                {
                    bubbleLeft.SetText(_config.leftDialogue);
                    bubbleLeft.SetVisible(true);
                }
            }

            if (bubbleRight != null)
            {
                bool hasRight = !string.IsNullOrWhiteSpace(_config.rightDialogue)
                    && _config.portraitRight != null;
                bubbleRight.gameObject.SetActive(false);
                if (hasRight)
                {
                    bubbleRight.SetText(_config.rightDialogue);
                    bubbleRight.SetVisible(true);
                }
            }
        }

        private void SetupPortrait(Image image, Sprite sprite, bool flip)
        {
            if (image == null) return;

            if (sprite == null)
            {
                image.gameObject.SetActive(false);
                return;
            }

            image.sprite = sprite;
            image.gameObject.SetActive(true);
            image.SetNativeSize();

            Vector3 scale = image.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (flip ? -1 : 1);
            image.transform.localScale = scale;
        }

        private void HandleShopUpdate(ShopType type, List<Vermines.UI.Screen.ShopCardEntry> entries)
        {
            if (type != ShopType) return;
            SetEntries(entries);
        }

        public void SetEntries(List<Vermines.UI.Screen.ShopCardEntry> entries)
        {
            _currentEntries = entries ?? new List<Vermines.UI.Screen.ShopCardEntry>();

            foreach (var e in _currentEntries)
                Debug.Log($"[ShopUIController] SetEntries — card {e.Data?.ID} stackCount={e.StackCount}");

            if (_currentPage >= TotalPages)
                _currentPage = 0;

            PopulateCurrentPage();
            RefreshPaginationUI();
        }

        private void GoToNextPage()
        {
            if (_currentPage >= TotalPages - 1) return;
            _currentPage++;
            PopulateCurrentPage();
            RefreshPaginationUI();
        }

        private void GoToPreviousPage()
        {
            if (_currentPage <= 0) return;
            _currentPage--;
            PopulateCurrentPage();
            RefreshPaginationUI();
        }

        private void RefreshPaginationUI()
        {
            if (_PageIndicator != null)
            {
                _PageIndicator.text = TotalPages > 1
                    ? $"{_currentPage + 1} / {TotalPages}"
                    : string.Empty;

                if (_PageIndicator.transform.parent != null)
                    _PageIndicator.transform.parent.gameObject.SetActive(TotalPages > 1);
            }

            if (_PrevButton != null)
                _PrevButton.gameObject.SetActive(_currentPage > 0);

            if (_NextButton != null)
                _NextButton.gameObject.SetActive(_currentPage < TotalPages - 1);
        }

        public void SetDialogueVisible(bool visible)
        {
            if (bubbleLeft != null
                && _config.portraitLeft != null
                && !string.IsNullOrWhiteSpace(_config.leftDialogue))
                bubbleLeft.SetVisible(visible);

            if (bubbleRight != null
                && _config.portraitRight != null
                && !string.IsNullOrWhiteSpace(_config.rightDialogue))
                bubbleRight.SetVisible(visible);
        }

        protected virtual void PopulateCurrentPage()
        {
            ReturnAllSlots();

            int startIndex = _currentPage * SlotsPerPage;

            for (int i = 0; i < SlotsPerPage; i++)
            {
                var slot = _CardPool.GetSlot(cardSlotRoot);

                if (slot == null)
                {
                    Debug.LogErrorFormat(
                        gameObject,
                        "[{0}] Failed to get slot from pool for shop {1}.",
                        nameof(ShopUIController),
                        ShopType
                    );
                    continue;
                }

                slot.ResetSlot();
                slot.gameObject.SetActive(true);
                slot.transform.SetParent(cardSlotRoot, false);
                slot.SetIndex(i);
                slot.transform.SetSiblingIndex(i);

                int entryIndex = startIndex + i;
                bool hasEntry = entryIndex < _currentEntries.Count;
                var entry = hasEntry ? _currentEntries[entryIndex] : null;

                if (entry?.Data != null)
                {
                    slot.Init(entry.Data, entry.IsNew, CreateClickHandler(entryIndex));

                    Debug.LogFormat(
                        "[{0}] Populating slot {1} with card ID {2} (Entry Index: {3}).",
                        nameof(ShopUIController),
                        i,
                        entry.Data.ID,
                        entryIndex
                    );
                    if (ShopType == ShopType.Market)
                    {
                        Debug.Log($"[ShopUIController] entry.StackCount for card {entry.Data.ID} = {entry.StackCount}");
                        slot.ShowStackCount(entry.StackCount);
                    }
                }

                _activeSlots.Add(slot);
            }
        }

        private void ReturnAllSlots()
        {
            foreach (var slot in _activeSlots)
            {
                if (slot == null) return;

                slot.gameObject.SetActive(false);
                slot.transform.SetParent(_CardPool.transform, false);
                _CardPool.ReturnSlot(slot);
            }
            _activeSlots.Clear();
        }

        protected virtual ICardClickHandler CreateClickHandler(int slotIndex)
        {
            return new ShopCardClickHandler(slotIndex);
        }
    }
}
