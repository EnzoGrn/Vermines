using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;

public class ShopCardClickHandler : ICardClickHandler
{
    private readonly ShopType _shopType;
    private readonly int _slotId;

    public ShopCardClickHandler(ShopType shopType, int slotId)
    {
        _shopType = shopType;
        _slotId = slotId;
    }

    public void OnCardClicked(ICard card)
    {
        Debug.Log($"[ShopCardClickHandler] Card clicked: {card.Data.Name}");
        if (UIContextManager.Instance.HasContext()) return;

        var context = ShopConfirmPopupFactory.Create(card, _shopType, _slotId);
        UIContextManager.Instance.SetContext(context);
    }
}
