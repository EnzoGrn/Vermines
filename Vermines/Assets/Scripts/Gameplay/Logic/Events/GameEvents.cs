using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Vermines;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Screen;

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

    public static UnityEvent OnPlayerInitialized = new();
    public static UnityEvent<PlayerData> OnPlayerUpdated = new();

    public static UnityEvent<ShopType, List<ShopCardEntry>> OnShopUpdated = new();
    public static UnityEvent<ShopType, Dictionary<int, ICard>> OnShopRefilled = new();

    static GameEvents()
    {
        foreach (ShopType shopType in Enum.GetValues(typeof(ShopType)))
        {
            OnShopsEvents[shopType] = new();
        }
    }

    public static void InvokeOnDrawCard(ICard card)
    {
        OnCardDrawn.Invoke(card);
        return;
    }

    public static void InvokeOnPlayerUpdated(NetworkDictionary<PlayerRef, PlayerData> playerData)
    {
        PlayerRef playerRef = GameManager.Instance.PlayerTurnOrder[GameManager.Instance.CurrentPlayerIndex];
        if (playerRef == null)
        {
            return;
        }
        if (playerData.TryGet(playerRef, out PlayerData data) == false)
        {
            return;
        }
        OnPlayerUpdated.Invoke(data);
        return;
    }

    public static void InvokeOnCardPurchaseRequested(ShopType shopType, int slotIndex)
    {
        Debug.Log($"[GameEvents] InvokeOnCardPurchaseRequested: {shopType}, Slot: {slotIndex}");
        OnCardPurchaseRequested.Invoke(shopType, slotIndex);
        return;
    }
}
