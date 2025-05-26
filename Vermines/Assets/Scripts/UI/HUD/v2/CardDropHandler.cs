using DG.Tweening;
using System;
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

            GameEvents.OnCardPlayedRefused.AddListener(OnPlayRefused);
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[CardDropHandler] Dropped on slot {slot.GetIndex()}");
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
                Debug.Log($"[CardDropHandler] Card is null, cannot drop.");
                return;
            }
            if (slot.CanAcceptCard(card))
            {
                GameEvents.OnCardPlayed.AddListener(OnCardPlayed);
                GameEvents.OnCardPlayedRequested.Invoke(card);
                drag.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[CardDropHandler] Cannot accept card {card.Data.Name} in slot {slot.GetIndex()}");
                // Return the card to its original position in hand
                 drag.ReturnToOriginalPosition();
            }
        }

        private void OnCardPlayed(ICard card)
        {
            // Handle the card discard event here if needed
            Debug.Log($"[CardDropHandler] Card {card.Data.Name} has been played.");
            GameObject go = HandManager.Instance.GetCardDisplayGO(card);
            if (go != null)
            {
                // Remove the card from the hand and destroy it
                HandManager.Instance.RemoveCard(go);
                go.transform.DOKill(true);
                Destroy(go);

                // Display the card on the table
                slot.Init(card, true, new TableCardClickHandler(slot.GetIndex()));
            }
            GameEvents.OnCardPlayed.RemoveListener(OnCardPlayed);
        }

        private void OnPlayRefused(ICard card)
        {
            // Handle the discard refusal event here if needed
            Debug.Log($"[CardDropHandler] Card {card.Data.Name} play refused.");
            GameObject go = HandManager.Instance.GetCardDisplayGO(card);
            if (go != null)
            {
                DraggableCard drag = go.GetComponent<DraggableCard>();
                if (drag != null)
                {
                    drag.ReturnToOriginalPosition();
                }
            }
            GameEvents.OnCardPlayed.RemoveListener(OnCardPlayed);
        }
    }
}