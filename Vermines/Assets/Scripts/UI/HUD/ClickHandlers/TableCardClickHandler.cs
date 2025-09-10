using Vermines.CardSystem.Elements;

public class TableCardClickHandler : ICardClickHandler
{
    private readonly int _slotId;

    public TableCardClickHandler(int slotId)
    {
        _slotId = slotId;
    }

    public void OnCardClicked(ICard card)
    {
        GameEvents.OnCardClicked.Invoke(card, _slotId);
    }
}
