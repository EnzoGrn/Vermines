using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Tutorial.Integration {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core;
    using Vermines.Gameplay.Errors;
    using Vermines.Gameplay.Phases;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    /// <summary>
    /// Small entry points called from the game code.
    /// Everything returns the normal behaviour when no tutorial lesson is running.
    /// </summary>
    public static class TutorialGameHooks {

        /// <summary>
        /// The phase button (GameplayUIMain).
        /// In the table: "phase_suivante" (any phase), "phase_suivante:Gain"... or "fin_tour".
        /// </summary>
        public static bool AllowNextPhase(PhaseType currentPhase)
        {
            if (!TutorialMatch.IsActive || !TutorialInputGate.IsRestricted)
                return true;
            string phase = currentPhase.ToString();

            // "fin_tour" in the table also allows the button during the Action phase (it ends the turn).
            foreach (AllowedAction allowed in TutorialInputGate.Allowed) {
                if (allowed.Allows(GameActionType.NextPhase, phase))
                    return true;
                if (currentPhase == PhaseType.Action && allowed.Allows(GameActionType.EndTurn, null))
                    return true;
            }

            // Not allowed: lets the gate tell the UI (the bubble shakes).
            return TutorialInputGate.IsAllowed(GameActionType.NextPhase, phase);
        }

        /// <summary>
        /// Drop of a hand card on a table slot (play) or on the discard slot.
        /// Checked by the drop handlers before their optimistic display, so a refused card simply goes back to the hand.
        /// </summary>
        public static bool AllowCardDrop(ICard card, bool discard)
        {
            if (!TutorialMatch.IsActive || card == null)
                return true;
            return TutorialInputGate.IsAllowed(discard ? GameActionType.DiscardCard : GameActionType.PlayCard, TutorialCardIds.Key(card));
        }

        /// <summary>
        /// Effect chosen in GameplayUIChoiceEffect.
        /// In the table: "choisir:pugionem_sacrificialem#0" (first effect of the card) or "choisir:pugionem_sacrificialem" (any effect).
        /// </summary>
        public static bool AllowEffectChoice(ICard card, int effectIndex)
        {
            if (!TutorialMatch.IsActive || card == null)
                return true;
            string key = TutorialCardIds.Key(card);

            if (TutorialInputGate.WouldAllow(GameActionType.ChooseEffect, key))
                return true;
            return TutorialInputGate.IsAllowed(GameActionType.ChooseEffect, $"{key}#{effectIndex}");
        }

        /// <summary>
        /// A card the drop handlers send back to the hand without asking the game (so the game shows nothing):
        /// published as "action_refused" with a key of the same style as the game's errors, so a lesson can explain why.
        ///     Table_Discard_CardNotDiscardable  a partisan without discard power dropped on the discard;
        ///     Table_Play_ToolOnTable            a tool dropped on a table slot;
        ///     Table_Play_TableFull              a partisan dropped while every table slot is taken.
        /// </summary>
        public static void ReportRefusedDrop(ICard card, bool discard)
        {
            if (!TutorialMatch.IsActive || card?.Data == null)
                return;
            string key = null;

            if (discard) {
                if (!card.Data.CanBeDiscard())
                    key = "Table_Discard_CardNotDiscardable";
            } else if (card.Data.Type != CardType.Partisan) {
                key = "Table_Play_ToolOnTable";
            } else {
                PlayerController player = PlayerController.Local;

                if (player != null && player.PlayedCards.Count >= player.Statistics.NumberOfSlotInTable)
                    key = "Table_Play_TableFull";
            }

            if (key != null)
                TutorialEventBus.Publish(TutorialGameEventRelay.ActionRefused, key);
        }

        /// <summary>
        /// Recycle screen: the click handler of a hand card that cannot be recycled (null outside a lesson, as before).
        /// A tool that cannot be recycled (the Pugionem Sacrificialem) publishes "action_refused Recycle_CardNotRecyclable" when clicked.
        /// </summary>
        public static ICardClickHandler RefusedRecycleClickHandler(ICard card)
        {
            if (!TutorialMatch.IsActive || card?.Data == null || card.Data.Type != CardType.Tools)
                return null;
            return new RefusedRecycleClick();
        }

        private sealed class RefusedRecycleClick : ICardClickHandler {

            public void OnCardClicked(ICard card)
            {
                TutorialEventBus.Publish(TutorialGameEventRelay.ActionRefused, "Recycle_CardNotRecyclable");
            }
        }

        /// <summary>
        /// The lessons teach the sacrifice with the dagger: during a lesson, the Sacrifice phase ends by itself (SacrificePhase.Run).
        /// </summary>
        public static bool SkipSacrificePhase => TutorialMatch.IsActive;
    }

    /// <summary>
    /// Translates the game's GameEvents into TutorialEventBus events,
    /// the vocabulary used by the steps ("wait for card_played operarius").
    /// 
    /// Actions of the human use the standard IDs (TutorialEvents.*);
    /// the same actions of a scripted opponent are published with the "opponent_" prefix,
    /// so that a step waiting for the player is never validated by the opponent.
    /// </summary>
    public sealed class TutorialGameEventRelay {

        public const string PhaseChanged   = "phase_changed";
        public const string EffectPrompt   = "effect_prompt";
        public const string ActionRefused  = "action_refused";
        public const string CameraLocation = "camera_location";
        public const string CardClicked    = "card_clicked";
        public const string ShopOpened     = "shop_opened";
        public const string ShopClosed     = "shop_closed";
        public const string SoulsReached   = "souls_reached";
        public const string OpponentPrefix = "opponent_";

        /// <summary>
        /// "souls_reached" is published for every multiple of this value the player's souls go past (arg = the threshold, "25").
        /// </summary>
        private const int SoulsStep = 5;

        private bool _Subscribed;

        private int _LastEloquence = int.MinValue;
        private int _LastSouls     = int.MinValue;

        private readonly Dictionary<int, int> _TurnCounts = new();

        /// <summary>
        /// Player and phase of the last processed phase, to know when a new turn starts.
        /// </summary>
        private int       _TurnPlayer = -1;
        private PhaseType _TurnPhase  = PhaseType.None;

        private CamManager _Camera;

        public int TurnCount(int playerIndex) => _TurnCounts.TryGetValue(playerIndex, out int count) ? count : 0;

        public void Subscribe()
        {
            if (_Subscribed)
                return;
            _Subscribed    = true;
            _LastEloquence = int.MinValue;
            _LastSouls     = int.MinValue;

            _TurnCounts.Clear();

            _TurnPlayer = -1;
            _TurnPhase  = PhaseType.None;

            GameEvents.OnCardDrawn.AddListener(OnCardDrawn);
            GameEvents.OnCardPlayed.AddListener(OnCardPlayed);
            GameEvents.OnCardPurchased.AddListener(OnCardPurchased);
            GameEvents.OnCardSacrificed.AddListener(OnCardSacrificed);
            GameEvents.OnCardRecycled.AddListener(OnCardRecycled);
            GameEvents.OnCardDiscarded.AddListener(OnCardDiscarded);
            GameEvents.OnPhaseChanged.AddListener(OnPhaseChanged);
            GameEvents.OnPlayerUpdated.AddListener(OnPlayerUpdated);
            GameEvents.OnPlayerWin.AddListener(OnPlayerWin);
            GameEvents.OnEffectPromptRequested.AddListener(OnEffectPromptRequested);
            GameEvents.OnActionRefused.AddListener(OnActionRefused);
            GameEvents.OnDiscardShuffled.AddListener(OnDiscardShuffled);
            GameEvents.OnCardClicked.AddListener(OnCardClicked);

            PhaseManager.PhaseProcessed += OnPhaseProcessed;
        }

        public void Unsubscribe()
        {
            if (!_Subscribed)
                return;
            _Subscribed = false;

            GameEvents.OnCardDrawn.RemoveListener(OnCardDrawn);
            GameEvents.OnCardPlayed.RemoveListener(OnCardPlayed);
            GameEvents.OnCardPurchased.RemoveListener(OnCardPurchased);
            GameEvents.OnCardSacrificed.RemoveListener(OnCardSacrificed);
            GameEvents.OnCardRecycled.RemoveListener(OnCardRecycled);
            GameEvents.OnCardDiscarded.RemoveListener(OnCardDiscarded);
            GameEvents.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            GameEvents.OnPlayerUpdated.RemoveListener(OnPlayerUpdated);
            GameEvents.OnPlayerWin.RemoveListener(OnPlayerWin);
            GameEvents.OnEffectPromptRequested.RemoveListener(OnEffectPromptRequested);
            GameEvents.OnActionRefused.RemoveListener(OnActionRefused);
            GameEvents.OnDiscardShuffled.RemoveListener(OnDiscardShuffled);
            GameEvents.OnCardClicked.RemoveListener(OnCardClicked);

            PhaseManager.PhaseProcessed -= OnPhaseProcessed;

            if (_Camera != null)
                _Camera.OnCamLocationChanged.RemoveListener(OnCameraLocationChanged);
            _Camera = null;
        }

        /// <summary>
        /// The camera only exists once the game scene is loaded: the bridge calls this while the lesson runs.
        /// </summary>
        public void BindCamera()
        {
            if (!_Subscribed || _Camera != null)
                return;
            _Camera = UnityEngine.Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            if (_Camera != null)
                _Camera.OnCamLocationChanged.AddListener(OnCameraLocationChanged);
        }

        #region Helpers

        private static GameplayMode Gameplay => PlayerController.Local != null ? PlayerController.Local.Context?.GameplayMode : null;

        /// <summary>
        /// Index (0 = human) of the player whose turn it is, -1 if unknown.
        /// </summary>
        private static int CurrentIndex()
        {
            GameplayMode gameplay = Gameplay;

            if (gameplay == null || gameplay.Object == null)
                return -1;
            return TutorialMatch.IndexOf(gameplay.CurrentPlayer);
        }

        private static void Publish(string id, int playerIndex, string arg = null, int value = 0)
        {
            TutorialEventBus.Publish(playerIndex > 0 ? OpponentPrefix + id : id, arg, value);
        }

        #endregion

        #region Events

        private void OnPhaseProcessed(PhaseType phase, PlayerRef player)
        {
            int index = TutorialMatch.IndexOf(player);

            if (index < 0)
                return;
            // The phase order comes from the gameplay prefab (StandartGameplay has no Sacrifice phase, turns start with Gain):
            // a turn starts when another player plays, or when the phases start over.
            bool turnStarted = index != _TurnPlayer || phase <= _TurnPhase;

            _TurnPlayer = index;
            _TurnPhase  = phase;

            if (turnStarted) {
                _TurnCounts[index] = TurnCount(index) + 1;

                // Arg = player index (as in TutorialEvents), not prefixed: steps can wait for "turn_started 1".
                TutorialEventBus.Publish(TutorialEvents.TurnStarted, index.ToString(), TurnCount(index));
            }
            if (phase == PhaseType.Resolution) {
                TutorialEventBus.Publish(TutorialEvents.TurnEnded, index.ToString(), TurnCount(index));
            }
        }

        private void OnCardDrawn(ICard card)
        {
            Publish(TutorialEvents.CardDrawn, 0, TutorialCardIds.Key(card));
        }

        private void OnCardPlayed(ICard card)
        {
            int index = CurrentIndex();

            Publish(TutorialEvents.CardPlayed, index, TutorialCardIds.Key(card), index);

            if (card?.Data != null && card.Data.Type == CardType.Partisan)
                Publish(TutorialEvents.PartisanPlaced, index, TutorialCardIds.Key(card), index);
        }

        private void OnCardPurchased(ShopType shopType, int cardId)
        {
            int  index = CurrentIndex();
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            Publish(TutorialEvents.CardBought, index, TutorialCardIds.Key(card), index);
        }

        private void OnCardSacrificed(ICard card)
        {
            int index = CurrentIndex();

            Publish(TutorialEvents.PartisanSacrificed, index, TutorialCardIds.Key(card), index);
        }

        private void OnCardRecycled(ICard card)
        {
            int index = CurrentIndex();

            Publish(TutorialEvents.CardRecycled, index, TutorialCardIds.Key(card), index);
        }

        private void OnCardDiscarded(ICard card)
        {
            // Raised for the local player only by the game.
            Publish(TutorialEvents.CardDiscarded, 0, TutorialCardIds.Key(card));
        }

        /// <summary>
        /// A card clicked in the hand, on the table or in a shop (raised by the game's click handlers, not in recycle mode).
        /// </summary>
        private void OnCardClicked(ICard card, int slotId)
        {
            TutorialEventBus.Publish(CardClicked, TutorialCardIds.Key(card), slotId);
        }

        private void OnDiscardShuffled()
        {
            TutorialEventBus.Publish(TutorialEvents.DeckReshuffled);
        }

        private void OnPhaseChanged(PhaseType phase)
        {
            TutorialEventBus.Publish(PhaseChanged, phase.ToString(), CurrentIndex());
        }

        private void OnPlayerUpdated(PlayerController player)
        {
            if (player == null || player.Object == null)
                return;
            int index = TutorialMatch.IndexOf(player.Object.InputAuthority);

            if (index != 0)
                return;
            int eloquence = player.Statistics.Eloquence;
            int souls     = player.Statistics.Souls;

            if (eloquence != _LastEloquence) {
                if (_LastEloquence != int.MinValue)
                    TutorialEventBus.Publish(TutorialEvents.EloquenceChanged, null, eloquence);
                _LastEloquence = eloquence;
            }

            if (souls != _LastSouls) {
                if (_LastSouls != int.MinValue) {
                    TutorialEventBus.Publish(TutorialEvents.SoulsChanged, null, souls);

                    // A step can wait for "souls_reached 25": the player has at least 25 souls.
                    for (int threshold = (_LastSouls / SoulsStep + 1) * SoulsStep; threshold <= souls; threshold += SoulsStep) {
                        if (threshold > 0)
                            TutorialEventBus.Publish(SoulsReached, threshold.ToString(), souls);
                    }
                }
                _LastSouls = souls;
            }
        }

        private void OnPlayerWin(PlayerRef winner, PlayerRef local)
        {
            TutorialEventBus.Publish(TutorialEvents.MatchEnded, winner == local ? "win" : "lose", TutorialMatch.IndexOf(winner));
        }

        private void OnEffectPromptRequested(Vermines.Gameplay.Cards.Effect.EffectPrompt prompt)
        {
            TutorialEventBus.Publish(EffectPrompt, prompt.Kind.ToString());
        }

        /// <summary>
        /// "camera_location" with the arg main, courtyard, market or sacrifice: the view the camera just reached
        /// (the shops open when the camera arrives).
        /// </summary>
        private void OnCameraLocationChanged(CamSplineType location)
        {
            string arg = location switch {
                CamSplineType.MainViewToCourtyard or CamSplineType.MarketToCourtyard => "courtyard",
                CamSplineType.MainViewToMarket or CamSplineType.CourtyardToMarket    => "market",
                CamSplineType.MainViewToSacrifice                                    => "sacrifice",
                _                                                                    => "main"
            };

            TutorialEventBus.Publish(CameraLocation, arg);
        }

        private void OnActionRefused(GameActionError error, string message)
        {
            TutorialEventBus.Publish(ActionRefused, error.MessageKey.ToString());

            Debug.Log($"[Tutoriel] Action refusée par le jeu : {message}");
        }

        #endregion
    }
}
