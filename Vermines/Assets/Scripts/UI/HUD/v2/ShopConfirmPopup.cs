using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using Vermines.UI.Shop;

public class ShopConfirmPopup : MonoBehaviour, IUIContext
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    [Header("Card Display")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardDisplayParent;

    private ICard _CardData;

    public void Setup(ICard cardData, System.Action<ICard> onBuy)
    {
        _CardData = cardData;

        nameText.text = _CardData.Data.name;

        GameObject displayGO = Instantiate(cardDisplayPrefab, cardDisplayParent);
        displayGO.transform.SetSiblingIndex(0);
        CardDisplay display = displayGO.GetComponent<CardDisplay>();
        display.Display(_CardData, null);

        var activeShop = ShopUIManager.Instance != null ? ShopUIManager.Instance.GetActiveShop() : null;
        if (activeShop is ShopUIController controller)
        {
            controller.SetDialogueVisible(false);
        }

        buyButton.onClick.AddListener(() =>
        {
            onBuy?.Invoke(_CardData);
            UIContextManager.Instance.ClearContext();
        });

        cancelButton.onClick.AddListener(() =>
        {
            if (activeShop is ShopUIController controller)
            {
                controller.SetDialogueVisible(true);
            }
            UIContextManager.Instance.ClearContext();
        });
    }

    public void Enter()
    {
        gameObject.SetActive(true);
    }

    public void Exit()
    {
        Destroy(gameObject);
    }
}
