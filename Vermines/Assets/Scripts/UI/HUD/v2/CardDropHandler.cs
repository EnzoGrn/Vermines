using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.UI.GameTable;

namespace Vermines.UI.Card
{
    public class CardDropHandler : MonoBehaviour, IDropHandler
    {
        protected CardSlotBase slot;

        private void Awake()
        {
            slot = GetComponent<CardSlotBase>();
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[CardDropHandler] Dropped on slot {slot.GetIndex()}");
            DraggableCard drag = eventData.pointerDrag?.GetComponent<DraggableCard>();
            if (drag == null || slot == null) return;

            ICard card = drag.GetCard();
            if (card == null)
            {
                Debug.Log($"[CardDropHandler] Card is null, cannot drop.");
                return;
            }
            if (slot.CanAcceptCard(card))
            {
                Debug.Log($"[CardDropHandler] Accepting card {card.Data.Name} in slot {slot.GetIndex()}");
                slot.SetCard(card);
                drag.gameObject.transform.SetParent(slot.transform);
                drag.OnDroppedOnTable();
            }
            else
            {
                Debug.Log($"[CardDropHandler] Cannot accept card {card.Data.Name} in slot {slot.GetIndex()}");
                // Return the card to its original position in hand
                // This could be a method in the DraggableCard class
                 drag.ReturnToOriginalPosition();
            }
        }
    }
}