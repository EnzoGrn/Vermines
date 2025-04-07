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

    public static IUIContext Create(ICard card, ShopType shopType, int slotId)
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

        Debug.Log($"[ShopConfirmPopup] Creation of a new popup for {card.Data.name}");
        var popupGO = GameObject.Instantiate(_PopupPrefab, _PopupParent);
        var popup = popupGO.GetComponent<ShopConfirmPopup>();

        popup.Setup(card, (c) => HandlePurchase(c, shopType, slotId));

        return popup;
    }

    private static void HandlePurchase(ICard card, ShopType shopType, int slotId)
    {
        Debug.Log($"[ShopConfirmPopup] Purchase confirmed for {card.Data.name}");

        GameEvents.OnCardBought.Invoke(shopType, slotId);
    }
}
