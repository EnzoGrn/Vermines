using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;

public static class GameEvents
{
    public static UnityEvent OnAttemptNextPhase = new ();
    public static UnityEvent<ICard> OnDrawCard = new ();
    public static Dictionary<ShopType, UnityEvent<int, ICard>> OnShopsEvents = new();
    public static UnityEvent<ShopType, int> OnCardBought = new();
    public static UnityEvent<int> OnDiscard = new();
    public static UnityEvent<int> OnCardPlayed = new();
    public static UnityEvent<int> OnCardSacrified = new();

    static GameEvents()
    {
        foreach (ShopType shopType in Enum.GetValues(typeof(ShopType)))
        {
            OnShopsEvents[shopType] = new();
        }
    }

    public static void AttemptNextPhase()
    {
        OnAttemptNextPhase.Invoke();
        return;
    }

    public static void InvokeOnDrawCard(ICard card)
    {
        OnDrawCard.Invoke(card);
        return;
    }

    public static void InvokeOnDiscard(int cardId)
    {
        OnDiscard.Invoke(cardId);
        return;
    }

    public static void InvokeOnCardPlayed(int cardId)
    {
        OnCardPlayed.Invoke(cardId);
        return;
    }
}
