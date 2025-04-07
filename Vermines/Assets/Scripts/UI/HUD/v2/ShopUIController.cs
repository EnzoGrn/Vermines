using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Vermines.ShopSystem.Enumerations;
using System.Collections.Generic;

namespace Vermines.UI.Shop
{
    public class ShopUIController : ShopBaseUI
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
        public ShopUIConfig config;

        public override void Init(List<ShopCardEntry> entries)
        {
            if (config == null)
            {
                Debug.LogError("[ShopUIController] Init called but config is null.");
                return;
            }

            ShopType = config.shopType;
            if (bubbleLeft != null)
                bubbleLeft.gameObject.SetActive(false);
            if (bubbleRight != null)
                bubbleRight.gameObject.SetActive(false);
            SetupUI();
            base.Init(entries);
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

        private void OnEnable()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopUpdated += HandleShopUpdate;
        }

        private void OnDisable()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopUpdated -= HandleShopUpdate;
        }

        private void HandleShopUpdate(ShopType type, List<ShopCardEntry> entries)
        {
            if (type != ShopType) return;
            Debug.Log($"[ShopUIController] Received {entries.Count} entries for {type}.");
            Init(entries);
        }

        public void SetDialogueVisible(bool visible)
        {
            if (bubbleLeft != null && config.portraitLeft != null && !string.IsNullOrWhiteSpace(config.leftDialogue))
                bubbleLeft.SetVisible(visible);

            if (bubbleRight != null && config.portraitRight != null && !string.IsNullOrWhiteSpace(config.rightDialogue))
                bubbleRight.SetVisible(visible);
        }
    }
}
