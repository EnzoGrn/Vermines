using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;

public class ShopCardSlot : MonoBehaviour
{
    [SerializeField] private GameObject _CardDisplayPrefab;
    private int _SlotIndex;
    public CardDisplay CardDisplay { get; private set; }

    public void SetIndex(int index)
    {
        _SlotIndex = index;
    }

    public void Init(ICard card, bool isNew, ICardClickHandler clickHandler)
    {
        if (CardDisplay == null)
        {
            GameObject obj = Instantiate(_CardDisplayPrefab, transform);
            CardDisplay = obj.GetComponent<CardDisplay>();
        }

        CardDisplay.Display(card, clickHandler);
        CardDisplay.transform.localScale = Vector3.one;

        if (isNew)
        {
            Debug.Log($"[ShopCardSlot] Card {card.Data.Name} is new.");
        }
    }

    public void ResetSlot()
    {
        CardDisplay.Clear();
    }

    public int GetIndex() => _SlotIndex;
}
