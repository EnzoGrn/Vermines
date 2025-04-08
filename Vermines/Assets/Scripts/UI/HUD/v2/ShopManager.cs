using System;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.UI.Shop
{
    public class ShopCardEntry
    {
        public ICard Data;
        public bool IsNew;

        public ShopCardEntry(ICard data, bool isNew = false)
        {
            Data = data;
            IsNew = isNew;
        }
    }

    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;
        public event Action<ShopType, List<ShopCardEntry>> OnShopUpdated;

        private Dictionary<ShopType, Dictionary<int, ICard>> previousShopStates = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            GameEvents.OnCardPurchased.AddListener(OnCardPurchased);
        }

        public void ReceiveFullShopList(ShopType type, Dictionary<int, ICard> newList)
        {
            List<ShopCardEntry> entries = new();

            // Check if there's an old list for this shop type
            Dictionary<int, ICard> oldList = previousShopStates.ContainsKey(type)
                ? previousShopStates[type]
                : new Dictionary<int, ICard>();

            foreach (var kvp in newList)
            {
                int slotIndex = kvp.Key;
                ICard newCard = kvp.Value;

                // Check if the card is new or has changed in the slot
                bool isNew = !oldList.TryGetValue(slotIndex, out var oldCard) || oldCard?.ID != newCard?.ID;

                entries.Add(new ShopCardEntry(newCard, isNew));
            }

            // Update the previous shop list
            previousShopStates[type] = new Dictionary<int, ICard>(newList);

            // Notification
            Debug.Log($"[ShopManager] {type} shop updated with {entries.Count} entries.");
            OnShopUpdated?.Invoke(type, entries);
        }

        public List<ShopCardEntry> GetEntries(ShopType type)
        {
            if (previousShopStates.TryGetValue(type, out var shopList))
            {
                List<ShopCardEntry> entries = new();
                foreach (var kvp in shopList)
                {
                    entries.Add(new ShopCardEntry(kvp.Value));
                }
                return entries;
            }
            return new List<ShopCardEntry>();
        }

        public void OnCardPurchased(ShopType shopType, int slotIndex)
        {
            if (!previousShopStates.TryGetValue(shopType, out var shopList))
            {
                Debug.LogWarning($"[ShopManager] No shop list found for type {shopType}");
                return;
            }

            if (!shopList.ContainsKey(slotIndex))
            {
                Debug.LogWarning($"[ShopManager] Slot index {slotIndex} not found in shop {shopType}");
                return;
            }

            Debug.Log($"[ShopManager] Card purchased from {shopType} shop at slot {slotIndex}");

            shopList[slotIndex] = null;

            ReceiveFullShopList(shopType, shopList);
        }
    }
}