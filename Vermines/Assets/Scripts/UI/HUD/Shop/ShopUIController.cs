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

        [Header("Configuration")]
        private ShopUIConfig config;

        [Header("Common UI")]
        public Transform cardSlotRoot;

        protected List<Vermines.UI.Screen.ShopCardEntry> currentEntries = new();
        protected List<ShopCardSlot> activeSlots = new();

        [SerializeField]
        private CardSlotPool _CardPool;

        [SerializeField]
        public ShopType ShopType;

        [SerializeField]
        private ShopConfirmPopup _popup;

        #region Override Methods

        /// <summary>
        /// Shows the parent screen.
        /// </summary>
        /// <param name="screen">The parent screen that this plugin is attached to.</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);

            _popup.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hides the parent screen.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            //GameEvents.OnShopUpdated.RemoveListener(HandleShopUpdate);
        }

        #endregion

        public void Init(List<Vermines.UI.Screen.ShopCardEntry> entries, ShopUIConfig configSet)
        {
            config = configSet;

            if (config == null)
            {
                Debug.LogError("[ShopUIController] Init called but config is null.");
                return;
            }
            Debug.Log($"[ShopUIController] Init called with {entries.Count} entries for {config.shopName}.");
            ShopType = config.shopType;
            if (bubbleLeft != null)
                bubbleLeft.gameObject.SetActive(false);
            if (bubbleRight != null)
                bubbleRight.gameObject.SetActive(false);
            SetupUI();
            currentEntries = entries;
            PopulateShop();

            if (_popup != null)
            {
                _popup.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("[ShopUIController] Popup is null.");
                return;
            }
            GameEvents.OnShopUpdated.AddListener(HandleShopUpdate);
        }

        private void SetupUI()
        {
            areaName.text = config.shopName;
            areaDescription.text = config.shopDescription;

            if (bubbleLeft != null && !string.IsNullOrWhiteSpace(config.leftDialogue) && config.portraitLeft != null)
            {
                bubbleLeft.SetText(config.leftDialogue);
                bubbleLeft.SetVisible(true);
            }

            if (bubbleRight != null && !string.IsNullOrWhiteSpace(config.rightDialogue) && config.portraitRight != null)
            {
                bubbleRight.SetText(config.rightDialogue);
                bubbleRight.SetVisible(true);
            }

            // TODO: Use the following code for Localization

            //areaName.text = ""; // Clear temp
            //areaDescription.text = "";

            //config.shopName.StringChanged += (value) => areaName.text = value;
            //config.shopDescription.StringChanged += (value) => areaDescription.text = value;

            //config.shopName.RefreshString(); // force refresh
            //config.shopDescription.RefreshString();

            //bubbleLeft.SetText(""); // clear
            //bubbleRight.SetText("");

            //leftDialogue.StringChanged += value => bubbleLeft.SetText(value);
            //rightDialogue.StringChanged += value => bubbleRight.SetText(value);

            //leftDialogue.RefreshString();
            //rightDialogue.RefreshString();

            SetupPortrait(portraitLeft, config.portraitLeft, config.flipLeft);
            SetupPortrait(portraitRight, config.portraitRight, config.flipRight);
        }

        private void SetupPortrait(Image image, Sprite sprite, bool flip)
        {
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
            Debug.Log($"[ShopUIController] Received {entries.Count} entries for {type}.");
            Init(entries, config);
        }

        public void SetDialogueVisible(bool visible)
        {
            if (bubbleLeft != null && config.portraitLeft != null && !string.IsNullOrWhiteSpace(config.leftDialogue))
                bubbleLeft.SetVisible(visible);

            if (bubbleRight != null && config.portraitRight != null && !string.IsNullOrWhiteSpace(config.rightDialogue))
                bubbleRight.SetVisible(visible);
        }

        protected virtual void PopulateShop()
        {
            foreach (var slot in activeSlots)
            {
                CardSlotPool.Instance.ReturnSlot(slot);
            }
            activeSlots.Clear();

            for (int i = 0; i < currentEntries.Count; i++)
            {
                Vermines.UI.Screen.ShopCardEntry entry = currentEntries[i];
                var slot = CardSlotPool.Instance.GetSlot(cardSlotRoot);

                if (slot == null)
                {
                    Debug.LogError($"[ShopUIController] Failed to get slot from pool for {ShopType} shop.");
                    continue;
                }

                slot.transform.SetParent(cardSlotRoot, false);
                slot.SetIndex(i);

                var clickHandler = CreateClickHandler(i);
                slot.Init(entry.Data, entry.IsNew, clickHandler);

                activeSlots.Add(slot);
            }
        }

        public void EnterReplaceMode(Action onCardReplaced)
        {
            Debug.Log("[ShopUIManager] Entered replace mode.");

            int shopSlotCount = currentEntries.Count;
            for (int i = 0; i < shopSlotCount; i++)
            {
                var slot = activeSlots[i];
                //slot.SetClickHandler(new ReplaceClickHandler(ShopType, i, onCardReplaced));
            }
        }

        protected virtual ICardClickHandler CreateClickHandler(int slotIndex)
        {
            return new ShopCardClickHandler(ShopType, slotIndex, _popup);
        }
    }
}
