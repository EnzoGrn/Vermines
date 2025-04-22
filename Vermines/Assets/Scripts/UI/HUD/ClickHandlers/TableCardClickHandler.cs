using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;

public class TableCardClickHandler : ICardClickHandler
{
    private readonly int _slotId;

    public TableCardClickHandler(int slotId)
    {
        _slotId = slotId;
    }

    public void OnCardClicked(ICard card)
    {
        Debug.Log($"[TableCardClickHandler] Card clicked: {card.Data.Name}");
        //if (UIContextManager.Instance.HasContext()) return;
    }
}
