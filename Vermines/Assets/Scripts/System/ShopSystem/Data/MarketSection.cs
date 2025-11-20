using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using Fusion;

namespace Vermines.ShopSystem.Data {

    using Vermines.CardSystem.Elements;

    [JsonObject(MemberSerialization.OptIn)]
    public class MarketSection : ShopSectionBase, IEnumerable<ICard> {

        #region Attributes

        [JsonProperty]
        public Dictionary<int, List<ICard>> CardPiles;

        [JsonProperty]
        private readonly int _Slots;

        public int Slots => _Slots;

        #endregion

        #region Constructor & Copy Constructor

        public MarketSection(int slots)
        {
            CardPiles = new();

            _Slots = slots;

            for (int i = 0; i < slots; i++)
                CardPiles[i] = new List<ICard>();
        }

        public override ShopSectionBase DeepCopy()
        {
            MarketSection section = new(_Slots) {
                CardPiles = new Dictionary<int, List<ICard>>(this.CardPiles)
            };

            return section;
        }

        #endregion

        #region Getters & Setters

        public override bool HasCard(int cardId)
        {
            foreach (var pile in CardPiles.Values) {
                if (pile.Count > 0 && pile[^1].ID == cardId)
                    return true;
            }

            return false;
        }

        public override void SetFree(bool free)
        {
            foreach (var pile in CardPiles.Values) {
                foreach (ICard card in pile)
                    card.Data.IsFree = free;
            }
        }

        /// <summary>
        /// Return the first card of each piles.
        /// </summary>
        public IEnumerator<ICard> GetEnumerator()
        {
            foreach (var pile in CardPiles.Values) {
                if (pile.Count > 0)
                    yield return pile[^1];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Methods

        public override ICard BuyCard(int cardId)
        {
            if (!HasCard(cardId))
                return null;
            foreach (var kvp in CardPiles) {
                var pile = kvp.Value;

                if (pile.Count > 0 && pile[^1].ID == cardId)
                {
                    ICard card = pile[^1];

                    pile.RemoveAt(pile.Count - 1);

                    return card;
                }
            }

            return null;
        }

        public override void ApplyReduction(int amount)
        {
            foreach (var pile in CardPiles.Values) {
                foreach (ICard card in pile)
                    card.Data.EloquenceReduction(amount);
            }
        }

        public override void RemoveReduction(int amount)
        {
            foreach (var pile in CardPiles.Values) {
                foreach (ICard card in pile)
                    card.Data.RemoveReduction(amount);
            }
        }

        public override void ReturnCard(ICard card)
        {
            bool returned = false;

            foreach (var pile in CardPiles.Values) {
                if (pile.Count > 0 && pile[^1].Data.Name == card.Data.Name) {
                    pile.Add(card);

                    returned = true;

                    break;
                }
            }

            if (!returned) {
                foreach (var pile in CardPiles.Values) {
                    if (pile.Count == 0) {
                        pile.Add(card);

                        returned = true;

                        break;
                    }
                }
            }

            if (returned)
                card.Owner = PlayerRef.None;
        }

        #endregion
    }
}
