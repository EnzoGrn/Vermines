using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using Vermines.UI.Shop;
using Vermines.UI.Utils;

public class ShopConfirmPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text soulValueText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private Image typeIcon;

    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    [Header("Card Display")]
    [SerializeField] private GameObject cardDisplay;
    [SerializeField] private Transform cardDisplayParent;

    private ICard _CardData;

    private void Awake()
    {
        #region ERROR HANDLING

        if (cardDisplayParent == null)
        {
            Debug.LogError("[ShopConfirmPopup] Card display parent is not assigned.");
        }

        if (nameText == null)
        {
            Debug.LogError("[ShopConfirmPopup] Name text is not assigned.");
        }

        if (descriptionText == null)
        {
            Debug.LogError("[ShopConfirmPopup] Description text is not assigned.");
        }

        if (costText == null)
        {
            Debug.LogError("[ShopConfirmPopup] Cost text is not assigned.");
        }

        if (buyButton == null)
        {
            Debug.LogError("[ShopConfirmPopup] Buy button is not assigned.");
        }

        if (cancelButton == null)
        {
            Debug.LogError("[ShopConfirmPopup] Cancel button is not assigned.");
        }

        #endregion

        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        nameText.text = string.Empty;
        descriptionText.text = string.Empty;
        costText.text = string.Empty;
    }

    public void Setup(ICard cardData, System.Action<ICard> onBuy, bool isReplace = false)
    {
        nameText.text = string.Empty;
        descriptionText.text = string.Empty;
        costText.text = string.Empty;

        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        _CardData = cardData;

        nameText.text = _CardData.Data.Name;
        foreach (var effect in _CardData.Data.Effects)
        {
            descriptionText.text += effect.Description + "\n";
        }

        // TODO: This needs to be changed with Localization later, using SmartString

        // If we have a free context, we write "Free" instead of the cost
        costText.text = UIContextManager.Instance.IsInContext<FreeCardContext>()
        ? "Free"
        : $"Cost: {_CardData.Data.CurrentEloquence} eloquences";

        // If the card has a cost > 0 in souls, display it
        soulValueText.text = _CardData.Data.Souls > 0
            ? $"+{_CardData.Data.CurrentSouls} souls if sacrificed"
            : string.Empty;

        questionText.text = isReplace
            ? $"Replace {_CardData.Data.Name} ?"
            : $"Buy {_CardData.Data.Name} ?";

        typeText.text = _CardData.Data.Type == Vermines.CardSystem.Enumerations.CardType.Partisan
            ? _CardData.Data.Family.ToString()
            : _CardData.Data.Type.ToString();

        typeIcon.sprite = typeIcon.sprite = UISpriteLoader.GetDefaultSprite(_CardData.Data.Type, _CardData.Data.Family, "Icon");

        CardDisplay display = cardDisplay.GetComponent<CardDisplay>();
        display.Display(_CardData, null);

        // Hide the shop dialogue
        var activeShop = GameObject.FindAnyObjectByType<ShopUIController>();
        if (activeShop != null)
        {
            activeShop.SetDialogueVisible(false);
        }

        buyButton.onClick.AddListener(() =>
        {
            onBuy?.Invoke(_CardData);
            UIContextManager.Instance.PopContext();
            if (activeShop is ShopUIController controller)
            {
                controller.SetDialogueVisible(true);
            }
        });

        cancelButton.onClick.AddListener(() =>
        {
            if (activeShop is ShopUIController controller)
            {
                controller.SetDialogueVisible(true);
            }
            Debug.Log($"[ShopConfirmPopup] Cancel button clicked, exiting context.");
        });
    }

    public void Enter()
    {
        Debug.Log($"[ShopConfirmPopup] Entering shop confirm popup context");
        gameObject.SetActive(true);
    }

    public void Exit()
    {
        Debug.Log($"[ShopConfirmPopup] Exiting shop confirm popup context");
        gameObject.SetActive(false);
    }

    public string GetName()
    {
        return "Shop Confirm Popup";
    }
}
