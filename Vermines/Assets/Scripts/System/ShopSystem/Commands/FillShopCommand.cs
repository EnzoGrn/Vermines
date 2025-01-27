using System.Collections.Generic;
using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {
    using System.Linq;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;
    using Vermines.ShopSystem.Data;

    public class FillShopCommand : ICommand {

        private ShopData _OldShop;

        public void Execute()
        {
            _OldShop = GameDataStorage.Instance.Shop;

            GameDataStorage.Instance.Shop = FillShop();
        }

        public void Undo()
        {
            GameDataStorage.Instance.Shop = _OldShop;
        }

        /// <note>
        /// The usage of 'ToList()' function are here for create copy of list, and evitate the 'InvalidOperationException' when we try to modify the list.
        /// </note>
        private ShopData FillShop()
        {
            ShopData                  shop = GameDataStorage.Instance.Shop;
            List<ShopSection> shopSections = shop.Sections.Values.ToList();

            foreach (ShopSection shopSection in shopSections) {
                foreach (var slot in shopSection.AvailableCards.ToList()) {
                    if (slot.Value != null)
                        continue; // Already have a card in the slots.

                    ICard card = DrawCard(shopSection);

                    shopSection.AvailableCards[slot.Key] = card;
                }
            }

            return shop;
        }

        private ICard DrawCard(ShopSection shopSection)
        {
            if (shopSection.Deck.Count == 0) {
                shopSection.DiscardDeck.Reverse();
                shopSection.Deck.Merge(shopSection.DiscardDeck);

                if (shopSection.Deck.Count == 0) {
                    // TODO: Notify UI, there is no more card available in this sections

                    return null;
                }
            }
            return shopSection.Deck.Draw();
        }
    }
}
