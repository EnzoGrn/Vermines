using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases;
using Vermines.UI;
using Vermines.UI.Popup;

public class TableCardClickHandler : ICardClickHandler
{
    private readonly int _slotId;

    public TableCardClickHandler(int slotId)
    {
        _slotId = slotId;
    }

    public void OnCardClicked(ICard card)
    {
        GameEvents.OnCardClicked.Invoke(card);
    }
}
