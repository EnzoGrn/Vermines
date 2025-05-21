using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

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

            CardDisplay.gameObject.SetActive(true);
        }

        public virtual void ResetSlot()
        {
            if (CardDisplay != null)
            {
                Debug.Log($"[{GetType().Name}] Resetting slot {_SlotIndex}");
                CardDisplay.Clear();
                CardDisplay.gameObject.SetActive(false);
            }
        }

        public virtual bool CanAcceptCard(ICard card)
        {
            Debug.Log($"[{GetType().Name}] Checking if card {card?.Data.Name} can be accepted in slot {_SlotIndex} of type {_acceptedType}.");
            return card != null && (_acceptedType == CardType.None || card.Data.Type == _acceptedType);
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
                Init(card, false);
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot accept card {card.Data.Name} in slot {_SlotIndex}");
            }
        }

        public int GetIndex() => _SlotIndex;
    }
}