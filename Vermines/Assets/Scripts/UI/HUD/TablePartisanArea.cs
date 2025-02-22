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
                Debug.LogWarning($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) Card not found.", this);
                return false;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) GameManager not found.", this);
                return false;
            }

            if (GameManager.Instance.IsMyTurn() == false)
            {
                Debug.Log($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) Action not allowed: Not your turn.", this);
                return false;
            }

            // Get gameObject from card
            GameObject cardGameObject = card.gameObject;
            CardBase cardBase = cardGameObject.GetComponent<CardBase>();
            if (cardBase == null)
            {
                Debug.LogWarning($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) CardBase not found.", this);
                return false;
            }
            CardData cardData = cardBase.Card.Data;
            if (cardData == null)
            {
                Debug.LogWarning($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) CardData not found.", this);
                return false;
            }
            // Check if card is a partisan
            if (cardData.Type != CardType.Partisan)
            {
                Debug.Log($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) Action not allowed: Card is not a partisan.", this);
                return false;
            }
            return true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[CLIENT] [{gameObject.name}] ({nameof(TabelPartisanArea)}) {eventData.pointerDrag.name} was dropped on {gameObject.name}");
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

            CardBase cardBase = card.GetComponent<CardBase>();

            if (cardBase != null)
            {
                GameEvents.InvokeOnCardPlayed(cardBase.Card.ID);
            }
        }
    }
}