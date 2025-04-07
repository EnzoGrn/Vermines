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

        private void OnEnable()
        {
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
            if (shopType != this.ShopType)
                return;

            if (slotIndex < 0 || slotIndex >= activeSlots.Count)
            {
                Debug.LogWarning($"[ShopBaseUI] Invalid slot index {slotIndex}.");
                return;
            }

            var slot = activeSlots[slotIndex];

            CardSlotPool.Instance.ReturnSlot(slot);
            activeSlots[slotIndex] = null;
        }

    }
}
