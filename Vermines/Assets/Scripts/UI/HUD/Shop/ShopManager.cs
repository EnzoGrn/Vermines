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
                bool isNew = !oldList.TryGetValue(slotIndex, out var oldCard) || oldCard.ID != newCard?.ID;

                entries.Add(new ShopCardEntry(newCard, isNew));
            }

            // Update the previous shop list
            previousShopStates[type] = new Dictionary<int, ICard>(newList);

            // Notification
            Debug.Log($"[ShopManager] {type} shop updated with {entries.Count} entries.");
            OnShopUpdated?.Invoke(type, entries);
        }
    }
}