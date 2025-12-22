using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Fusion;
using UnityEngine;

namespace Vermines.Gameplay {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.Player;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;

    public class StandartGameInitializer : GameModeInitializer {

        #region Constructor

        public StandartGameInitializer(GameplayMode mode) : base(mode) {}

        #endregion

        #region Methods

        private List<PlayerRef> InitializePlayerTurnOrder(List<PlayerRef> players)
        {
            NetworkGame.Random = new(NetworkGame.Seed);

            for (int i = players.Count - 1; i > 0; i--) {
                int k = NetworkGame.Random.Next(i + 1);

                (players[i], players[k]) = (players[k], players[i]);
            }

            Mode.SetPlayerTurnOrder(players);

            return players;
        }

        private void InitializePlayers(List<PlayerRef> players)
        {
            foreach (PlayerRef playerRef in players) {
                PlayerController player = NetworkGame.GetPlayer(playerRef);

                if (player) {
                    PlayerStatistics stats = player.Statistics;

                    stats.Eloquence           = 0;
                    stats.Souls               = 0;
                    stats.NumberOfSlotInTable = 3;

                    player.UpdateStatistics(stats);
                }
            }
        }

        private void InitializeCards(List<CardFamily> families)
        {
            int[] familyIds = FamilyUtils.FamiliesListToIds(families);

            Mode.RPC_InitializeCards(familyIds);
        }

        private void InitializePlayerDecks(List<PlayerRef> players)
        {
            List<ICard> starterCards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard);
            int    starterDeckLength = starterCards.Count / players.Count;

            foreach (PlayerRef playerRef in players) {
                PlayerController player = NetworkGame.GetPlayer(playerRef);

                if (player) {
                    PlayerDeck deck = new();

                    deck.Initialize(NetworkGame.Seed);

                    for (int i = 0; i < starterDeckLength; i++) {
                        ICard card = starterCards[NetworkGame.Random.Next(starterDeckLength - deck.Deck.Count)];

                        deck.Deck.Add(card);
                        starterCards.Remove(card);
                    }

                    deck.Deck.Shuffle(NetworkGame.Seed);

                    player.RPC_DeckResynchronization(deck.Serialize());
                }
            }
        }

        private void InitializeShop()
        {
            List<ICard> everyBuyableCard = CardSetDatabase.Instance.GetEveryCardWith(card => !card.Data.IsStartingCard);

            InitializeCourtyard(everyBuyableCard.Where(card => card.Data.Type == CardType.Partisan).ToList());
            InitializeMarket(everyBuyableCard.Where(card => card.Data.Type == CardType.Tools).ToList());
        }

        private void InitializeCourtyard(List<ICard> cards)
        {
            CourtyardSection section = new(3, 2);

            List<ICard> partisan1Cards = cards.Where(card => card.Data.Level == 1).ToList();
            List<ICard> partisan2Cards = cards.Where(card => card.Data.Level == 2).ToList();

            partisan1Cards.Shuffle(NetworkGame.Seed);
            partisan2Cards.Shuffle(NetworkGame.Seed);

            section.Deck1 = partisan1Cards;
            section.Deck2 = partisan2Cards;

            section.Refill();

            Mode.Shop.AddSection(ShopType.Courtyard, section);
            Mode.RPC_InitializeShop(ShopType.Courtyard, Mode.Shop.SerializeSection(ShopType.Courtyard));
        }

        private void InitializeMarket(List<ICard> cards)
        {
            var groupedByName = cards.GroupBy(c => c.Data.Name).ToDictionary(g => g.Key, g => g.ToList());
            int index = 0;

            MarketSection section = new(groupedByName.Count);

            cards.Shuffle(NetworkGame.Seed);

            foreach (var kvp in groupedByName) {
                foreach (ICard card in kvp.Value)
                    section.CardPiles[index].Add(card);
                index++;
            }

            Mode.Shop.AddSection(ShopType.Market, section);
            Mode.RPC_InitializeShop(ShopType.Market, Mode.Shop.SerializeSection(ShopType.Market));
        }

        private void InitializeDeck(List<PlayerRef> playerRefs)
        {
            int baseCardsToDraw = 3;
            bool isFirst = true;

            foreach (PlayerRef playerRef in playerRefs) {
                PlayerController player = NetworkGame.GetPlayer(playerRef);

                if (player) {
                    int cardsToDraw = isFirst ? baseCardsToDraw - 1 : baseCardsToDraw;

                    player.RPC_DrawCards(cardsToDraw);
                }

                isFirst = false;
            }
        }

        #endregion

        #region Events

        protected override void OnInitialize(string data)
        {
            Dictionary<int, CardFamily> json = JsonConvert.DeserializeObject<Dictionary<int, CardFamily>>(data);
            Dictionary<PlayerRef, CardFamily> playerFamilies = new();

            foreach (var kvp in json)
                playerFamilies[PlayerRef.FromEncoded(kvp.Key)] = kvp.Value;
            List<PlayerRef> players = InitializePlayerTurnOrder(playerFamilies.Select(kvp => kvp.Key).ToList());

            InitializePlayers(players);
            InitializeCards(playerFamilies.Select(kvp => kvp.Value).ToList());
            InitializePlayerDecks(players);
            InitializeShop();
            InitializeDeck(players);
        }

        protected override void OnActivate() {}

        #endregion
    }
}
