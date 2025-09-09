using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.UI.GameTable;

namespace Vermines.UI.Card
{
    public abstract class CardSlotBase : MonoBehaviour
    {
        [SerializeField] private GameObject _CardDisplayPrefab;
        protected int _SlotIndex;
        protected CardType _acceptedType = CardType.None;

        public CardDisplay CardDisplay { get; private set; }
        public bool IsInteractable { get; set; } = true;

        public virtual void SetIndex(int index)
        {
            _SlotIndex = index;
        }

        public virtual void Init(ICard card, bool isNew, ICardClickHandler clickHandler = null)
        {
            if (CardDisplay == null)
            {
                GameObject obj = Instantiate(_CardDisplayPrefab, transform);
                DraggableCard draggableCard = obj.GetComponent<DraggableCard>();
                if (draggableCard != null)
                {
                    Destroy(draggableCard);
                }
                CardDisplay = obj.GetComponent<CardDisplay>();
            }

            CardDisplay.gameObject.SetActive(false);

            if (card == null) return;

            CardDisplay.Display(card, clickHandler);
            CardDisplay.transform.localScale = Vector3.one;

            if (isNew)
            {
                Debug.Log($"[{GetType().Name}] Card {card.Data.Name} is new.");
            }

            //Debug.Log($"[{GetType().Name}] Initializing slot {_SlotIndex} with card {card.Data.Name} of type {card.Data.Type}.");

            CardDisplay.gameObject.SetActive(true);
        }

        public virtual void ResetSlot()
        {
            if (CardDisplay != null)
            {
                CardDisplay.Clear();
                CardDisplay.gameObject.SetActive(false);
            }
        }

        public virtual bool CanAcceptCard(ICard card)
        {
            return card != null && (_acceptedType == CardType.None || card.Data.Type == _acceptedType) && IsInteractable;
        }

        public virtual void SetAcceptedType(CardType type)
        {
            _acceptedType = type;
        }

        public virtual void SetInteractable(bool interactable)
        {
            IsInteractable = interactable;
        }

        public virtual void SetCard(ICard card)
        {
            if (CanAcceptCard(card))
            {
                Init(card, false, new HandClickHandler());
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot accept card {card.Data.Name} in slot {_SlotIndex}");
            }
        }

        public void SetClickHandler(ICardClickHandler handler)
        {
            CardDisplay.SetClickHandler(handler);
        }

        public int GetIndex() => _SlotIndex;
    }
}