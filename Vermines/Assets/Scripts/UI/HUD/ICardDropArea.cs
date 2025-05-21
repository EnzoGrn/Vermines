using UnityEngine;
using Vermines.UI.Card;

public interface ICardDropArea
{
    bool IsDropAllowed(CardDraggable card);
    void OnDropCard(CardDraggable card);
}