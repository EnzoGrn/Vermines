using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Vermines.UI.Card
{
    /// <summary>
    /// Generic scene-singleton object pool for CardSlotBase-derived slot
    /// prefabs. Unifies what used to be two near-identical pools, one for
    /// the Shop (ShopCardSlot) and one for the Table (TableCardSlot).
    /// Each closed generic type gets its own static Instance - a static
    /// field on a generic class is per closed type in the CLR - so
    /// CardSlotPool (ShopCardSlot) and GameTableCardSlotPool (TableCardSlot)
    /// remain two fully independent singletons, same as before.
    /// </summary>
    public abstract class CardSlotPoolBase<T> : MonoBehaviour where T : CardSlotBase
    {
        public static CardSlotPoolBase<T> Instance { get; private set; }

        [FormerlySerializedAs("cardSlotPrefab")]
        [FormerlySerializedAs("tableCardSlotPrefab")]
        [SerializeField] private GameObject _slotPrefab;

        private readonly Stack<T> _availableSlots = new();

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Returns a slot instance parented under the specified transform.
        /// </summary>
        public T GetSlot(Transform parent)
        {
            T slot;

            if (_availableSlots.Count > 0)
            {
                slot = _availableSlots.Pop();
                slot.gameObject.SetActive(true);
            }
            else
            {
                GameObject obj = Instantiate(_slotPrefab, parent);

                if (obj == null)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to instantiate slot prefab.");
                    return null;
                }

                slot = obj.GetComponent<T>();
            }

            slot.transform.SetParent(parent, false);

            return slot;
        }

        /// <summary>
        /// Returns the slot to the pool.
        /// </summary>
        public void ReturnSlot(T slot)
        {
            if (slot == null)
                return;

            slot.ResetSlot();
            slot.gameObject.SetActive(false);
            _availableSlots.Push(slot);
        }

        public void ClearPool()
        {
            _availableSlots.Clear();
        }
    }
}
