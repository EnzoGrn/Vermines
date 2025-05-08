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
    [SerializeField] private TMP_Text soulValueText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    [Header("Card Display")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardDisplayParent;

    private ICard _CardData;

    private void Awake()
    {
        #region ERROR HANDLING
        if (cardDisplayPrefab == null)
        {
            Debug.LogError("[ShopConfirmPopup] Card display prefab is not assigned.");
        }

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
        _CardData = cardData;

        nameText.text = _CardData.Data.name;
        foreach (var effect in _CardData.Data.Effects)
        {
            descriptionText.text += effect.Description + "\n";
        }

        // TODO: This needs to be changed with Localization later, using SmartString
        costText.text = $"Cost: {_CardData.Data.Eloquence} eloquences";
        soulValueText.text = $"+{_CardData.Data.Souls} souls if sacrificed";
        questionText.text = isReplace
        ? $"Replace {_CardData.Data.name} ?"
        : $"Buy {_CardData.Data.name} ?";

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
            UIContextManager.Instance.PopContext();
        });

        cancelButton.onClick.AddListener(() =>
        {
            UIContextManager.Instance.PopContext();
            if (activeShop is ShopUIController controller)
            {
                controller.SetDialogueVisible(true);
            }
        });
    }

    public void Enter()
    {
        Debug.Log($"[ShopConfirmPopup] Entering shop confirm popup context");
        gameObject.SetActive(true);
    }

    public void Exit()
    {
        Destroy(gameObject);
    }
}
