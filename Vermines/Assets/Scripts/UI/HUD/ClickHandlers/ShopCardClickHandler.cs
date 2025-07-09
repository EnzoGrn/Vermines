using Vermines.CardSystem.Elements;

public class ShopCardClickHandler : ICardClickHandler
{
    private readonly int _slotId;

    public ShopCardClickHandler(int slotId)
    {
        _slotId = slotId;
    }

    public void OnCardClicked(ICard card)
    {
        GameEvents.OnCardClicked.Invoke(card, _slotId);
    }
}
