using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vermines.HUD.Card
{
    using Vermines.HUD;

    public class CardDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Transform originalParent;
        
        private CardHover cardHover;

        private float xOffset;
        private float yOffset;

        private void Start()
        {
            cardHover = GetComponent<CardHover>();
            originalParent = transform.parent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("OnMouseDown");
            //transform.DOKill();
            //transform.SetParent(null);
            transform.rotation = Quaternion.identity;
            HandManager.instance.RemoveCard(gameObject);
            cardHover.SetLocked(true);
            xOffset = this.transform.position.x - eventData.position.x;
            yOffset = this.transform.position.y - eventData.position.y;

            GetComponent<CanvasGroup>().blocksRaycasts = false;

        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("OnMouseDrag");
            this.transform.position = new Vector3(eventData.position.x + xOffset, eventData.position.y + yOffset, 0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnMouseUp");
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            cardHover.SetLocked(false);
            ReturnToHand();
        }

        private void ReturnToHand()
        {
            //transform.SetParent(originalParent); // On remet la carte dans la main
            HandManager.instance.AddCard(gameObject); // On ajoute la carte à la main
            //transform.DOKill();
            HandManager.instance.UpdateCardPosition();
        }
    }
}