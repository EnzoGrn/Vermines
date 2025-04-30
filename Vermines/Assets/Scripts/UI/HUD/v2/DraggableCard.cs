using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using DG.Tweening;

namespace Vermines.UI.GameTable
{
    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Transform _originalParent;
        private Vector2 _originalPosition;
        private ICard _card;

        public void Init(ICard card)
        {
            _card = card;
        }

        public ICard GetCard()
        {
            if (_card == null)
            {
                CardDisplay cardDisplay = GetComponent<CardDisplay>();
                if (cardDisplay != null)
                {
                    _card = cardDisplay.Card;
                }
                else
                {
                    Debug.LogError($"[DraggableCard] Card is null and no CardDisplay found.");
                }
            }
            return _card;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalParent = transform.parent;
            _originalPosition = _rectTransform.anchoredPosition;
            transform.SetParent(transform.root);
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            CardDropHandler dropHandler = eventData.pointerDrag?.GetComponent<CardDropHandler>();
            if (dropHandler == null)
            {
                ReturnToOriginalPosition();
            }
        }

        public void OnDroppedOnTable()
        {
            // Lock, disable the drop or something like that
            _canvasGroup.blocksRaycasts = true;
            HandManager.Instance.RemoveCard(gameObject);
            gameObject.transform.DOKill(true);
            Destroy(gameObject);
        }

        public void ReturnToOriginalPosition()
        {
            transform.SetParent(_originalParent);
            _rectTransform.anchoredPosition = _originalPosition;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}