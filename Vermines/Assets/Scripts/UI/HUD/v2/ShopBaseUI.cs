using System.Collections.Generic;
using UnityEngine;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.UI.Shop
{
    public abstract class ShopBaseUI : MonoBehaviour
    {
        [Header("Common UI")]
        public Transform cardSlotRoot;

        protected List<ShopCardEntry> currentEntries = new();
        protected List<ShopCardSlot> activeSlots = new();

        [SerializeField]
        public ShopType ShopType;

        public virtual void Init(List<ShopCardEntry> entries)
        {
            Debug.Log($"[ShopBaseUI] Init with {entries.Count} entries.");
            currentEntries = entries;
            PopulateShop();
        }

        protected virtual void OnEnable()
        {
            Debug.Log($"[ShopBaseUI] OnEnable called for {ShopType} shop.");
            GameEvents.OnCardPurchase.AddListener(OnCardPurchased);
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
                ShopCardEntry entry = currentEntries[i];
                var slot = CardSlotPool.Instance.GetSlot(cardSlotRoot);

                slot.transform.SetParent(cardSlotRoot, false);
                slot.SetIndex(i);
                slot.Init(entry.Data, entry.IsNew, new ShopCardClickHandler(ShopType, i));
                activeSlots.Add(slot);
            }
        }

        public virtual void OnCardPurchased(ShopType shopType, int slotIndex)
        {
            Debug.Log($"[ShopBaseUI] OnCardPurchased called with {shopType} and slot index {slotIndex}.");
            if (shopType != this.ShopType)
                return;

            if (slotIndex < 0 || slotIndex >= activeSlots.Count)
            {
                Debug.LogWarning($"[ShopBaseUI] Invalid slot index {slotIndex}.");
                return;
            }

            Debug.Log($"[ShopBaseUI] Card purchased from slot {slotIndex} in {shopType} shop.");

            ShopCardSlot slot = activeSlots[slotIndex];

            if (slot == null)
            {
                Debug.LogWarning($"[ShopBaseUI] Slot {slotIndex} is null.");
                return;
            }

            Debug.Log($"[ShopBaseUI] The card {slot.CardDisplay.Card.Data.Name} has been purchased.");

            CardSlotPool.Instance.ReturnSlot(slot);
            activeSlots[slotIndex] = null;
        }

    }
}
