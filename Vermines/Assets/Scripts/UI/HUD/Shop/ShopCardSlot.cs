using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;

public class ShopCardSlot : MonoBehaviour
{
    [SerializeField] private GameObject _CardDisplayPrefab;
    private int _SlotIndex;
    private CardDisplay _CardDisplayInstance;

    public void SetIndex(int index)
    {
        _SlotIndex = index;
    }

    public void Init(ICard card, bool isNew, ICardClickHandler clickHandler)
    {
        if (_CardDisplayInstance == null)
        {
            GameObject obj = Instantiate(_CardDisplayPrefab, this.transform);
            _CardDisplayInstance = obj.GetComponent<CardDisplay>();
        }

        _CardDisplayInstance.Display(card, clickHandler);
        _CardDisplayInstance.transform.localScale = Vector3.one;

        if (isNew)
        {
            Debug.Log($"[ShopCardSlot] Card {card.Data.Name} is new.");
        }
    }

    public void ResetSlot()
    {
        _CardDisplayInstance.Clear();
    }

    public int GetIndex() => _SlotIndex;
}
