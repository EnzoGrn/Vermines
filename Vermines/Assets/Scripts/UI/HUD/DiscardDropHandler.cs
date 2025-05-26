using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.UI.GameTable;
using DG.Tweening;

namespace Vermines.UI.Card
{
    public class DiscardDropHandler : CardDropHandler
    {
        private void Awake()
        {
            // Initialize any necessary components or variables here
            GameEvents.OnCardDiscardedRefused.AddListener(OnDiscardRefused);
            slot = GetComponent<CardSlotBase>();
        }

        private void OnDestroy()
        {
            // Clean up event listeners to avoid memory leaks
            GameEvents.OnCardDiscardedRefused.RemoveListener(OnDiscardRefused);
            GameEvents.OnCardDiscarded.RemoveListener(OnCardDiscarded);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            DraggableCard drag = eventData.pointerDrag?.GetComponent<DraggableCard>();
            if (drag == null || slot == null) return;

            if (GameManager.Instance.IsMyTurn() == false)
            {
                Debug.Log("[DiscardDropHandler] Not your turn, cannot discard card.");
                drag.ReturnToOriginalPosition();
                return;
            }

            ICard card = drag.GetCard();
            if (card == null)
            {
                Debug.Log("[DiscardDropHandler] Card is null, cannot discard.");
                return;
            }

            Debug.Log($"[DiscardDropHandler] Card {card.Data.Name} discard requested.");

            if (slot.CanAcceptCard(card))
            {
                GameEvents.OnCardDiscarded.AddListener(OnCardDiscarded);
                GameEvents.OnCardDiscardedRequested.Invoke(card);
                drag.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[CardDropHandler] Cannot accept card {card.Data.Name} in slot {slot.GetIndex()}");
                drag.ReturnToOriginalPosition();
            }
        }

        private void OnCardDiscarded(ICard card)
        {
            // Handle the card discard event here if needed
            Debug.Log($"[DiscardDropHandler] Card {card.Data.Name} has been discarded.");
            slot.ResetSlot();
            slot.SetCard(card);
            GameEvents.OnCardDiscarded.RemoveListener(OnCardDiscarded);
        }

        private void OnDiscardRefused(ICard card)
        {
            // Handle the discard refusal event here if needed
            Debug.Log($"[DiscardDropHandler] Card {card.Data.Name} discard refused.");
            GameObject go = HandManager.Instance.GetCardDisplayGO(card);
            if (go != null)
            {
                DraggableCard drag = go.GetComponent<DraggableCard>();
                if (drag != null)
                {
                    drag.ReturnToOriginalPosition();
                }
            }
            GameEvents.OnCardDiscarded.RemoveListener(OnCardDiscarded);
        }
    }
}
