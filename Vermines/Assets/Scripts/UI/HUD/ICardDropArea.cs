using UnityEngine;
using Vermines.HUD.Card;

public interface ICardDropArea
{
    bool IsDropAllowed(CardDraggable card);
    void OnDropCard(CardDraggable card);
}