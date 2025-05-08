using System.Collections.Generic;
using UnityEngine;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Card;
using System;

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
    }
}
