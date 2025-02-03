using System.Collections.Generic;
using Defective.JSON;

namespace Vermines.ShopSystem.Data {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using System.Linq;

    [System.Serializable]
    public class ShopSection {

        public ShopSection(int slots)
        {
            AvailableCards = new Dictionary<int, ICard>();

            for (int i = 0; i < slots; i++)
                AvailableCards.Add(i, null);
            Deck = new List<ICard>();
            DiscardDeck = new List<ICard>();
        }

        private ShopSection() {}

        /// <summary>
        /// Serialize the shop section
        /// </summary>
        /// <example>
        /// {
        ///   slots: [
        ///     { 0: null },
        ///     { 1:    5 },
        ///     { 2: null },
        ///     { 3:   45 },
        ///     { 4:  155 },
        ///     { 5:    1 }
        ///   ],
        ///   deck: [10,11,12,16,18]
        ///   discard: [2,3,1,7,8]
        /// }
        /// </example>
        /// <returns>Section data</returns>
        public JSONObject Serialize(JSONObject json)
        {
            // -- Slots --

            json.AddField("slots", new JSONObject(JSONObject.Type.Array));

            foreach (var slotData in AvailableCards) {
                JSONObject slot = new(JSONObject.Type.Object);

                slot.AddField(slotData.Key.ToString(), slotData.Value == null ? "null" : slotData.Value.ID.ToString());
                json.GetField("slots").Add(slot);
            }

            // -- Deck --

            json.AddField("deck", new JSONObject(JSONObject.Type.Array));

            foreach (ICard card in Deck) {
                if (card == null)
                    continue;
                json.GetField("deck").Add(card.ID);
            }

            // -- Discard Deck --

            json.AddField("discard", new JSONObject(JSONObject.Type.Array));

            foreach (ICard card in DiscardDeck) {
                if (card == null)
                    continue;
                json.GetField("discard").Add(card.ID);
            }

            return json;
        }
        
        public void Deserialize(JSONObject json)
        {
            // -- Slots --

            JSONObject slotsJson = json.GetField("slots");

            foreach (JSONObject slotData in slotsJson) {
                int.TryParse(slotData.keys[0], out int slot);
                string cardID = slotData[slot.ToString()].stringValue;

                AvailableCards[slot] = CardSetDatabase.Instance.GetCardByID(cardID);
            }

            // -- Deck --

            JSONObject deckJson = json.GetField("deck");

            foreach (JSONObject deck in deckJson) {
                ICard card = CardSetDatabase.Instance.GetCardByID(deck.ToString());

                if (card != null)
                    Deck.Add(card);
            }

            // -- Discard Deck --

            JSONObject discardJson = json.GetField("discard");

            foreach (JSONObject discard in discardJson) {
                ICard card = CardSetDatabase.Instance.GetCardByID(discard.ToString());

                if (card != null)
                    DiscardDeck.Add(card);
            }
        }

        public bool HasCard(int cardID)
        {
            foreach (var slot in AvailableCards)
                if (slot.Value != null && slot.Value.ID == cardID)
                    return true;
            return false;
        }

        public bool HasCardAtSlot(int slot)
        {
            return AvailableCards[slot] != null;
        }

        /// <summary>
        /// Call this methods only if the card can be buy, because it's remove it from the shop
        /// </summary>
        /// <param name="cardID">The card wanted</param>
        /// <returns>The card bought</returns>
        public ICard BuyCard(int cardID)
        {
            foreach (var slot in AvailableCards) {
                if (slot.Value != null && slot.Value.ID == cardID) {
                    ICard card = slot.Value;

                    AvailableCards[slot.Key] = null;

                    return card;
                }
            }
            return null;
        }

        public ICard BuyCardAtSlot(int slot)
        {
            if (AvailableCards.TryGetValue(slot, out ICard card)) {
                AvailableCards[slot] = null;

                return card;
            }
            return null;
        }

        public ShopSection DeepCopy()
        {
            return new ShopSection {
                AvailableCards = new Dictionary<int, ICard>(this.AvailableCards),
                Deck           = new List<ICard>(this.Deck),
                DiscardDeck    = new List<ICard>(this.DiscardDeck)
            };
        }

        /// <summary>
        /// Current cards available in the shop (e.g. Courtyard or Market).
        /// </summary>
        public Dictionary<int, ICard> AvailableCards;

        /// <summary>
        /// Deck of cards for this section.
        /// </summary>
        public List<ICard> Deck;

        /// <summary>
        /// The discarded cards for this section.
        /// </summary>
        public List<ICard> DiscardDeck;
    }
}
