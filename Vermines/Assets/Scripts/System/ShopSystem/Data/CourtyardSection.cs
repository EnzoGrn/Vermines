using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

namespace Vermines.ShopSystem.Data {
    using System.Linq;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;
    using Vermines.ShopSystem.Enumerations;

    [JsonObject(MemberSerialization.OptIn)]
    public class CourtyardSection : ShopSectionBase, IEnumerable<ICard> {

        #region Attributes

        [JsonProperty]
        private readonly int _Level1Slots;

        [JsonProperty]
        private readonly int _Level2Slots;

        [JsonProperty]
        public Dictionary<int, ICard> AvailableCards;

        [JsonProperty]
        public List<ICard> Deck1;

        [JsonProperty]
        public List<ICard> Deck2;

        #endregion

        #region Constructor & Copy Constructor

        public CourtyardSection(int level1Slots = 3, int level2Slots = 2)
        {
            AvailableCards = new();

            _Level1Slots = level1Slots;
            _Level2Slots = level2Slots;

            for (int i = 0; i < (level1Slots + level2Slots); i++)
                AvailableCards.Add(i, null);
            Deck1 = new List<ICard>();
            Deck2 = new List<ICard>();
        }

        public override ShopSectionBase DeepCopy()
        {
            CourtyardSection section = new(_Level1Slots, _Level2Slots) {
                AvailableCards = new Dictionary<int, ICard>(this.AvailableCards),

                Deck1 = new List<ICard>(this.Deck1),
                Deck2 = new List<ICard>(this.Deck2),
            };

            return section;
        }

        #endregion

        #region Getters & Setters

        public override bool HasCard(int cardId)
        {
            var slot = AvailableCards.FirstOrDefault(x => x.Value?.ID == cardId);

            if (!slot.Equals(default(KeyValuePair<int, ICard>)))
                return true;
            return false;
        }

        public override void SetFree(bool free)
        {
            foreach (var slot in AvailableCards) {
                if (slot.Value != null)
                    slot.Value.Data.IsFree = free;
            }

            foreach (ICard card in Deck1)
                card.Data.IsFree = free;
            foreach (ICard card in Deck2)
                card.Data.IsFree = free;
        }

        /// <summary>
        /// Returns the card of each slot.
        /// </summary>
        public IEnumerator<ICard> GetEnumerator()
        {
            foreach (var slot in AvailableCards)
                yield return slot.Value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Methods

        private int FindSlotEmpty()
        {
            int index = 0;

            foreach (var slot in AvailableCards) {
                if (slot.Value == null)
                    return index;
                index++;
            }

            return index;
        }

        public ICard AddCard(int level)
        {
            List<ICard> deck = level == 1 ? Deck1 : Deck2;

            if (deck.Count == 0)
                return null;
            ICard card = deck.Draw();

            int index = FindSlotEmpty();

            AvailableCards[index] = card;

            return card;
        }

        public ICard NextCard(int level)
        {
            List<ICard> deck = level == 1 ? Deck1 : Deck2;

            if (deck.Count == 0)
                return null;
            return deck[0];
        }

        public override ICard BuyCard(int cardId)
        {
            if (!HasCard(cardId))
                return null;
            var slot = AvailableCards.FirstOrDefault(x => x.Value?.ID == cardId);

            if (slot.Equals(default(KeyValuePair<int, ICard>)))
                return null;
            int  index = slot.Key;
            ICard card = slot.Value;

            int lastIndex = AvailableCards.Count - 1;

            for (int i = index; i < lastIndex; i++)
                AvailableCards[i] = AvailableCards[i + 1];
            AvailableCards.Remove(lastIndex);

            return card;
        }

        public override void ApplyReduction(int amount)
        {
            foreach (var slot in AvailableCards)
                slot.Value?.Data.EloquenceReduction(amount);
            foreach (ICard card in Deck1)
                card.Data.EloquenceReduction(amount);
            foreach (ICard card in Deck2)
                card.Data.EloquenceReduction(amount);
        }

        public override void RemoveReduction(int amount)
        {
            foreach (var slot in AvailableCards)
                slot.Value?.Data.RemoveReduction(amount);
            foreach (ICard card in Deck1)
                card.Data.RemoveReduction(amount);
            foreach (ICard card in Deck2)
                card.Data.RemoveReduction(amount);
        }

        #endregion
    }
}
