namespace Vermines.UI.Card
{
    public class CardSlotPool : CardSlotPoolBase<ShopCardSlot>
    {
        public static new CardSlotPool Instance => (CardSlotPool)CardSlotPoolBase<ShopCardSlot>.Instance;
    }
}
