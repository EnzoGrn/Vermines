using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using Fusion;

namespace Vermines.ShopSystem.Data {

    using Vermines.CardSystem.Elements;

    [JsonObject(MemberSerialization.OptIn)]
    [CreateAssetMenu(menuName = "Vermines/Shops/MarketSection")]
    public class MarketSection : ShopSectionBase, IEnumerable<ICard> {

        #region Attributes

        public int Slots => _Slots;

        [JsonProperty, SerializeField]
        private int _Slots = 5;

        [JsonProperty]
        public Dictionary<int, List<ICard>> CardPiles = new();

        #endregion

        #region Constructor & Copy Constructor

        public override void Initialize()
        {
            if (CardPiles == null || CardPiles.Count == 0) {
                CardPiles = new();

                for (int i = 0; i < _Slots; i++)
                    CardPiles[i] = new();
            }
        }

        public override ShopSectionBase DeepCopy()
        {
            MarketSection section = Instantiate(this);

            section._Slots    = this._Slots;
            section.CardPiles = new Dictionary<int, List<ICard>>(this.CardPiles);

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

                if (pile.Count > 0 && pile[^1].ID == cardId) {
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
