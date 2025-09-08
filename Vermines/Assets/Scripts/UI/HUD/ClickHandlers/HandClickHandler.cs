using Vermines.CardSystem.Elements;
using UnityEngine;

public class HandClickHandler : ICardClickHandler
{
    public HandClickHandler()
    {
    }

    public void OnCardClicked(ICard card)
    {
        Debug.Log("YAAAAAAAAAAAAAAAAAa");
        GameEvents.OnCardClicked.Invoke(card, -1);
    }
}
