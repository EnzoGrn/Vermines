using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Elements;

public class ShopConfirmPopup : MonoBehaviour, IUIContext
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    private ICard _CardData;

    public void Setup(ICard cardData, System.Action<ICard> onBuy)
    {
        _CardData = cardData;

        nameText.text = cardData.Data.name;

        buyButton.onClick.AddListener(() =>
        {
            onBuy?.Invoke(_CardData);
            UIContextManager.Instance.ClearContext();
        });

        cancelButton.onClick.AddListener(() =>
        {
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
