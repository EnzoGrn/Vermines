using UnityEngine;
using Vermines.CardSystem.Elements;

public static class ShopConfirmPopupFactory
{
    private static GameObject _PopupPrefab;
    private static Transform _PopupParent;

    public static void Init(GameObject popupPrefab, Transform popupParent = null)
    {
        _PopupPrefab = popupPrefab;
        _PopupParent = popupPrefab.transform.parent;
    }

    public static IUIContext Create(ICard card)
    {
        Debug.Log($"[Popup] Creation d'un popup de confirmation pour {card.Data.name}");
        var popupGO = GameObject.Instantiate(_PopupPrefab, _PopupParent);
        var popup = popupGO.GetComponent<ShopConfirmPopup>();

        popup.Setup(card, HandlePurchase);

        return popup;
    }

    private static void HandlePurchase(ICard card)
    {
        Debug.Log($"[Popup] Achat confirm� pour {card.Data.name}");

        // ShopManager.Instance.PurchaseCard(cardData);
    }
}
