using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.ShopSystem.Enumerations;

public static class GameEvents
{
    public static UnityEvent OnAttemptNextPhase = new ();
    public static UnityEvent<ICard> OnCardDrawn = new();
    public static Dictionary<ShopType, UnityEvent<int, ICard>> OnShopsEvents = new();
    public static UnityEvent<ShopType, int> OnCardPurchaseRequested = new();
    public static UnityEvent<ShopType, int> OnCardPurchased = new();
    public static UnityEvent<ShopType, int> OnShopCardReplaced = new();

    #region DISCARD
    public static UnityEvent<ICard> OnCardDiscardedRequested = new();
    public static UnityEvent<ICard> OnCardDiscardedRequestedNoEffect = new();
    public static UnityEvent<ICard> OnCardDiscardedRefused = new();
    public static UnityEvent<ICard> OnCardDiscarded = new();
    #endregion

    public static UnityEvent<ICard> OnCardPlayedRequested = new();
    public static UnityEvent<ICard> OnCardPlayedRefused = new();
    public static UnityEvent<ICard> OnCardPlayed = new();

    public static UnityEvent<ICard> OnCardSacrificedRequested = new();
    public static UnityEvent<ICard> OnCardSacrifiedRefused = new();
    public static UnityEvent<ICard> OnCardSacrified = new();

    public static UnityEvent<PhaseType> OnPhaseChanged = new();
    public static UnityEvent<int> OnTurnChanged = new();

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
        OnCardDrawn.Invoke(card);
        return;
    }
}
