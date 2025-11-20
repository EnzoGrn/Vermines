using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

namespace Vermines.ShopSystem.Data {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;

    [JsonObject(MemberSerialization.OptIn)]
    public class CourtyardSection : ShopSectionBase, IEnumerable<ICard> {

        #region Attributes

        [JsonProperty]
        private int _Level1Slots;

        [JsonProperty]
        private int _Level2Slots;

        [JsonProperty]
        public Dictionary<int, ICard> AvailableCards;

        [JsonProperty]
        public List<ICard> Deck1;

        [JsonProperty]
        public List<ICard> Deck2;

        [JsonProperty]
        public List<ICard> Discard1;

        [JsonProperty]
        public List<ICard> Discard2;

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

            Discard1 = new List<ICard>();
            Discard2 = new List<ICard>();
        }

        public override ShopSectionBase DeepCopy()
        {
            CourtyardSection section = new(_Level1Slots, _Level2Slots) {
                AvailableCards = new Dictionary<int, ICard>(this.AvailableCards),

                Deck1 = new List<ICard>(this.Deck1),
                Deck2 = new List<ICard>(this.Deck2),

                Discard1 = new List<ICard>(this.Discard1),
                Discard2 = new List<ICard>(this.Discard2)
            };

            return section;
        }

        #endregion

        #region Getters & Setters

        public override bool HasCard(int cardId)
        {
            foreach (var slot in AvailableCards) {
                if (slot.Value != null && slot.Value.ID == cardId)
                    return true;
            }

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
            foreach (ICard card in Discard1)
                card.Data.IsFree = free;
            foreach (ICard card in Discard2)
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

        public override ICard BuyCard(int cardId)
        {
            if (!HasCard(cardId))
                return null;
            foreach (var slot in AvailableCards) {
                if (slot.Value != null && slot.Value.ID == cardId) {
                    ICard card = slot.Value;

                    AvailableCards[slot.Key] = null;

                    return card;
                }
            }

            return null;
        }

        public override ICard ChangeCard(ICard card)
        {
            int slotIndex = -1;

            foreach (var kvp in AvailableCards) {
                if (kvp.Value != null && kvp.Value.ID == card.ID) {
                    slotIndex = kvp.Key;

                    break;
                }
            }

            if (slotIndex == -1)
                return null;
            ICard oldCard = AvailableCards[slotIndex];

            AvailableCards[slotIndex] = null;

            if (oldCard.Data.Level == 1)
                Discard1.Add(oldCard);
            else
                Discard2.Add(oldCard);
            ICard newCard = Draw(oldCard.Data.Level);

            AvailableCards[slotIndex] = newCard;

            return newCard;
        }

        private ICard Draw(int level)
        {
            if (level == 1) {
                if (Deck1.Count == 0) {
                    Discard1.Reverse();
                    Deck1.Merge(Discard1);
                }

                return Deck1.Draw();
            }

            if (Deck2.Count == 0) {
                Discard2.Reverse();
                Deck2.Merge(Discard2);
            }

            return Deck2.Draw();
        }

        public override void Refill()
        {
            for (int i = 0; i < AvailableCards.Count; i++) {
                if (AvailableCards[i] != null)
                    continue;
                ICard card = Draw(i >= _Level1Slots ? 2 : 1);

                AvailableCards[i] = card;
            }
        }

        public override void ApplyReduction(int amount)
        {
            foreach (var slot in AvailableCards)
                slot.Value?.Data.EloquenceReduction(amount);
            foreach (ICard card in Deck1)
                card.Data.EloquenceReduction(amount);
            foreach (ICard card in Deck2)
                card.Data.EloquenceReduction(amount);
            foreach (ICard card in Discard1)
                card.Data.EloquenceReduction(amount);
            foreach (ICard card in Discard2)
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
            foreach (ICard card in Discard1)
                card.Data.RemoveReduction(amount);
            foreach (ICard card in Discard2)
                card.Data.RemoveReduction(amount);
        }

        #endregion
    }
}
