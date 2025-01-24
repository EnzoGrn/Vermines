using System.Collections.Generic;
using System.Linq;
using Fusion;

namespace Vermines.Player {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Utilities;

    public struct PlayerData : INetworkStruct {

        [Networked, Capacity(24)]
        public string Nickname
        {
            get => default;
            set {}
        }

        public PlayerRef PlayerRef;

        public bool IsConnected;

        #region In-game Data

        public int Eloquence;
        public int Souls;

        public CardFamily Family;

        #endregion

        public PlayerData(PlayerRef player)
        {
            PlayerRef   = player;
            IsConnected = false;
            Eloquence   = 0;
            Souls       = 0;
            Family      = CardFamily.None;
            Nickname    = player.ToString();
        }
    }

    public struct PlayerDeck {

        public List<ICard> Deck;
        public List<ICard> Hand;
        public List<ICard> Discard;
        public List<ICard> Graveyard;

        public PlayerDeck(int deckSize = 0)
        {
            Deck =  deckSize == 0 ? new List<ICard>() : new List<ICard>(deckSize);

            Hand      = new List<ICard>();
            Discard   = new List<ICard>();
            Graveyard = new List<ICard>();
        }

        #region Deck Manipulation

        public void Draw()
        {
            // -- Check if I can take a card from the deck
            if (Deck.Count == 0) {
                if (Discard.Count == 0) {
                    // TODO: Maybe alert the player that is no more have card in his deck.

                    return;
                }
                Deck.Merge(Discard);
                Deck.Shuffle(GameManager.Instance.Config.Seed);
            }
            ICard card = Deck.Last();

            Deck.Remove(card);
            Hand.Add(card);
        }

        #endregion

        #region Serialization

        public readonly string Serialize()
        {
            string serializedPlayerDeck = string.Empty;

            // Decks serializer
            serializedPlayerDeck += $"Deck[{string.Join(","      , Deck.Select(card      => card.ID))}]";
            serializedPlayerDeck += $";Hand[{string.Join(","     , Hand.Select(card      => card.ID))}]";
            serializedPlayerDeck += $";Discard[{string.Join(","  , Discard.Select(card   => card.ID))}]";
            serializedPlayerDeck += $";Graveyard[{string.Join(",", Graveyard.Select(card => card.ID))}]";

            return serializedPlayerDeck;
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
                }
            }

            return deck;
        }

        #endregion
    }
}
