using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vermines.CardSystem.Elements;
using System.Collections.Generic;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.UI.Shop
{
    public class ShopMarketUI : ShopBaseUI
    {
        public TMP_Text AreaName;
        public TMP_Text AreaDescription;
        public Image PortraitLeft;
        public Image PortraitRight;

        [Header("Sprite")]
        public Sprite SpriteMagician;
        public bool FlipMagician = false;
        public Sprite SpriteBlacksmith;
        public bool FlipBlacksmith = false;

        public string ShopName = "Market";
        public string ShopDescription = "Welcome to the vermin's lair.";

        private void Start()
        {
            AreaName.text = ShopName;
            AreaDescription.text = ShopDescription;
            shopType = ShopType.Market;

            if (SpriteMagician != null)
            {
                PortraitRight.sprite = SpriteMagician;
                PortraitRight.SetNativeSize();
            }
            else
            {
                PortraitRight.gameObject.SetActive(false);
            }
            if (FlipMagician)
            {
                PortraitRight.transform.localScale = new Vector3(PortraitRight.transform.localScale.x * -1, PortraitRight.transform.localScale.y, PortraitRight.transform.localScale.z);
            }
            if (SpriteBlacksmith != null)
            {
                PortraitLeft.sprite = SpriteBlacksmith;
                PortraitLeft.SetNativeSize();
            }
            else
            {
                PortraitLeft.gameObject.SetActive(false);
            }
            if (FlipBlacksmith)
            {
                PortraitLeft.transform.localScale = new Vector3(PortraitRight.transform.localScale.x * -1, PortraitRight.transform.localScale.y, PortraitRight.transform.localScale.z);
            }
        }

        public override void OnBuyCard(ICard card)
        {
            Debug.Log("Achat dans ShopA");
        }

        private void OnEnable()
        {
            if (ShopManager.Instance == null)
            {
                Debug.LogError("[ShopMarketUI] ShopManager.Instance is null at OnEnable.");
                return;
            }
            Debug.Log("[ShopMarketUI] Subscribing to OnShopUpdated event.");
            ShopManager.Instance.OnShopUpdated += HandleShopUpdate;
        }

        private void HandleShopUpdate(ShopType shopType, List<ShopCardEntry> newEntries)
        {
            Debug.Log($"[ShopMarketUI] Received update for {shopType} with {newEntries.Count} entries.");
            if (ShopType.Market != shopType) return;

            Init(newEntries);
        }
    }
}