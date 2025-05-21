using System.Collections.Generic;
using UnityEngine;
using Vermines.UI.Card;

namespace Vermines.UI.GameTable
{
    public class GameTableCardSlotPool : MonoBehaviour
    {
        public static GameTableCardSlotPool Instance { get; private set; }

        [SerializeField] private GameObject tableCardSlotPrefab;
        private readonly Stack<TableCardSlot> _availableSlots = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Returns a slot instance parented under the specified transform.
        /// </summary>
        public TableCardSlot GetSlot(Transform parent)
        {
            TableCardSlot slot;

            if (_availableSlots.Count > 0)
            {
                slot = _availableSlots.Pop();
                slot.gameObject.SetActive(true);
            }
            else
            {
                GameObject obj = Instantiate(tableCardSlotPrefab, parent);
                slot = obj.GetComponent<TableCardSlot>();
            }

            slot.transform.SetParent(parent, false);
            return slot;
        }

        /// <summary>
        /// Returns the slot to the pool.
        /// </summary>
        public void ReturnSlot(TableCardSlot slot)
        {
            if (slot == null) return;

            slot.ResetSlot();
            //slot.gameObject.SetActive(false);
            _availableSlots.Push(slot);
        }

        public void ClearPool()
        {
            _availableSlots.Clear();
        }
    }
}
