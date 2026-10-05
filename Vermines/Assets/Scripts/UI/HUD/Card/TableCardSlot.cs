
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
            if (!IsInteractable)
                return false;

            return card != null && (card.Data.Type == _acceptedType || _acceptedType == CardType.None);
        }
    }
}
