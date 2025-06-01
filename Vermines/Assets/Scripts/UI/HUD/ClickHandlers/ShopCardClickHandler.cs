using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI;

public class ShopCardClickHandler : ICardClickHandler
{
    private readonly ShopType _shopType;
    private readonly int _slotId;
    private readonly ShopConfirmPopup _popup;

    public ShopCardClickHandler(ShopType shopType, int slotId, ShopConfirmPopup popup)
    {
        _shopType = shopType;
        _slotId = slotId;
        _popup = popup;
    }

    public void OnCardClicked(ICard card)
    {
        Debug.Log($"[ShopCardClickHandler] Card clicked: {card.Data.Name}");

        if (UIContextManager.Instance.IsInContext<ReplaceEffectContext>())
        {
            _popup.Setup(card, (c) =>
            {
                Debug.Log($"[ShopCardClickHandler] Setup replace popup for {card.Data.Name}");
                GameEvents.OnCardClickedInShopWithSlotIndex.Invoke(_shopType, _slotId);
                GameplayUIController controller = GameObject.FindAnyObjectByType<GameplayUIController>();
                if (controller != null)
                {
                    controller.ShowLast();
                }
            }, isReplace: true, _shopType);
        }
        else
        {
            _popup.Setup(card, (c) =>
            {
                Debug.Log($"[ShopCardClickHandler] Setup popup for {card.Data.Name}");
                GameEvents.InvokeOnCardPurchaseRequested(_shopType, _slotId);
            }, isReplace: false, _shopType);
        }

        _popup.gameObject.SetActive(true);
    }
}
