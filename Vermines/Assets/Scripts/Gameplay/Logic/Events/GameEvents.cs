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
    // --- Initialize ---
    public static readonly TrackedEvent OnGameInitialized = new("OnGameInitialized");

    // --- GENERAL ---
    public static readonly TrackedEvent OnAttemptNextPhase = new("OnAttemptNextPhase");
    public static readonly TrackedEvent<ICard> OnCardDrawn = new("OnCardDrawn");
    public static readonly TrackedEvent<PhaseType> OnPhaseChanged = new("OnPhaseChanged");
    public static readonly TrackedEvent<int> OnTurnChanged = new("OnTurnChanged");
    public static readonly TrackedEvent OnPlayerInitialized = new("OnPlayerInitialized");
    public static readonly TrackedEvent<PlayerController> OnPlayerUpdated = new("OnPlayerUpdated");
    public static readonly TrackedEvent<PlayerRef, PlayerRef> OnPlayerWin = new("OnPlayerWin");

    // --- CARD PLAYING ---
    public static readonly TrackedEvent<ICard> OnCardPlayedRequested = new("OnCardPlayedRequested");
    public static readonly TrackedEvent<ICard> OnCardPlayedRefused = new("OnCardPlayedRefused");
    public static readonly TrackedEvent<ICard> OnCardPlayed = new("OnCardPlayed");

    // --- CARD SACRIFICE ---
    // NOTE: OnCardSacrifiedRefused / OnCardSacrified contiennent une faute
    // ("Sacrifi(c)ed") incohérente avec OnCardSacrificedRequested juste au-dessus.
    // Pas corrigé ici : le rename touche 12 fichiers (RPC, effets, phases) et doit
    // se faire dans un commit dédié isolé, pas mélangé à ce nettoyage rapide.
    public static readonly TrackedEvent<ICard> OnCardSacrificedRequested = new("OnCardSacrificedRequested");
    public static readonly TrackedEvent<ICard> OnCardSacrifiedRefused = new("OnCardSacrifiedRefused");
    public static readonly TrackedEvent<ICard> OnCardSacrified = new("OnCardSacrified");

    // --- CARD RECYCLING ---
    public static readonly TrackedEvent<ICard> OnCardRecycled = new("OnCardRecycled");

    // --- CARD DISCARD ---
    public static readonly TrackedEvent<ICard> OnCardDiscardedRefused = new("OnCardDiscardedRefused");
    public static readonly TrackedEvent<ICard> OnCardDiscarded = new("OnCardDiscarded");

    // --- CARD UI EVENTS ---
    public static readonly TrackedEvent<ICard, int> OnCardClicked = new("OnCardClicked");
    public static readonly TrackedEvent<ShopType, int> OnCardClickedInShopWithSlotIndex = new("OnCardClickedInShopWithSlotIndex");

    // --- SHOP EVENTS ---
    public static readonly Dictionary<ShopType, TrackedEvent<int, ICard>> OnShopsEvents = new();
    public static readonly TrackedEvent<ShopType, int> OnCardPurchaseRequested = new("OnCardPurchaseRequested");
    public static readonly TrackedEvent<ShopType, int> OnCardPurchased = new("OnCardPurchased");
    public static readonly TrackedEvent<ICard> OnEquipmentCardPurchased = new("OnEquipmentCardPurchased");
    public static readonly TrackedEvent<ShopType, int> OnShopCardReplaced = new("OnShopCardReplaced");
    public static readonly TrackedEvent<ShopType, List<ShopCardEntry>> OnShopUpdated = new("OnShopUpdated");
    public static readonly TrackedEvent<ShopType, Dictionary<int, ICard>> OnShopRefilled = new("OnShopRefilled");

    // --- CARD EFFECTS ---
    public static readonly TrackedEvent<ICard> OnEffectSelectCard = new("OnEffectSelectCard");
    public static readonly TrackedEvent<ICard> OnCardReborned = new("OnCardReborned");

    // --- DISCARD PILE ---
    public static readonly TrackedEvent OnDiscardShuffled = new("OnDiscardShuffled");

    // --- TABLE ---
    public static readonly TrackedEvent<int> OnPartisanAreaSlotChanged = new("OnPartisanAreaSlotChanged");

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

    public static void InvokeOnCardPurchaseRequested(ShopType shopType, int cardId)
    {
        OnCardPurchaseRequested.Invoke(shopType, cardId);
    }

    public static void InvokeOnPlayerWin(PlayerRef winnerRef, PlayerRef localPlayerRef)
    {
        OnPlayerWin.Invoke(winnerRef, localPlayerRef);
    }
}
