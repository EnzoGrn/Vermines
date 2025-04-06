using System.Collections.Generic;
using UnityEngine;
using Vermines.UI.Shop;

namespace Vermines.UI
{
    public class CardSlotPool : MonoBehaviour
    {
        public static CardSlotPool Instance { get; private set; }

        [SerializeField] private GameObject cardSlotPrefab;
        private Queue<ShopCardSlot> availableSlots = new Queue<ShopCardSlot>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public ShopCardSlot GetSlot(Transform parent)
        {
            ShopCardSlot slot;

            if (availableSlots.Count > 0)
            {
                slot = availableSlots.Dequeue();
                slot.gameObject.SetActive(true);
            }
            else
            {
                var obj = Instantiate(cardSlotPrefab, parent);
                slot = obj.GetComponent<ShopCardSlot>();
            }

            slot.transform.SetParent(parent, false);
            return slot;
        }

        public void ReturnSlot(ShopCardSlot slot)
        {
            slot.ResetSlot();
            slot.gameObject.SetActive(false);
            availableSlots.Enqueue(slot);
        }

        public void ClearPool()
        {
            availableSlots.Clear();
        }
    }
}
