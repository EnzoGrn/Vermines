using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
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
        protected ShopType shopType;

        public ShopType ShopType => shopType;

        public virtual void Init(List<ShopCardEntry> entries)
        {
            Debug.Log($"[ShopBaseUI] Init with {entries.Count} entries.");
            currentEntries = entries;
            PopulateShop();
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
                slot.Init(entry.Data, entry.IsNew, new ShopCardClickHandler(shopType, i));
                activeSlots.Add(slot);
            }
        }

        public abstract void OnBuyCard(ICard card);
    }
}
