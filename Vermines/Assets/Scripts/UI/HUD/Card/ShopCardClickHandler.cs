using UnityEngine;
using Vermines.CardSystem.Elements;

public class ShopCardClickHandler : ICardClickHandler
{
    public void OnCardClicked(ICard card)
    {
        Debug.Log($"[ShopCardClickHandler] Card clicked: {card.Data.Name}");
        if (UIContextManager.Instance.HasContext()) return;

        var context = ShopConfirmPopupFactory.Create(card);
        UIContextManager.Instance.SetContext(context);
    }
}
