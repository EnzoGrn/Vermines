using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Vermines.HUD.Card
{
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;

    public class TabelPartisanArea : MonoBehaviour, ICardDropArea, IDropHandler
    {
        public bool IsDropAllowed(CardDraggable card)
        {
            if (card == null) {
                return false;
            }
            // Get gameObject from card
            GameObject cardGameObject = card.gameObject;
            CardBase cardBase = cardGameObject.GetComponent<CardBase>();
            if (cardBase == null)
            {
                Debug.Log("CardBase component not found on card object.");
                return false;
            }
            CardData cardData = cardBase.Card.Data;
            if (cardData == null)
            {
                Debug.Log("CardData not found on card object.");
                return false;
            }
            // Check if card is a partisan
            if (cardData.Type != CardType.Partisan)
            {
                Debug.Log("Card is not a partisan.");
                return false;
            }
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
            card.GetComponent<CanvasGroup>().blocksRaycasts = true;
            card.gameObject.AddComponent<CardInTable>();
            card.gameObject.GetComponent<CardInTable>().SetCardBase(card.gameObject.GetComponent<CardBase>());
            card.gameObject.tag = "TableCard";
            Debug.Log("Card dropped on Discard Area");

            // TODO: Add card to active card list, remove it from hand, and play effect
            CardBase cardBase = card.GetComponent<CardBase>();

            if (cardBase != null)
            {
                // TODO: Add card to discard list, remove it from hand, and play effect
                GameEvents.InvokeOnCardPlayed(cardBase.Card.ID);
            }
        }
    }
}