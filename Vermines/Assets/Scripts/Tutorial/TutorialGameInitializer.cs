using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Fusion;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Characters;
    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.Gameplay;
    using Vermines.Player;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;

    /// <summary>
    /// Replaces StandartGameInitializer during a tutorial lesson:
    /// instead of a random seed, the board comes from the lesson's ScriptedMatchSetup (families, gods, deck order, courtyard and market, starting resources).
    /// 
    /// It also spawns the scripted opponents, since a tutorial match runs offline with a single human.
    /// Runs on the state authority only, like the standard initializer.
    /// </summary>
    public class TutorialGameInitializer : GameModeInitializer {

        #region Attributes

        private readonly HashSet<int> _UsedCards = new();

        private List<ICard> _Pool;

        private List<CardFamily> _Families = new();

        private readonly Dictionary<int, List<ICard>> _StartingHands = new();

        #endregion

        #region Constructor

        public TutorialGameInitializer(GameplayMode mode) : base(mode) {}

        #endregion

        #region Events

        protected override void OnInitialize(string data)
        {
            ScriptedMatchSetup setup = TutorialMatch.Setup;

            if (setup == null || setup.players.Count == 0) {
                Debug.LogError("[Tutorial] Scripted match without setup: the lesson cannot start.");

                return;
            }

            NetworkGame.Seed      = setup.fallbackSeed;
            NetworkGame.Random    = new System.Random(setup.fallbackSeed);
            Mode.TurnTimerSeconds = 0;

            if (setup.victorySoulsOverride > 0)
                Mode.SoulsLimit = setup.victorySoulsOverride;
            // -- Players: the human who joined, then one spawned controller per scripted opponent.
            Dictionary<int, CardFamily> joined = JsonConvert.DeserializeObject<Dictionary<int, CardFamily>>(data) ?? new();

            PlayerRef human = joined.Keys.Select(PlayerRef.FromEncoded).FirstOrDefault(p => NetworkGame.GetPlayer(p) != null);

            if (human == PlayerRef.None) {
                Debug.LogError("[Tutorial] No local player found for the lesson.");

                return;
            }

            List<CardFamily> families = ResolveFamilies(setup, joined.TryGetValue(human.RawEncoded, out CardFamily joinedFamily) ? joinedFamily : CardFamily.None);

            _Families = families;
            List<PlayerRef>  players  = new() {
                human
            };

            List<PlayerController> controllers = new() {
                NetworkGame.GetPlayer(human)
            };

            EnsureClientServerTopology();

            for (int i = 1; i < setup.players.Count; i++) {
                PlayerRef     botRef = PlayerRef.FromIndex(human.AsIndex + i);
                PlayerController bot = NetworkGame.SpawnTutorialPlayer(botRef);

                if (bot == null) {
                    Debug.LogError($"[Tutorial] Oppenent creation failed {i}.");

                    return;
                }

                string nickname = string.IsNullOrEmpty(setup.players[i].displayName) ? $"Opponent {i}" : setup.players[i].displayName;

                if (bot.Object.InputAuthority != botRef) {
                    Debug.LogError($"[Tutoriel] L'adversaire {i} n'a pas reçu son PlayerRef ({botRef}, reçu {bot.Object.InputAuthority}) : topologie Fusion {Runner.Config.Simulation.Topology}.");

                    return;
                }

                bot.SetupAsTutorialOpponent(nickname, families[i]);

                players.Add(botRef);
                controllers.Add(bot);
            }

            TutorialMatch.MarkInitialized(players);

            InitializeTurnOrder(setup, players);
            InitializeStatistics(setup, controllers, families);
            InitializeGods(setup, controllers);

            Mode.RPC_InitializeCards(FamilyUtils.FamiliesListToIds(families));

            _Pool = CardSetDatabase.Instance.GetEveryCardWith() ?? new List<ICard>();

            _UsedCards.Clear();
            _StartingHands.Clear();

            for (int i = 0; i < controllers.Count; i++)
                InitializeDeck(setup.players[i], controllers[i], families[i], i);
            for (int i = 0; i < controllers.Count; i++)
                InitializeHand(setup.players[i], families[i], i);
            for (int i = 0; i < controllers.Count; i++)
                InitializeBoard(setup.players[i], controllers[i], families[i]);
            InitializeCourtyard(setup);
            InitializeMarket(setup);
            DrawStartingHands(setup, controllers);

            Mode.RPC_Initialized();
        }

        protected override void OnActivate() {}

        #endregion

        #region Players

        /// <summary>
        /// Fusion does not set the topology of a GameMode.Single session
        /// (it stays 0, see SimulationConfig.Topology, a [NonSerialized] field only set for Host/Client/Shared).
        /// With a topology other than ClientServer, NetworkObject.AssignInputAuthority ignores every player but the local one,
        /// so the scripted opponent would get PlayerRef.None.
        /// 
        /// Runner.Config.Simulation is the very object the running simulation reads,
        /// so the value is fixed here, on this offline runner only, before the opponents are spawned.
        /// </summary>
        private void EnsureClientServerTopology()
        {
            if (Runner == null || Runner.GameMode != GameMode.Single || Runner.Config?.Simulation == null)
                return;
            if (Runner.Config.Simulation.Topology != Topologies.ClientServer)
                Runner.Config.Simulation.Topology = Topologies.ClientServer;
        }

        private List<CardFamily> ResolveFamilies(ScriptedMatchSetup setup, CardFamily joinedFamily)
        {
            List<CardFamily> families = new();
            Cultist[]        cultists = Global.Settings.Cultists.GetAllCultists();

            for (int i = 0; i < setup.players.Count; i++) {
                CardFamily family = ResolveFamily(setup.players[i].cultistId, cultists);

                if (family == CardFamily.None && i == 0)
                    family = joinedFamily;
                if (family == CardFamily.None) {
                    // First cultist family nobody uses yet, so that each player has its own family cards.
                    foreach (Cultist cultist in cultists) {
                        if (cultist.family != CardFamily.None && !families.Contains(cultist.family)) {
                            family = cultist.family;

                            break;
                        }
                    }
                }

                if (family == CardFamily.None)
                    family = (CardFamily)(i % (int)CardFamily.Count);
                families.Add(family);
            }

            return families;
        }

        /// <summary>
        /// A cultist ID ("2"), cultist name ("Scarab Cultist") or family name ("Scarab").
        /// </summary>
        private static CardFamily ResolveFamily(string cultistId, Cultist[] cultists)
        {
            if (string.IsNullOrWhiteSpace(cultistId))
                return CardFamily.None;
            if (int.TryParse(cultistId, out int id)) {
                foreach (Cultist cultist in cultists) {
                    if (cultist.ID == id)
                        return cultist.family;
                }
            }

            string key = TutorialCardIds.Key(cultistId);

            foreach (Cultist cultist in cultists) {
                if (TutorialCardIds.Key(cultist.Name) == key || TutorialCardIds.Key(cultist.name) == key)
                    return cultist.family;
            }

            for (CardFamily family = 0; family < CardFamily.Count; family++) {
                if (TutorialCardIds.Key(family.ToString()) == key)
                    return family;
            }

            Debug.LogWarning($"[Tutorial] Cultist \"{cultistId}\" unknown: default family used.");

            return CardFamily.None;
        }

        private void InitializeTurnOrder(ScriptedMatchSetup setup, List<PlayerRef> players)
        {
            int             first = Mathf.Clamp(setup.firstPlayerIndex, 0, players.Count - 1);
            List<PlayerRef> order = new();

            for (int i = 0; i < players.Count; i++)
                order.Add(players[(first + i) % players.Count]);
            Mode.SetPlayerTurnOrder(order);

            Mode.CurrentPlayerIndex = 0;
        }

        private void InitializeStatistics(ScriptedMatchSetup setup, List<PlayerController> controllers, List<CardFamily> families)
        {
            for (int i = 0; i < controllers.Count; i++) {
                PlayerStatistics stats = controllers[i].Statistics;

                stats.PlayerRef           = controllers[i].Object.InputAuthority;
                stats.Eloquence           = setup.players[i].startingEloquence;
                stats.Souls               = setup.players[i].startingSouls;
                stats.NumberOfSlotInTable = 3;
                stats.Family              = families[i];
                stats.IsConnected         = true;

                controllers[i].UpdateStatistics(stats);
            }
        }

        private void InitializeGods(ScriptedMatchSetup setup, List<PlayerController> controllers)
        {
            God[] gods = Global.Settings.Gods.GetAllGods();

            if (gods == null || gods.Length == 0)
                return;
            for (int i = 0; i < controllers.Count; i++) {
                God god = ResolveGod(setup.players[i].godId, gods) ?? gods[NetworkGame.Random.Next(0, gods.Length)];

                Mode.RPC_InitializeGod(controllers[i].Object.InputAuthority.RawEncoded, god.ID);
            }
        }

        private static God ResolveGod(string godId, God[] gods)
        {
            if (string.IsNullOrWhiteSpace(godId))
                return null;
            if (int.TryParse(godId, out int id)) {
                foreach (God god in gods) {
                    if (god.ID == id)
                        return god;
                }
            }

            string key = TutorialCardIds.Key(godId);

            foreach (God god in gods) {
                if (TutorialCardIds.Key(god.Name) == key || TutorialCardIds.Key(god.name) == key)
                    return god;
            }

            Debug.LogWarning($"[Tutorial] God \"{godId}\" unknown: default god used.");

            return null;
        }

        #endregion

        #region Cards

        /// <summary>
        /// A card ID of the setup can carry two suffixes:
        ///     "antistes@opponent" (or "@player", "@firefly"...) = the copy of this family,
        ///     "idolum_maledictum*1" = only this number of copies (market piles).
        /// </summary>
        private static string ParseCardSpec(string raw, out string familyToken, out int count)
        {
            familyToken = null;
            count       = 0;

            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;
            string name = raw.Trim();
            int    star = name.IndexOf('*');

            if (star >= 0) {
                if (!int.TryParse(name.Substring(star + 1).Trim(), out count))
                    count = 0;
                name = name.Substring(0, star);
            }

            int at = name.IndexOf('@');

            if (at >= 0) {
                familyToken = name.Substring(at + 1).Trim();
                name        = name.Substring(0, at);
            }

            return name.Trim();
        }

        /// <summary>
        /// "opponent"/"adversaire" = family of the first opponent, "player"/"joueur" = family of the human,
        /// otherwise a family or cultist name.
        /// </summary>
        private CardFamily ResolveFamilyToken(string token)
        {
            switch (TutorialCardIds.Key(token)) {
                case "opponent":
                case "adversaire":
                    return _Families.Count > 1 ? _Families[1] : CardFamily.None;
                case "player":
                case "joueur":
                    return _Families.Count > 0 ? _Families[0] : CardFamily.None;
                default:
                    return ResolveFamily(token, Global.Settings.Cultists.GetAllCultists());
            }
        }

        /// <summary>
        /// Takes an unused card with this name from the card set.
        /// Family cards exist once per family: the copy of the player's family wins, unless the ID forces a family ("antistes@opponent").
        /// Starting copies are preferred for decks, shop copies for the shop.
        /// </summary>
        private ICard Take(string cardId, CardFamily family, bool preferStartingCopy)
        {
            string       name = ParseCardSpec(cardId, out string familyToken, out _);
            string        key = TutorialCardIds.Key(name);
            CardFamily forced = string.IsNullOrEmpty(familyToken) ? CardFamily.None : ResolveFamilyToken(familyToken);
            ICard        best = null;
            int     bestScore = -1;

            if (forced != CardFamily.None)
                family = forced;
            foreach (ICard card in _Pool) {
                if (_UsedCards.Contains(card.ID) || TutorialCardIds.Key(card) != key)
                    continue;
                if (forced != CardFamily.None && card.Data.IsFamilyCard && card.Data.Family != forced)
                    continue;
                int score = 0;

                if (!card.Data.IsFamilyCard || card.Data.Family == family)
                    score += 2;
                if (card.Data.IsStartingCard == preferStartingCopy)
                    score += 1;
                if (score > bestScore) {
                    best      = card;
                    bestScore = score;
                }
            }

            if (best == null) {
                Debug.LogWarning($"[Tutorial] Card \"{cardId}\" not found (or no more available copies) in the card set.");

                return null;
            }

            _UsedCards.Add(best.ID);

            return best;
        }

        private List<ICard> TakeAll(IEnumerable<string> cardIds, CardFamily family, bool preferStartingCopy)
        {
            List<ICard> cards = new();

            if (cardIds == null)
                return cards;
            foreach (string id in cardIds) {
                ICard card = Take(id, family, preferStartingCopy);

                if (card != null)
                    cards.Add(card);
            }

            return cards;
        }

        private void InitializeDeck(ScriptedPlayerSetup player, PlayerController controller, CardFamily family, int index)
        {
            List<ICard> deck;

            if (player.exactDeck || (player.deckTopToBottom != null && player.deckTopToBottom.Count > 0)) {
                deck = TakeAll(player.deckTopToBottom, family, true);
            } else {
                // Normal starting deck (every starting card, DeckExemplars times), shuffled with the lesson seed.
                deck = new List<ICard>();

                foreach (var group in _Pool.Where(card => card.Data.IsStartingCard).GroupBy(card => TutorialCardIds.Key(card))) {
                    ICard sample = group.FirstOrDefault(card => !card.Data.IsFamilyCard || card.Data.Family == family);

                    if (sample == null)
                        continue;
                    for (int i = 0; i < Mathf.Max(1, sample.Data.DeckExemplars); i++) {
                        ICard card = Take(group.Key, family, true);

                        if (card != null)
                            deck.Add(card);
                    }
                }

                deck.Shuffle(NetworkGame.Seed + index);
            }

            controller.InitializeDeck(NetworkGame.Seed + index, deck);
        }

        /// <summary>
        /// A scripted starting hand is taken now, before the shops use the remaining cards.
        /// </summary>
        private void InitializeHand(ScriptedPlayerSetup player, CardFamily family, int index)
        {
            if (player.startingHand == null || player.startingHand.Count == 0)
                return;
            _StartingHands[index] = TakeAll(player.startingHand, family, true);
        }

        private void InitializeBoard(ScriptedPlayerSetup player, PlayerController controller, CardFamily family)
        {
            foreach (ICard card in TakeAll(player.startingPartisans, family, false))
                controller.AddCardToPlayedCards(card);
            foreach (ICard card in TakeAll(player.startingDiscard, family, true))
                controller.AddCardToDiscard(card);
            foreach (ICard card in TakeAll(player.startingSacrificed, family, false))
                controller.AddCardToGraveyard(card);
        }

        private void InitializeCourtyard(ScriptedMatchSetup setup)
        {
            const int level1Slots = 3;
            const int level2Slots = 2;

            CourtyardSection section = new(level1Slots, level2Slots);

            List<ICard> scripted1 = new();
            List<ICard> scripted2 = new();

            // courtOverride = the exact cards shown at the start, so they go on top of their pile.
            List<ICard> shown = TakeAll(setup.courtOverride, CardFamily.None, false);

            foreach (ICard card in shown)
                (card.Data.Level == 2 ? scripted2 : scripted1).Add(card);
            scripted1.AddRange(TakeAll(setup.courtPileI, CardFamily.None, false));
            scripted2.AddRange(TakeAll(setup.courtPileII, CardFamily.None, false));

            List<ICard> rest1 = Buyable(card => card.Data.Type == CardType.Partisan && card.Data.Level == 1);
            List<ICard> rest2 = Buyable(card => card.Data.Type == CardType.Partisan && card.Data.Level == 2);

            rest1.Shuffle(NetworkGame.Seed);
            rest2.Shuffle(NetworkGame.Seed);

            section.Deck1 = scripted1.Concat(rest1).ToList();
            section.Deck2 = scripted2.Concat(rest2).ToList();

            if (shown.Count > 0) {
                // Only the override cards are revealed: the other slots stay empty (a lesson can show a court to refill).
                foreach (ICard card in shown)
                    section.AddCard(card.Data.Level == 2 ? 2 : 1);
            } else {
                for (int i = 0; i < level1Slots; i++)
                    section.AddCard(1);
                for (int i = 0; i < level2Slots; i++)
                    section.AddCard(2);
            }
            Mode.Shop.AddSection(ShopType.Courtyard, section);
            Mode.RPC_InitializeShop(ShopType.Courtyard, Mode.Shop.SerializeSection(ShopType.Courtyard));
        }

        private void InitializeMarket(ScriptedMatchSetup setup)
        {
            List<ICard>  tools = Buyable(card => card.Data.Type == CardType.Tools);
            List<string>   allTools = tools.Select(card => TutorialCardIds.Key(card)).Distinct().ToList();
            List<string>  pileOrder = new();
            Dictionary<string, int> copies = new();

            if (setup.marketPiles == null || setup.marketPiles.Count == 0) {
                pileOrder.AddRange(allTools);
            } else {
                // "name*N" limits a pile, "*" adds every tool not listed yet.
                foreach (string pile in setup.marketPiles) {
                    if (pile != null && pile.Trim() == "*") {
                        pileOrder.Add("*");

                        continue;
                    }

                    string key = TutorialCardIds.Key(ParseCardSpec(pile, out _, out int count));

                    if (string.IsNullOrEmpty(key) || pileOrder.Contains(key))
                        continue;
                    pileOrder.Add(key);

                    if (count > 0)
                        copies[key] = count;
                }

                int wildcard = pileOrder.IndexOf("*");

                if (wildcard >= 0) {
                    pileOrder.RemoveAt(wildcard);
                    pileOrder.InsertRange(wildcard, allTools.Where(key => !pileOrder.Contains(key)));
                }
            }

            MarketSection section = new(pileOrder.Count);

            for (int i = 0; i < pileOrder.Count; i++) {
                int max = copies.TryGetValue(pileOrder[i], out int count) ? count : int.MaxValue;

                foreach (ICard card in tools.Where(card => TutorialCardIds.Key(card) == pileOrder[i]).Take(max)) {
                    section.CardPiles[i].Add(card);

                    _UsedCards.Add(card.ID);
                }
            }

            Mode.Shop.AddSection(ShopType.Market, section);
            Mode.RPC_InitializeShop(ShopType.Market, Mode.Shop.SerializeSection(ShopType.Market));
        }

        /// <summary>
        /// Shop copies not used by the setup, marked as used.
        /// </summary>
        private List<ICard> Buyable(System.Func<ICard, bool> filter)
        {
            List<ICard> cards = _Pool.Where(card => !card.Data.IsStartingCard && !_UsedCards.Contains(card.ID) && filter(card)).ToList();

            foreach (ICard card in cards)
                _UsedCards.Add(card.ID);
            return cards;
        }

        /// <summary>
        /// Same rule as a normal match: the first player draws one card less.
        /// A scripted starting hand replaces the draw.
        /// </summary>
        private void DrawStartingHands(ScriptedMatchSetup setup, List<PlayerController> controllers)
        {
            const int baseCardsToDraw = 3;

            int first = Mathf.Clamp(setup.firstPlayerIndex, 0, controllers.Count - 1);

            for (int i = 0; i < controllers.Count; i++) {
                if (_StartingHands.TryGetValue(i, out List<ICard> hand)) {
                    foreach (ICard card in hand)
                        controllers[i].DrawCardToHand(card);
                    continue;
                }

                controllers[i].DrawAuthoritative(i == first ? baseCardsToDraw - 1 : baseCardsToDraw);
            }
        }

        #endregion
    }
}
