using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using Vermines.Player;
namespace Vermines.HUD.Card
{
    public class DiscardArea : MonoBehaviour, ICardDropArea, IDropHandler
    {
        public bool IsDropAllowed(CardDraggable card)
        {
            return true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log(eventData.pointerDrag.name + " was dropped on " + gameObject.name);
            CardDraggable card = eventData.pointerDrag.GetComponent<CardDraggable>();
            if (card != null)
            {
                if (IsDropAllowed(card))
                {
                    OnDropCard(card);
                }
            }
        }

        public void OnDropCard(CardDraggable card)
        {
            card.transform.DOMove(transform.position, 0.3f);
            card.transform.SetParent(transform);
            Destroy(card.GetComponent<CardDraggable>());
            Destroy(card.GetComponent<CardHover>());
            Debug.Log("Card dropped on Discard Area");

            CardBase cardBase = card.GetComponent<CardBase>();

            if (cardBase != null)
            {
                // TODO: Add card to discard list, remove it from hand, and play effect
                GameEvents.InvokeOnDiscard(cardBase.Card.ID);
            }
        }
    }
}