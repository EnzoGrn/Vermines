using System;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;

public static class ShopConfirmPopupFactory
{
    private static GameObject _PopupPrefab;
    private static Transform _PopupParent;

    public static void Init(GameObject popupPrefab, Transform popupParent)
    {
        _PopupPrefab = popupPrefab;
        _PopupParent = popupParent;
    }

    public static ShopConfirmPopup Create(ICard card, ShopType shopType, int slotId, bool isReplace, Action<ICard> onBuy)
    {
        if (_PopupPrefab == null)
        {
            Debug.LogError("[ShopConfirmPopup] Popup prefab is not set. Please call Init() first.");
            return null;
        }
        if (card == null)
        {
            Debug.LogError("[ShopConfirmPopup] Card is null. Cannot create popup.");
            return null;
        }
        if (_PopupParent == null)
        {
            Debug.LogError("[ShopConfirmPopup] Popup parent is not set. Cannot create popup.");
            return null;
        }

        Debug.Log($"[ShopConfirmPopup] Creation of a new popup for {card.Data.Name}");
        var popupGO = GameObject.Instantiate(_PopupPrefab, _PopupParent);
        var popup = popupGO.GetComponent<ShopConfirmPopup>();

        popup.Setup(card, onBuy, isReplace);

        return popup;
    }

    public static void RequestPurchase(ICard card, ShopType shopType, int slotId)
    {
        Debug.Log($"[ShopConfirmPopup] Purchase asked for {card.Data.Name}");

        GameEvents.OnCardPurchaseRequested.Invoke(shopType, slotId);
    }

    public static void RequestReplace(ICard card, ShopType shopType, int slotId)
    {
       
        if (UIContextManager.Instance.IsInContext<ReplaceEffectContext>())
        {
            Debug.Log($"[ShopConfirmPopup] Replace asked for {card.Data.Name}");
            ReplaceEffectContext context = UIContextManager.Instance.GetContext<ReplaceEffectContext>();
            context.OnShopCardClicked(shopType, slotId);
        }
    }
}
