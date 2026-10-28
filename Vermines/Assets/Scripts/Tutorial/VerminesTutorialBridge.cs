using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Tutorial.Integration {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core;
    using Vermines.Core.Network;
    using Vermines.Gameplay.Phases;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.UI;
    using Vermines.UI.Screen;
    using CelticEdge.Tutorial;

    /// <summary>
    /// The game side of the tutorial (ITutorialGameBridge) for Vermines:
    /// - starts the lesson as an offline Fusion session (GameMode.Single), so no
    ///   Photon connection nor matchmaking is needed;
    /// - plays the scripted opponents' turns on the host;
    /// - executes the board commands of the steps (AddEloquence, GiveCardToHand...).
    /// 
    /// Custom commands of the table (Custom(arg=...)):
    ///     skip_to_action  closes the gain summary of the player's turn, so the lesson starts in the Action phase;
    ///     open_table      opens the player's table;
    ///     open_courtyard, open_market.
    /// </summary>
    public class VerminesTutorialBridge : MonoBehaviour, ITutorialGameBridge {

        #region Attributes

        [SerializeField]
        private float _StartTimeout = 60f;

        [SerializeField]
        [Tooltip("Pause before an opponent ends its Gain phase, so the player sees it.")]
        private float _OpponentGainDelay = 1.2f;

        [SerializeField]
        [Tooltip("Pause before an opponent ends a phase after its last action.")]
        private float _OpponentEndPhaseDelay = 0.8f;

        public string GameScenePath { get; set; }

        /// <summary>Raised when the match disappears while a lesson runs (player left from the game menu, disconnection...).</summary>
        public event Action MatchLost;

        private readonly TutorialGameEventRelay _Relay = new();

        private bool _MatchRunning;
        private bool _MatchSeen;

        #endregion

        #region Context helpers

        private static GameplayMode Gameplay
        {
            get
            {
                PlayerController local = PlayerController.Local;

                if (local == null || local.Context == null)
                    return null;
                GameplayMode gameplay = local.Context.GameplayMode;

                return gameplay != null && gameplay.Object != null ? gameplay : null;
            }
        }

        private static PlayerController GetPlayer(int playerIndex)
        {
            GameplayMode gameplay = Gameplay;
            PlayerRef    player   = TutorialMatch.RefOf(playerIndex);

            if (gameplay == null || player == PlayerRef.None)
                return null;
            return gameplay.Context.NetworkGame.GetPlayer(player);
        }

        private static bool IsLoadingScreenVisible()
        {
            string loading = Global.Settings != null ? Global.Settings.LoadingScene : null;

            if (string.IsNullOrEmpty(loading))
                return false;
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(loading);

            return scene.IsValid() && scene.isLoaded;
        }

        #endregion

        #region MonoBehaviour

        private void OnEnable()
        {
            PhaseManager.PhaseProcessed += OnPhaseProcessed;
        }

        private void OnDisable()
        {
            PhaseManager.PhaseProcessed -= OnPhaseProcessed;

            _Relay.Unsubscribe();
        }

        private void Update()
        {
            if (!_MatchRunning)
                return;
            bool hasMatch = Gameplay != null;

            if (hasMatch) {
                _MatchSeen = true;

                _Relay.BindCamera();

                NetworkGame game = Gameplay.Context.NetworkGame;

                if (game != null)
                    game.CheckTutorialPlayers();
            } else if (_MatchSeen) {
                // The game was closed from somewhere else (game menu, disconnection).
                _MatchRunning = false;
                _MatchSeen    = false;

                MatchLost?.Invoke();
            }
        }

        #endregion

        #region ITutorialGameBridge

        public IEnumerator PrepareLesson(TutorialLessonData lesson)
        {
            VerminesLessonSetup setup = lesson.Setup as VerminesLessonSetup;

            if (setup == null) {
                Debug.LogError($"[Tutorial] Lesson \"{lesson.LessonId}\" has no VerminesLessonSetup: cannot start it.");

                yield break;
            }
            if (string.IsNullOrEmpty(GameScenePath)) {
                Debug.LogError("[Tutorial] Unknown game scene path: cannot start the lesson.");

                yield break;
            }

            TutorialMatch.Begin(setup);
            TutorialFreePlayCoach.Begin(VerminesTutorialContent.FreePlayRules(lesson.LessonId));
            _Relay.Subscribe();

            string userID = Global.PlayerService?.PlayerData?.UserID;

            // ReturnToMainMenu: the lesson list is reopened over the main menu, not the tavern of a multiplayer match.
            SessionRequest request = new() {
                UserID           = string.IsNullOrEmpty(userID) ? "tutorial-player" : userID,
                GameMode         = GameMode.Single,
                GameplayType     = GameplayType.Standart,
                SessionName      = $"tutorial-{Guid.NewGuid():N}",
                ScenePath        = GameScenePath,
                ExpectedPlayers  = 1,
                IsCustom         = false,
                IsGameSession    = true,
                ReturnToMainMenu = true
            };

            _MatchRunning = true;
            _MatchSeen    = false;

            Global.Networking.StartGame(request);

            // Ready when the scripted board is built, the local player exists and the loading screen is gone.
            float limit = Time.realtimeSinceStartup + _StartTimeout;

            while (!TutorialMatch.IsInitialized || PlayerController.Local == null || Gameplay == null || Gameplay.State != GameplayMode.GState.Active || IsLoadingScreenVisible()) {
                if (Time.realtimeSinceStartup > limit) {
                    Debug.LogError("[Tutorial] The lesson match did not start in time.");

                    break;
                }

                yield return null;
            }

            TutorialEventBus.Publish(VerminesTutorialEvents.MatchStarted);
        }

        public IEnumerator ExecuteCommand(TutorialCommand command)
        {
            GameplayMode   gameplay = Gameplay;
            PlayerController player = GetPlayer(command.GetInt(VerminesTutorialCommands.PlayerKeys));
            string             card = command.Get(VerminesTutorialCommands.CardKeys);
            int              amount = command.GetInt(VerminesTutorialCommands.AmountKeys);

            if (gameplay == null || !gameplay.HasStateAuthority) {
                Debug.LogWarning($"[Tutorial] Command {command} ignored: no active game.");

                yield break;
            }

            switch (command.Name) {
                case VerminesTutorialCommands.SetEloquence:
                    player?.SetEloquence(amount);

                    break;
                case VerminesTutorialCommands.AddEloquence:
                    if (player != null)
                        player.SetEloquence(player.Statistics.Eloquence + amount);
                    break;
                case VerminesTutorialCommands.AddSouls:
                    if (player != null)
                        player.SetSouls(player.Statistics.Souls + amount);
                    break;
                case VerminesTutorialCommands.DrawCards:
                    player?.DrawAuthoritative(Mathf.Max(1, amount));

                    break;
                case VerminesTutorialCommands.GiveCardToHand:
                    if (player != null) {
                        ICard given = player.TutorialTakeFromDeck(card) ?? FindFreeCard(card);

                        if (given != null)
                            player.DrawCardToHand(given);
                        else
                            Debug.LogWarning($"[Tutorial] GiveCardToHand: no card \"{card}\" available.");
                    }

                    break;
                case VerminesTutorialCommands.PutCardOnTopOfDeck:
                    if (player != null) {
                        ICard given = player.TutorialTakeFromDeck(card) ?? FindFreeCard(card);

                        if (given != null)
                            player.TutorialPutOnTopOfDeck(given);
                    }

                    break;
                case VerminesTutorialCommands.AddCardToCourt:
                    AddCardToCourt(gameplay, card);

                    break;
                case VerminesTutorialCommands.RemoveCardFromCourt:
                    RemoveCardFromCourt(gameplay, card);

                    break;
                case VerminesTutorialCommands.EndCurrentTurn:
                    gameplay.PhaseManager.ForceEndTurn();

                    break;
                case VerminesTutorialCommands.SetCameraFocus:
                    FocusCamera(command.Get(VerminesTutorialCommands.TargetKeys));

                    break;
                case VerminesTutorialCommands.Custom:
                    yield return RunCustom(command.Get(VerminesTutorialCommands.ArgKeys));

                    break;
                default:
                    Debug.Log($"[Tutorial] Command {command.Name} not handled by Vermines.");

                    break;
            }

            // Let the networked state and the UI catch up before the next step shows.
            yield return null;
            yield return null;
        }

        public IEnumerator PerformScriptedAction(int agentIndex, ScriptedAction action)
        {
            GameplayMode   gameplay = Gameplay;
            PlayerController player = GetPlayer(agentIndex);

            if (gameplay == null || player == null || !gameplay.HasStateAuthority)
                yield break;
            PlayerRef playerRef = player.Object.InputAuthority;

            switch (action.Action) {
                case VerminesTutorialActions.PlayCard: {
                    ICard card = FindByName(player.Hand, action.Subject);

                    if (card != null)
                        gameplay.OnCardPlayed(playerRef, card.ID);
                    else
                        WarnMissing(action, "in hand");
                    break;
                }

                case VerminesTutorialActions.BuyCard: {
                    if (TryFindInShop(gameplay, action.Subject, out ShopType shopType, out ICard card))
                        gameplay.OnBuyCard(playerRef, shopType, card.ID);
                    else
                        WarnMissing(action, "in the courtyard or market");
                    break;
                }

                case VerminesTutorialActions.SacrificePartisan: {
                    ICard card = FindByName(player.PlayedCards, action.Subject);

                    if (card != null)
                        gameplay.OnCardSacrificed(playerRef, card.ID);
                    else
                        WarnMissing(action, "in play");
                    break;
                }

                case VerminesTutorialActions.RecycleCard: {
                    ICard card = FindByName(player.Hand, action.Subject);

                    if (card != null)
                        gameplay.OnCardRecycled(playerRef, card.ID);
                    else
                        WarnMissing(action, "in hand");
                    break;
                }

                case VerminesTutorialActions.DiscardCard: {
                    ICard card = FindByName(player.Hand, action.Subject);

                    if (card != null)
                        gameplay.OnDiscardCard(playerRef, card.ID, true);
                    else
                        WarnMissing(action, "in hand");
                    break;
                }

                case VerminesTutorialActions.ActivateEffect: {
                    ICard card = FindByName(player.PlayedCards, action.Subject);

                    if (card != null)
                        gameplay.OnActivateEffect(playerRef, card.ID);
                    else
                        WarnMissing(action, "in play");
                    break;
                }

                default:
                    break;
            }

            yield return null;
        }

        public void CleanupLesson(TutorialLessonData lesson, bool completed)
        {
            _Relay.Unsubscribe();
            StopAllCoroutines();

            TutorialFreePlayCoach.End();

            bool stillInGame = _MatchRunning && Gameplay != null;

            _MatchRunning = false;
            _MatchSeen    = false;

            TutorialMatch.End();

            // As at the end of a normal match (GameplayMode.StopGame): the NPCs of the environment stop.
            StopNpcRoutine();

            if (stillInGame)
                Global.Networking.StopGame();
        }

        /// <summary>
        /// The NPC routine is started by GameplayMode when the lesson match starts, and only stopped by GameplayMode.StopGame,
        /// which a lesson does not call (it stops the offline session itself).
        /// </summary>
        private static void StopNpcRoutine()
        {
            RoutineManager routine = FindFirstObjectByType<RoutineManager>(FindObjectsInactive.Include);

            if (routine != null)
                routine.StopRoutine();
        }

        #endregion

        #region Opponent turns

        private void OnPhaseProcessed(PhaseType phase, PlayerRef player)
        {
            if (!TutorialMatch.IsInitialized)
                return;
            int index = TutorialMatch.IndexOf(player);

            if (index <= 0)
                return;
            GameplayMode gameplay = Gameplay;

            if (gameplay == null || !gameplay.HasStateAuthority)
                return;
            if (GetPlayer(index) == null)
                LogMissingOpponent(index, player);
            StartCoroutine(PlayOpponentPhase(index, player, phase));
        }

        private IEnumerator PlayOpponentPhase(int index, PlayerRef player, PhaseType phase)
        {
            // Phases without anything to do end by themselves (Sacrifice without partisan, Resolution).
            yield return null;

            if (!IsCurrent(player, phase))
                yield break;
            List<ScriptedAction> turn    = OpponentActions(index, out bool useAI);
            List<ScriptedAction> actions = new();

            // A turn where the opponent does nothing is skipped without the pauses made to follow its actions.
            bool quick = !useAI && turn.All(a => a.Action == VerminesTutorialActions.EndTurn || a.Action == VerminesTutorialActions.NextPhase);

            switch (phase) {
                case PhaseType.Sacrifice:
                    actions.AddRange(turn.Where(a => a.Action == VerminesTutorialActions.SacrificePartisan));

                    break;
                case PhaseType.Gain:
                    if (!quick)
                        yield return Wait(_OpponentGainDelay);

                    break;
                case PhaseType.Action:
                    if (useAI)
                        yield return PlayAITurn(index);
                    else {
                        foreach (ScriptedAction action in turn) {
                            if (action.Action == VerminesTutorialActions.EndTurn)
                                break;
                            if (action.Action != VerminesTutorialActions.SacrificePartisan && action.Action != VerminesTutorialActions.NextPhase)
                                actions.Add(action);
                        }
                    }

                    break;
                default:
                    yield break;
            }

            if (actions.Count > 0)
                yield return TutorialSession.PlayScriptedTurn(this, index, actions);
            if (phase == PhaseType.Action)
                DiscardHand(index);
            if (useAI)
                CapAISouls(index);
            yield return Wait(quick ? 0f : _OpponentEndPhaseDelay);

            if (!IsCurrent(player, phase))
                yield break;
            GameplayMode gameplay = Gameplay;

            if (gameplay.PhaseManager.Phases.TryGetValue(phase, out PhaseAsset asset))
                asset.OnPhaseEnding(player, true);
        }

        /// <summary>
        /// Diagnostic: the opponent's PlayerController can no longer be found from its PlayerRef.
        /// </summary>
        private static void LogMissingOpponent(int index, PlayerRef player)
        {
            GameplayMode gameplay = Gameplay;
            List<string> controllers = new();

            foreach (PlayerController controller in gameplay.Runner.GetAllBehaviours<PlayerController>())
                controllers.Add(controller.Object != null ? $"{controller.name} -> {controller.Object.InputAuthority}" : $"{controller.name} -> (no object)");
            Debug.LogError($"[Tutorial] Opponent {index} ({player}) has no PlayerController. Controllers: {string.Join(", ", controllers)}. Topology: {gameplay.Runner.Config.Simulation.Topology}.");
        }

        /// <summary>
        /// The scripted actions of this turn of the opponent.
        /// useAI = true when the game plays the turn instead (opponent not scripted, or unscripted turn of an opponent played by the AI).
        /// </summary>
        private List<ScriptedAction> OpponentActions(int index, out bool useAI)
        {
            int turnNumber = _Relay.TurnCount(index);

            useAI = !TutorialSession.TryGetScriptedTurn(index, turnNumber, out IReadOnlyList<ScriptedAction> actions);

            return useAI ? new List<ScriptedAction>() : actions.ToList();
        }

        /// <summary>
        /// Discard powers the AI can use: they only give eloquence, souls or cards, without a choice to make
        /// (the effect prompts only exist on the human's screen).
        /// </summary>
        private static readonly HashSet<string> _AIDiscardCards = new() {
            "operarius", "vicanus", "latro", "scriptum_insanum", "insectonomicon"
        };

        /// <summary>
        /// Cards whose discard power adds a card to the court. The power only works for the local player,
        /// so the AI discards them, then adds the card itself (GameplayMode.OnCardAddInCourtyard), as the power would.
        /// </summary>
        private static readonly HashSet<string> _AIRefillCards = new() {
            "epistula_regia", "armiger"
        };

        /// <summary>
        /// Unscripted turn of an opponent, one action at a time (each one changes what the next can do):
        ///     1. discard the cards whose power gives eloquence (Operarius...), except partisans that still fit on the table;
        ///     2. court with 2 cards or less: refill it with an Epistula Regia or an Armiger of its hand;
        ///     3. table full: sacrifice the partisan worth the most souls, if the soul cap allows it;
        ///     4. place its partisans on the table;
        ///     5. buy the partisan worth the most souls it can afford, as long as the court keeps 2 cards for the player;
        ///        with a court too small, buy an Epistula Regia at the market to refill it later.
        /// It plays fair enough to win a whole match, without combos nor long-term plans.
        /// </summary>
        private IEnumerator PlayAITurn(int index)
        {
            const float actionDelay = 0.8f;
            const int   courtToKeep = 2;

            PlayerController player = GetPlayer(index);

            if (player == null)
                yield break;
            int slots = Mathf.Max(1, player.Statistics.NumberOfSlotInTable);

            foreach (ICard card in player.Hand.ToList()) {
                bool fitsOnTable = card.Data.Type == CardType.Partisan && player.PlayedCards.Count < slots;

                if (fitsOnTable || !_AIDiscardCards.Contains(TutorialCardIds.Key(card)) || !card.Data.CanBeDiscard())
                    continue;
                yield return PlayAIAction(index, new ScriptedAction(VerminesTutorialActions.DiscardCard, TutorialCardIds.Key(card), actionDelay));
            }

            ICard refill = player.Hand.FirstOrDefault(card => _AIRefillCards.Contains(TutorialCardIds.Key(card)));

            if (refill != null && CourtCards().Count <= courtToKeep) {
                yield return PlayAIAction(index, new ScriptedAction(VerminesTutorialActions.DiscardCard, TutorialCardIds.Key(refill), actionDelay));

                GameplayMode gameplay = Gameplay;

                if (gameplay != null && player.Object != null)
                    gameplay.OnCardAddInCourtyard(player.Object.InputAuthority, 1);
            }

            int  partisansInHand = player.Hand.Count(card => card.Data.Type == CardType.Partisan);
            ICard        victim = player.PlayedCards.OrderByDescending(card => card.Data.CurrentSouls).FirstOrDefault();

            if (partisansInHand > 0 && player.PlayedCards.Count >= slots && victim != null && CanAIEarnSouls(index, player, victim.Data.CurrentSouls))
                yield return PlayAIAction(index, new ScriptedAction(VerminesTutorialActions.SacrificePartisan, TutorialCardIds.Key(victim), actionDelay));
            foreach (ICard card in player.Hand.Where(card => card.Data.Type == CardType.Partisan).ToList()) {
                if (player.PlayedCards.Count >= slots)
                    break;
                yield return PlayAIAction(index, new ScriptedAction(VerminesTutorialActions.PlayCard, TutorialCardIds.Key(card), actionDelay));
            }

            List<ICard> shown = CourtCards();
            ICard      choice = shown.Where(card => card.Data.CurrentEloquence <= player.Statistics.Eloquence).OrderByDescending(card => card.Data.CurrentSouls).FirstOrDefault();

            if (shown.Count > courtToKeep && choice != null)
                yield return PlayAIAction(index, new ScriptedAction(VerminesTutorialActions.BuyCard, TutorialCardIds.Key(choice), actionDelay));
            else if (shown.Count <= courtToKeep && !player.Deck.Concat(player.Discard).Concat(player.Hand).Any(card => _AIRefillCards.Contains(TutorialCardIds.Key(card)))) {
                GameplayMode gameplay = Gameplay;

                if (gameplay != null && TryFindInShop(gameplay, "epistula_regia", out ShopType shopType, out ICard epistula) && shopType == ShopType.Market && epistula.Data.CurrentEloquence <= player.Statistics.Eloquence)
                    yield return PlayAIAction(index, new ScriptedAction(VerminesTutorialActions.BuyCard, "epistula_regia", actionDelay));
            }
        }

        /// <summary>
        /// Cards shown in the court.
        /// </summary>
        private static List<ICard> CourtCards()
        {
            GameplayMode gameplay = Gameplay;

            if (gameplay == null || !gameplay.Shop.Sections.TryGetValue(ShopType.Courtyard, out var section) || section is not CourtyardSection court)
                return new List<ICard>();
            return court.AvailableCards.Values.Where(card => card != null).ToList();
        }

        private IEnumerator PlayAIAction(int index, ScriptedAction action)
        {
            yield return TutorialSession.PlayScriptedTurn(this, index, new[] { action });
        }

        private static int AISoulCap(int index)
        {
            VerminesLessonSetup setup = TutorialMatch.Setup;

            return setup != null && index >= 0 && index < setup.Players.Count ? setup.Players[index].AIMaxSouls : 0;
        }

        private static bool CanAIEarnSouls(int index, PlayerController player, int souls)
        {
            int cap = AISoulCap(index);

            return cap <= 0 || player.Statistics.Souls + souls <= cap;
        }

        /// <summary>
        /// Powers can still give souls to the AI (Latro, effects at the start of its turn): above the cap, its souls go back to the cap.
        /// </summary>
        private static void CapAISouls(int index)
        {
            PlayerController player = GetPlayer(index);
            int                 cap = AISoulCap(index);

            if (player == null || cap <= 0 || player.Statistics.Souls <= cap)
                return;
            player.SetSouls(cap);

            Debug.Log($"[Tutorial] Opponent {index}: souls brought back to the lesson's cap ({cap}).");
        }

        /// <summary>
        /// End of the Action phase: the cards left in hand are discarded without effect, as for a player.
        /// </summary>
        private static void DiscardHand(int index)
        {
            GameplayMode   gameplay = Gameplay;
            PlayerController player = GetPlayer(index);

            if (gameplay == null || player == null)
                return;
            foreach (ICard card in player.Hand.ToList()) {
                if (card.Data.CanBeDiscard())
                    gameplay.OnDiscardCard(player.Object.InputAuthority, card.ID, false);
            }
        }

        private static bool IsCurrent(PlayerRef player, PhaseType phase)
        {
            GameplayMode gameplay = Gameplay;

            return gameplay != null && gameplay.State == GameplayMode.GState.Active && gameplay.CurrentPlayer == player && gameplay.PhaseManager.CurrentPhase == phase;
        }

        private static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds || TutorialSession.AgentsPaused; t += Time.deltaTime)
                yield return null;
        }

        #endregion

        #region Board helpers

        private static ICard FindByName(IEnumerable<ICard> cards, string cardId)
        {
            return cards?.FirstOrDefault(card => TutorialCardIds.Is(card, cardId));
        }

        private static void WarnMissing(ScriptedAction action, string where)
        {
            Debug.LogWarning($"[Tutorial] Opponent: {action} impossible, card \"{action.Subject}\" missing {where}.");
        }

        private static bool TryFindInShop(GameplayMode gameplay, string cardId, out ShopType shopType, out ICard card)
        {
            foreach (var section in gameplay.Shop.Sections) {
                IEnumerable<ICard> visible = section.Value switch {
                    CourtyardSection courtyard => courtyard.AvailableCards.Values,
                    MarketSection market       => market,
                    _                          => Enumerable.Empty<ICard>()
                };

                card = FindByName(visible.Where(c => c != null), cardId);

                if (card != null) {
                    shopType = section.Key;

                    return true;
                }
            }

            shopType = default;
            card     = null;

            return false;
        }

        /// <summary>
        /// A card of this name that nobody owns and that is not in the shop.
        /// </summary>
        private static ICard FindFreeCard(string cardId)
        {
            GameplayMode gameplay = Gameplay;

            if (gameplay == null)
                return null;
            HashSet<int> owned = new();

            foreach (PlayerController player in gameplay.Runner.GetAllBehaviours<PlayerController>()) {
                foreach (IReadOnlyList<ICard> zone in new[] {
                    player.Deck, player.Discard, player.Hand, player.PlayedCards, player.Equipments, player.ToolDiscard, player.Graveyard
                }) {
                    foreach (ICard card in zone)
                        owned.Add(card.ID);
                }
            }

            foreach (var section in gameplay.Shop.Sections) {
                if (section.Value is CourtyardSection courtyard) {
                    foreach (ICard card in courtyard.AvailableCards.Values.Concat(courtyard.Deck1).Concat(courtyard.Deck2)) {
                        if (card != null)
                            owned.Add(card.ID);
                    }
                } else if (section.Value is MarketSection market) {
                    foreach (List<ICard> pile in market.CardPiles.Values) {
                        foreach (ICard card in pile)
                            owned.Add(card.ID);
                    }
                }
            }

            return CardSetDatabase.Instance.GetEveryCardWith(card => !owned.Contains(card.ID) && TutorialCardIds.Is(card, cardId)).FirstOrDefault();
        }

        private static void AddCardToCourt(GameplayMode gameplay, string cardId)
        {
            if (!(gameplay.Shop.Sections.TryGetValue(ShopType.Courtyard, out var section) && section is CourtyardSection courtyard))
                return;
            ICard card = courtyard.Deck1.Concat(courtyard.Deck2).FirstOrDefault(c => TutorialCardIds.Is(c, cardId)) ?? FindFreeCard(cardId);

            if (card == null) {
                Debug.LogWarning($"[Tutorial] AddCardToCourt: no card \"{cardId}\" available.");

                return;
            }

            courtyard.Deck1.Remove(card);
            courtyard.Deck2.Remove(card);

            List<ICard> deck = card.Data.Level == 2 ? courtyard.Deck2 : courtyard.Deck1;

            deck.Insert(0, card);

            if (courtyard.AvailableCards.Values.Any(c => c == null))
                courtyard.AddCard(card.Data.Level == 2 ? 2 : 1);
            RefreshShop(gameplay, ShopType.Courtyard);
        }

        private static void RemoveCardFromCourt(GameplayMode gameplay, string cardId)
        {
            if (!(gameplay.Shop.Sections.TryGetValue(ShopType.Courtyard, out var section) && section is CourtyardSection courtyard))
                return;
            ICard card = courtyard.AvailableCards.Values.FirstOrDefault(c => TutorialCardIds.Is(c, cardId));

            if (card == null)
                return;
            courtyard.BuyCard(card.ID);

            RefreshShop(gameplay, ShopType.Courtyard);
        }

        private static void RefreshShop(GameplayMode gameplay, ShopType shopType)
        {
            gameplay.RPC_InitializeShop(shopType, gameplay.Shop.SerializeSection(shopType));
        }

        private static void FocusCamera(string target)
        {
            CamManager camera = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            if (camera == null)
                return;
            switch (TutorialCardIds.Key(target)) {
                case "court":
                case "courtyard":
                case "cour":
                    camera.GoOnCourtyardLocation();

                    break;
                case "market":
                case "marche":
                    camera.GoOnMarketLocation();

                    break;
                case "sacrifice":
                    camera.GoOnSacrificeLocation();

                    break;
                default:
                    camera.GoOnNoneLocation();

                    break;
            }
        }

        private static IEnumerator RunCustom(string arg)
        {
            switch (TutorialCardIds.Key(arg)) {
                case "open_courtyard":
                    GameEvents.OnShopOpenRequested.Invoke(ShopType.Courtyard);

                    break;
                case "open_market":
                    GameEvents.OnShopOpenRequested.Invoke(ShopType.Market);

                    break;
                case "skip_to_action":
                    yield return SkipToActionPhase();

                    break;
                case "open_table":
                    OpenTable();

                    break;
                default:
                    Debug.LogWarning($"[Tutorial] Custom command \"{arg}\" unknown.");

                    break;
            }
        }

        /// <summary>
        /// Waits for the player's gain summary and closes it as its button does, until the player is in the Action phase.
        /// </summary>
        private static IEnumerator SkipToActionPhase()
        {
            const float timeout = 30f;

            float limit  = Time.realtimeSinceStartup + timeout;
            bool  closed = false;

            while (Time.realtimeSinceStartup < limit) {
                GameplayMode gameplay = Gameplay;

                if (gameplay != null && TutorialMatch.IndexOf(gameplay.CurrentPlayer) == 0) {
                    PhaseType phase = gameplay.PhaseManager.CurrentPhase;

                    if (phase == PhaseType.Action)
                        yield break;
                    if (phase == PhaseType.Gain && !closed) {
                        GameplayUIGainSummary summary = FindFirstObjectByType<GameplayUIGainSummary>(FindObjectsInactive.Exclude);

                        if (summary != null) {
                            summary.TutorialClose();

                            closed = true;
                        }
                    }
                }

                yield return null;
            }

            Debug.LogWarning("[Tutorial] skip_to_action: the player never reached the Action phase.");
        }

        private static void OpenTable()
        {
            GameplayUIController ui = FindFirstObjectByType<GameplayUIController>(FindObjectsInactive.Include);

            if (ui == null)
                return;
            ui.GetActiveScreen(out GameplayUIScreen active);

            if (active is GameplayUITable)
                return;
            ui.Show<GameplayUITable>(active);
        }

        #endregion
    }
}
