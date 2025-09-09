using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;

public class CardInfo : MonoBehaviour
{
    [SerializeField] private CardDisplay _cardDisplay;

    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    void Start()
    {
        GameEvents.OnCardClicked.AddListener(Show);
        gameObject.SetActive(false);
    }

    public void Show(ICard card, int i)
    {
        Debug.Log($"[DraggableResizableCard] Show card: {card?.Data.Name}, index: {i}");
        if (card == null || i > 0) return;

        if (_cardDisplay != null)
        {
            _cardDisplay.Display(card);
            gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(closeKey))
        {
            CloseCard();
        }
    }

    public void CloseCard()
    {
        gameObject.SetActive(false);
    }
}
