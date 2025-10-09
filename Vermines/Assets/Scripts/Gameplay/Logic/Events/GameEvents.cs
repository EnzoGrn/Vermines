using System;
using System.Collections.Generic;
using UnityEngine;
using Vermines;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Screen;
using Fusion;

public static class GameEvents
{
    // --- GENERAL ---
    public static TrackedEvent OnAttemptNextPhase = new("OnAttemptNextPhase");
    public static TrackedEvent<ICard> OnCardDrawn = new("OnCardDrawn");
    public static TrackedEvent<PhaseType> OnPhaseChanged = new("OnPhaseChanged");
    public static TrackedEvent<int> OnTurnChanged = new("OnTurnChanged");
    public static TrackedEvent OnPlayerInitialized = new("OnPlayerInitialized");
    public static TrackedEvent<PlayerData> OnPlayerUpdated = new("OnPlayerUpdated");
    public static TrackedEvent<NetworkDictionary<PlayerRef, PlayerData>> OnPlayersUpdated = new("OnPlayersUpdated");
    public static TrackedEvent<PlayerRef, PlayerRef> OnPlayerWin = new("OnPlayerWin");

    // --- CARD PLAYING ---
    public static TrackedEvent<ICard> OnCardPlayedRequested = new("OnCardPlayedRequested");
    public static TrackedEvent<ICard> OnCardPlayedRefused = new("OnCardPlayedRefused");
    public static TrackedEvent<ICard> OnCardPlayed = new("OnCardPlayed");

    // --- CARD SACRIFICE ---
    public static TrackedEvent<ICard> OnCardSacrificedRequested = new("OnCardSacrificedRequested");
    public static TrackedEvent<ICard> OnCardSacrifiedRefused = new("OnCardSacrifiedRefused");
    public static TrackedEvent<ICard> OnCardSacrified = new("OnCardSacrified");

    // --- CARD DISCARD ---
    public static TrackedEvent<ICard> OnCardDiscardedRefused = new("OnCardDiscardedRefused");
    public static TrackedEvent<ICard> OnCardDiscarded = new("OnCardDiscarded");

    // --- CARD UI EVENTS ---
    public static TrackedEvent<ICard, int> OnCardClicked = new("OnCardClicked");
    public static TrackedEvent<ShopType, int> OnCardClickedInShopWithSlotIndex = new("OnCardClickedInShopWithSlotIndex");

    // --- SHOP EVENTS ---
    public static Dictionary<ShopType, TrackedEvent<int, ICard>> OnShopsEvents = new();
    public static TrackedEvent<ShopType, int> OnCardPurchaseRequested = new("OnCardPurchaseRequested");
    public static TrackedEvent<ShopType, int> OnCardPurchased = new("OnCardPurchased");
    public static TrackedEvent<ICard, int> OnEquipmentCardPurchased = new("OnEquipmentCardPurchased");
    public static TrackedEvent<ShopType, int> OnShopCardReplaced = new("OnShopCardReplaced");
    public static TrackedEvent<ShopType, List<ShopCardEntry>> OnShopUpdated = new("OnShopUpdated");
    public static TrackedEvent<ShopType, Dictionary<int, ICard>> OnShopRefilled = new("OnShopRefilled");

    // --- CARD EFFECTS ---
    public static TrackedEvent<ICard> OnEffectSelectCard = new("OnEffectSelectCard");

    // --- DISCARD PILE ---
    public static TrackedEvent OnDiscardShuffled = new("OnDiscardShuffled");

    static GameEvents()
    {
        foreach (ShopType shopType in Enum.GetValues(typeof(ShopType)))
        {
            OnShopsEvents[shopType] = new TrackedEvent<int, ICard>($"OnShopsEvents[{shopType}]");
        }
    }

    // --- HELPERS / INVOKERS ---

    public static void InvokeOnDrawCard(ICard card)
    {
        OnCardDrawn.Invoke(card);
    }

    public static void InvokeOnPlayerUpdated(NetworkDictionary<PlayerRef, PlayerData> playerData)
    {
        PlayerRef playerRef = GameManager.Instance.PlayerTurnOrder[GameManager.Instance.CurrentPlayerIndex];
        if (playerRef == null) return;

        if (playerData.TryGet(playerRef, out PlayerData data))
        {
            OnPlayerUpdated.Invoke(data);
        }
    }

    public static void InvokeOnCardPurchaseRequested(ShopType shopType, int slotIndex)
    {
        Debug.Log($"[GameEvents] InvokeOnCardPurchaseRequested: {shopType}, Slot: {slotIndex}");
        OnCardPurchaseRequested.Invoke(shopType, slotIndex);
    }

    public static void InvokeOnPlayerWin(PlayerRef winnerRef, PlayerRef localPlayerRef)
    {
        Debug.Log($"[FinalAnimation]: Cardfamily -> {winnerRef}");
        OnPlayerWin.Invoke(winnerRef, localPlayerRef);
    }
}
