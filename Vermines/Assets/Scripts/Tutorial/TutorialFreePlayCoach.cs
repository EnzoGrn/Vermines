using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using Vermines.CardSystem.Elements;
    using Vermines.Core;
    using Vermines.Player;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;

    /// <summary>
    /// What a free-play lesson does around the player (VerminesTutorialContent.FreePlayRules):
    /// hints after a forbidden move, and strong cards offered in the court at the right moment.
    /// </summary>
    public sealed class TutorialFreePlayRules {

        public List<TutorialHint> hints = new();

        /// <summary>
        /// Kept out of the court and the decks at the start, then offered one by one, in this order.
        /// </summary>
        public List<TutorialCourtOffer> offers = new();

        /// <summary>
        /// Player turns between two offers.
        /// </summary>
        public int turnsBetweenOffers = 2;

        /// <summary>
        /// Cards worth a word of advice the first time the player sees them in the court (no forced purchase).
        /// </summary>
        public List<TutorialCardTip> tips = new();

        /// <summary>
        /// The lesson is only passed by winning the match: a lost match plays this interlude, then the lesson fails (empty = it fails at once).
        /// Null or empty with requireWin false: the end of the match is not watched.
        /// </summary>
        public bool   requireWin;
        public string defeatInterludeId;
    }

    public sealed class TutorialCardTip {

        public string cardId;

        public string interludeId;
    }

    public sealed class TutorialHint {

        /// <summary>
        /// A key of the game's errors (Shop_Buy_NotEnoughEloquence...), a key raised by the tutorial for the refusals the game makes silently
        /// (Table_Play_ToolOnTable, Table_Play_TableFull), or "court_incomplete" (the player opened a court with missing cards).
        /// </summary>
        public string trigger;

        /// <summary>
        /// Interlude of the lesson, written in the table with the lesson "lecon_04/&lt;id&gt;".
        /// </summary>
        public string interludeId;

        /// <summary>
        /// Player turns before the same hint can show again (1 = once per turn).
        /// </summary>
        public int cooldownTurns = 1;

        /// <summary>
        /// Shown the first time only.
        /// </summary>
        public bool once;
    }

    public enum TutorialOfferEffect {
        /// <summary>A power that works at the start of the turn (Play effect): shown at the next turn of the player.</summary>
        NextTurnStart,
        /// <summary>A power that works when a card is added to the court (Cruce Signatus).</summary>
        CourtCardAdded
    }

    public sealed class TutorialCourtOffer {

        public string cardId;

        /// <summary>The card arrives in the court, the player is guided to buy it.</summary>
        public string offerInterludeId;

        /// <summary>The player placed it on the table: what its power will do.</summary>
        public string placedInterludeId;

        /// <summary>Its power just worked.</summary>
        public string effectInterludeId;

        public TutorialOfferEffect effectMoment;
    }

    /// <summary>
    /// Runs the free-play rules of the current lesson, from the tutorial events of the match.
    /// Created by the bridge when the lesson starts, before the initializer reserves the offered cards.
    /// </summary>
    public sealed class TutorialFreePlayCoach {

        #region Attributes

        public static TutorialFreePlayCoach Current { get; private set; }

        /// <summary>
        /// Slots of the court at the start of a match (3 of level 1, 2 of level 2), see TutorialGameInitializer.InitializeCourtyard.
        /// </summary>
        private const int CourtSlots = 5;

        private readonly TutorialFreePlayRules _Rules;

        private readonly Dictionary<string, ICard> _Reserved = new();

        /// <summary>
        /// Last player turn each hint was shown.
        /// </summary>
        private readonly Dictionary<string, int> _HintTurns = new();

        private int _PlayerTurn;

        private int _NextOffer;
        private int _LastOfferTurn = -1000;

        /// <summary>
        /// Offered cards the player bought, then placed, then saw working.
        /// </summary>
        private readonly HashSet<string> _Bought = new();
        private readonly HashSet<string> _Placed = new();
        private readonly HashSet<string> _EffectShown = new();

        private bool _Offering;

        private readonly HashSet<string> _TipsShown = new();

        /// <summary>
        /// "eloquence_max": the player's eloquence reached the game's cap (GameplayMode.MaxEloquence), what they earn beyond is lost.
        /// </summary>
        public const string EloquenceMaxTrigger = "eloquence_max";

        public const string CourtIncompleteTrigger = "court_incomplete";

        /// <summary>
        /// "table_filled": the player just placed a partisan in the last free slot of their table.
        /// </summary>
        public const string TableFilledTrigger = "table_filled";

        /// <summary>
        /// "end_game_player" / "end_game_rival": the player, or an opponent, is the first to reach the souls limit (the last turns start).
        /// </summary>
        public const string EndGamePlayerTrigger = "end_game_player";
        public const string EndGameRivalTrigger  = "end_game_rival";

        private bool _EndGameAnnounced;

        #endregion

        #region Lifecycle

        private TutorialFreePlayCoach(TutorialFreePlayRules rules)
        {
            _Rules = rules;
        }

        /// <summary>
        /// Starts the coach of a lesson; a lesson without free-play rules has none.
        /// </summary>
        public static void Begin(TutorialFreePlayRules rules)
        {
            End();

            if (rules == null)
                return;
            Current = new TutorialFreePlayCoach(rules);

            TutorialEventBus.OnEvent += Current.OnTutorialEvent;
            GameEvents.OnShopRefilled.AddListener(Current.OnShopRefilled);
            GameEvents.OnPlayerUpdated.AddListener(Current.OnPlayerUpdated);

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.InterludeFinished += Current.OnInterludeFinished;
        }

        public static void End()
        {
            if (Current == null)
                return;
            TutorialEventBus.OnEvent -= Current.OnTutorialEvent;
            GameEvents.OnShopRefilled.RemoveListener(Current.OnShopRefilled);
            GameEvents.OnPlayerUpdated.RemoveListener(Current.OnPlayerUpdated);

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.InterludeFinished -= Current.OnInterludeFinished;
            Current = null;
        }

        /// <summary>
        /// Called by the initializer before it fills the court: the offered cards are taken from the card set, so nobody gets them before their offer.
        /// </summary>
        public void ReserveOfferedCards(Func<string, ICard> take)
        {
            _Reserved.Clear();

            foreach (TutorialCourtOffer offer in _Rules.offers) {
                ICard card = take(offer.cardId);

                if (card != null)
                    _Reserved[TutorialCardIds.Key(offer.cardId)] = card;
            }
        }

        #endregion

        #region Helpers

        private static GameplayMode Gameplay => PlayerController.Local != null ? PlayerController.Local.Context?.GameplayMode : null;

        private static PlayerController Human
        {
            get
            {
                GameplayMode gameplay = Gameplay;

                return gameplay != null ? gameplay.Context.NetworkGame.GetPlayer(TutorialMatch.HumanRef) : null;
            }
        }

        private static CourtyardSection Court
        {
            get
            {
                GameplayMode gameplay = Gameplay;

                if (gameplay == null || !gameplay.Shop.Sections.TryGetValue(ShopType.Courtyard, out var section))
                    return null;
                return section as CourtyardSection;
            }
        }

        private static bool IsHumanTurn()
        {
            GameplayMode gameplay = Gameplay;

            return gameplay != null && gameplay.Object != null && TutorialMatch.IndexOf(gameplay.CurrentPlayer) == 0;
        }

        private static bool IsInterludeRunning => TutorialManager.Instance != null && TutorialManager.Instance.RunningInterlude != null;

        private static bool PlayInterlude(string interludeId)
        {
            TutorialManager manager = TutorialManager.Instance;

            return manager != null && !string.IsNullOrEmpty(interludeId) && manager.PlayInterlude(interludeId);
        }

        private TutorialCourtOffer FindOffer(string cardId)
        {
            return _Rules.offers.FirstOrDefault(offer => TutorialCardIds.Key(offer.cardId) == cardId);
        }

        #endregion

        #region Events

        private void OnTutorialEvent(TutorialEvent e)
        {
            switch (e.Id) {
                case TutorialGameEventRelay.ActionRefused:
                    // The errors of the opponents' actions are not the player's.
                    if (IsHumanTurn())
                        ShowHint(e.Arg);

                    break;
                case TutorialGameEventRelay.ShopOpened:
                    // Not during an interlude (an offer opens the court): the next visit will show them.
                    if (e.Arg == "courtyard" && !IsInterludeRunning) {
                        CheckCourt();
                        ShowCardTips();
                    }
                    break;
                case TutorialEvents.EloquenceChanged:
                    GameplayMode gameplay = Gameplay;

                    if (gameplay != null && e.Value >= gameplay.MaxEloquence)
                        ShowHint(EloquenceMaxTrigger);
                    break;
                case TutorialEvents.MatchEnded:
                    OnMatchEnded(e.Arg == "win");

                    break;
                case TutorialEvents.TurnStarted:
                    if (e.Arg == "0")
                        OnPlayerTurnStarted();
                    break;
                case TutorialGameEventRelay.PhaseChanged:
                    if (e.Arg == "Action" && IsHumanTurn())
                        TryOffer();
                    break;
                case TutorialEvents.CardBought:
                    OnCardBought(e.Arg);

                    break;
                case TutorialEvents.PartisanPlaced:
                    OnPartisanPlaced(e.Arg);

                    break;
            }
        }

        private void OnPlayerTurnStarted()
        {
            _PlayerTurn++;

            // The powers that work at the start of the turn: shown the first turn the card is on the table.
            PlayerController human = Human;

            if (human == null)
                return;
            foreach (string cardId in _Placed) {
                TutorialCourtOffer offer = FindOffer(cardId);

                if (offer == null || offer.effectMoment != TutorialOfferEffect.NextTurnStart || _EffectShown.Contains(cardId))
                    continue;
                if (human.PlayedCards.Any(card => TutorialCardIds.Is(card, cardId)) && PlayInterlude(offer.effectInterludeId))
                    _EffectShown.Add(cardId);
            }
        }

        private void OnCardBought(string cardId)
        {
            if (FindOffer(cardId) != null)
                _Bought.Add(cardId);
        }

        private void OnPartisanPlaced(string cardId)
        {
            PlayerController human = Human;

            if (human != null && human.PlayedCards.Count >= human.Statistics.NumberOfSlotInTable)
                ShowHint(TableFilledTrigger);
            TutorialCourtOffer offer = FindOffer(cardId);

            if (offer == null || !_Bought.Contains(cardId) || !_Placed.Add(cardId))
                return;
            PlayInterlude(offer.placedInterludeId);
        }

        /// <summary>
        /// A card added to the court during the player's turn, by one of their cards (not by an offer).
        /// </summary>
        private void OnShopRefilled(ShopType shopType, Dictionary<int, ICard> cards)
        {
            if (shopType != ShopType.Courtyard || _Offering || !IsHumanTurn())
                return;
            PlayerController human = Human;

            if (human == null)
                return;
            foreach (string cardId in _Placed) {
                TutorialCourtOffer offer = FindOffer(cardId);

                if (offer == null || offer.effectMoment != TutorialOfferEffect.CourtCardAdded || _EffectShown.Contains(cardId))
                    continue;
                if (human.PlayedCards.Any(card => TutorialCardIds.Is(card, cardId)) && PlayInterlude(offer.effectInterludeId))
                    _EffectShown.Add(cardId);
            }
        }

        /// <summary>
        /// The first player to reach the souls limit (GameplayMode.SoulsLimit) starts the end of the match: explained once.
        /// </summary>
        private void OnPlayerUpdated(PlayerController player)
        {
            GameplayMode gameplay = Gameplay;

            if (_EndGameAnnounced || gameplay == null || player == null || player.Object == null || player.Statistics.Souls < gameplay.SoulsLimit)
                return;
            _EndGameAnnounced = true;

            ShowHint(TutorialMatch.IndexOf(player.Object.InputAuthority) == 0 ? EndGamePlayerTrigger : EndGameRivalTrigger);
        }

        #endregion

        #region Hints

        private void ShowHint(string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return;
            foreach (TutorialHint hint in _Rules.hints) {
                if (!string.Equals(hint.trigger, trigger, StringComparison.OrdinalIgnoreCase))
                    continue;
                // Hints sharing an interlude share their cooldown.
                if (_HintTurns.TryGetValue(hint.interludeId, out int turn) && (hint.once || _PlayerTurn - turn < Mathf.Max(1, hint.cooldownTurns)))
                    return;
                if (PlayInterlude(hint.interludeId))
                    _HintTurns[hint.interludeId] = _PlayerTurn;
                return;
            }
        }

        private void CheckCourt()
        {
            CourtyardSection court = Court;

            if (court == null)
                return;
            int shown = court.AvailableCards.Values.Count(card => card != null);

            if (shown < CourtSlots)
                ShowHint(CourtIncompleteTrigger);
        }

        /// <summary>
        /// Advice on a card of the court, the first time the player sees it there.
        /// </summary>
        private void ShowCardTips()
        {
            CourtyardSection court = Court;

            if (court == null)
                return;
            foreach (TutorialCardTip tip in _Rules.tips) {
                string cardId = TutorialCardIds.Key(tip.cardId);

                if (_TipsShown.Contains(cardId) || !court.AvailableCards.Values.Any(card => card != null && TutorialCardIds.Is(card, cardId)))
                    continue;
                if (PlayInterlude(tip.interludeId))
                    _TipsShown.Add(cardId);
            }
        }

        #endregion

        #region End of the match

        private bool _Lost;

        /// <summary>
        /// A lesson to win (requireWin): a win validates it at once, since the end of the match can bring the game back to the menu before the last step;
        /// a loss explains it (defeatInterludeId), then fails the lesson.
        /// </summary>
        private void OnMatchEnded(bool won)
        {
            TutorialManager manager = TutorialManager.Instance;

            if (!_Rules.requireWin || manager == null || manager.CurrentLesson == null)
                return;
            if (won) {
                manager.Progress?.MarkCompleted(manager.CurrentLesson.lessonId);

                return;
            }

            _Lost = true;

            if (!PlayInterlude(_Rules.defeatInterludeId))
                manager.FailCurrentLesson();
        }

        private void OnInterludeFinished(string interludeId)
        {
            if (_Lost && interludeId == _Rules.defeatInterludeId && TutorialManager.Instance != null)
                TutorialManager.Instance.FailCurrentLesson();
        }

        #endregion

        #region Offers

        /// <summary>
        /// Start of the player's Action phase: the next strong card arrives in the court if the player can buy it now.
        /// </summary>
        private void TryOffer()
        {
            if (_NextOffer >= _Rules.offers.Count || _PlayerTurn - _LastOfferTurn < Mathf.Max(1, _Rules.turnsBetweenOffers))
                return;
            TutorialCourtOffer offer = _Rules.offers[_NextOffer];

            if (!_Reserved.TryGetValue(TutorialCardIds.Key(offer.cardId), out ICard card)) {
                Debug.LogWarning($"[Tutorial] Offer \"{offer.cardId}\": card not reserved, offer skipped.");

                _NextOffer++;

                return;
            }

            PlayerController human = Human;
            GameplayMode  gameplay = Gameplay;

            if (human == null || gameplay == null || !gameplay.HasStateAuthority || human.Statistics.Eloquence < card.Data.CurrentEloquence)
                return;
            if (!PutInCourt(gameplay, card))
                return;
            _NextOffer++;
            _LastOfferTurn = _PlayerTurn;

            _Reserved.Remove(TutorialCardIds.Key(offer.cardId));

            PlayInterlude(offer.offerInterludeId);
        }

        /// <summary>
        /// The card goes on top of its pile and is revealed in a free slot.
        /// In a full court, it takes the place of the last card, which goes back under its pile.
        /// </summary>
        private bool PutInCourt(GameplayMode gameplay, ICard card)
        {
            CourtyardSection court = Court;

            if (court == null)
                return false;
            int         level = card.Data.Level == 2 ? 2 : 1;
            List<ICard>  pile = level == 2 ? court.Deck2 : court.Deck1;

            _Offering = true;

            if (court.AvailableCards.Values.Any(c => c == null) || court.AvailableCards.Count < CourtSlots) {
                pile.Insert(0, card);
                court.AddCard(level);
            } else {
                int     last = court.AvailableCards.Keys.Max();
                ICard replaced = court.AvailableCards[last];

                court.AvailableCards[last] = card;

                (replaced.Data.Level == 2 ? court.Deck2 : court.Deck1).Add(replaced);
            }

            gameplay.RPC_InitializeShop(ShopType.Courtyard, gameplay.Shop.SerializeSection(ShopType.Courtyard));

            _Offering = false;

            Debug.Log($"[Tutorial] Offer: {card.Data.Name} added to the court.");

            return true;
        }

        #endregion
    }
}
