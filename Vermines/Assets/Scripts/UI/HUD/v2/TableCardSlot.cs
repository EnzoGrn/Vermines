using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

namespace Vermines.UI.Card
{
    public class TableCardSlot : CardSlotBase
    {
        public void Setup(CardType accepted)
        {
            _acceptedType = accepted;
        }

        public override bool CanAcceptCard(ICard card)
        {
            Debug.Log($"[TableCardSlot] Checking if card {card?.Data.Name} can be accepted in slot of type {_acceptedType}.");
            if (!IsInteractable) return false;
            if (card != null && card.Data.Type != _acceptedType && _acceptedType != CardType.None)
            {
                Debug.LogWarning($"[TableCardSlot] Card of type {card.Data.Type} cannot be accepted in slot of type {_acceptedType}.");
                return false;
            }

            Debug.Log($"[TableCardSlot] Card {card.Data.Name} accepted in slot of type {_acceptedType}.");
            return true;
        }
    }
}