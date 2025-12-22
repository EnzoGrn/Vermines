using System.Collections.Generic;
using System.Linq;

namespace Vermines.Player {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;

    public struct PlayerDeck {

        private int Seed;

        public List<ICard> Deck { get; set; }
        public List<ICard> Hand { get; set; }
        public List<ICard> Discard { get; set; }
        public List<ICard> ToolDiscard { get; set; }
        public List<ICard> Graveyard { get; set; }
        public List<ICard> PlayedCards { get; set; }
        public List<ICard> Equipments { get; set; }

        public void Initialize(int seed)
        {
            Seed = seed;

            Deck        = new List<ICard>();
            Deck        = new List<ICard>();
            Hand        = new List<ICard>();
            Discard     = new List<ICard>();
            ToolDiscard = new List<ICard>();
            Graveyard   = new List<ICard>();
            PlayedCards = new List<ICard>();
            Equipments  = new List<ICard>();
        }

        #region Deck Manipulation

        public readonly ICard Draw()
        {
            // -- Check if I can take a card from the deck
            if (Deck.Count == 0) {
                if (Discard.Count == 0)
                    return null;
                Deck.Merge(Discard);
                Deck.Shuffle(Seed);

                // TODO: Clear the discard pile visual
                GameEvents.OnDiscardShuffled.Invoke();
            }

            ICard card = Deck.Draw();

            Hand.Add(card);

            return card;
        }

        public readonly ICard DiscardCard(int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card != null && Hand.Contains(card)) {
                Hand.Remove(card);

                if (card.Data.Type == CardType.Tools)
                    ToolDiscard.Add(card);
                else
                    Discard.Add(card);
                return card;
            }

            return null;
        }

        public readonly void MergeToolDiscard(int seed)
        {
            if (ToolDiscard.Count > 0) {
                Deck.Merge(ToolDiscard);
                Deck.Shuffle(seed);
            }
        }

        public readonly ICard PlayCard(int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card != null && Hand.Contains(card)) {
                Hand.Remove(card);
                PlayedCards.Add(card);

                return card;
            }

            return null;
        }

        #endregion

        #region Copy

        public PlayerDeck DeepCopy()
        {
            return new() {
                Deck        = new List<ICard>(this.Deck),
                Hand        = new List<ICard>(this.Hand),
                Discard     = new List<ICard>(this.Discard),
                Graveyard   = new List<ICard>(this.Graveyard),
                PlayedCards = new List<ICard>(this.PlayedCards),
                Equipments  = new List<ICard>(this.Equipments)
            };
        }

        #endregion

        #region Serialization

        public readonly string Serialize()
        {
            static string SerializeList(string name, List<ICard> list)
            {
                if (list == null || list.Count == 0)
                    return $"{name}[]";
                return $"{name}[{string.Join(",", list.Select(card => card.ID))}]";
            }

            string[] parts = new[] {
                SerializeList("Deck", Deck),
                SerializeList("Hand", Hand),
                SerializeList("Discard", Discard),
                SerializeList("ToolDiscard", ToolDiscard),
                SerializeList("Graveyard", Graveyard),
                SerializeList("PlayedCards", PlayedCards),
                SerializeList("Equipments", Equipments)
            };

            return string.Join(";", parts);
        }

        static public PlayerDeck Deserialize(string data)
        {
            PlayerDeck deck = new();

            // Separate each section of the data
            string[] sections = data.Split(';');

            // Parse each section
            foreach (string deckSection in sections) {
                if (deckSection.StartsWith("Deck[") && deckSection.EndsWith("]")) {
                    string content = deckSection[5..^1];

                    deck.Deck = CardSetDatabase.Instance.GetCardByIds(content);
                } else if (deckSection.StartsWith("Hand[") && deckSection.EndsWith("]")) {
                    string content = deckSection[5..^1];

                    deck.Hand = CardSetDatabase.Instance.GetCardByIds(content);
                } else if (deckSection.StartsWith("Discard[") && deckSection.EndsWith("]")) {
                    string content = deckSection[8..^1];

                    deck.Discard = CardSetDatabase.Instance.GetCardByIds(content);
                } else if (deckSection.StartsWith("Graveyard[") && deckSection.EndsWith("]")) {
                    string content = deckSection[10..^1];

                    deck.Graveyard = CardSetDatabase.Instance.GetCardByIds(content);
                } else if (deckSection.StartsWith("ToolDiscard[") && deckSection.EndsWith("]")) {
                    string content = deckSection[12..^1];

                    deck.ToolDiscard = CardSetDatabase.Instance.GetCardByIds(content);
                } else if (deckSection.StartsWith("PlayedCards[") && deckSection.EndsWith("]")) {
                    string content = deckSection[12..^1];

                    deck.PlayedCards = CardSetDatabase.Instance.GetCardByIds(content);
                } else if (deckSection.StartsWith("Equipments[") && deckSection.EndsWith("]")) {
                    string content = deckSection[11..^1];

                    deck.Equipments = CardSetDatabase.Instance.GetCardByIds(content);
                }
            }

            return deck;
        }

        #endregion
    }
}
