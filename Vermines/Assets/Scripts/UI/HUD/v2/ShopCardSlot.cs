using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

namespace Vermines.UI.Card
{
    public class ShopCardSlot : CardSlotBase
    {
        private void Awake()
        {
            _acceptedType = CardType.None;
            IsInteractable = false;
        }

        public override bool CanAcceptCard(ICard card)
        {
            return false;
        }
    }
}