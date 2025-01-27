using System.Collections.Generic;
using System.Linq;
using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;
    using Vermines.ShopSystem.Data;

    public class FillShopCommand : ICommand {

        private ShopData _Shop;
        private ShopData _OldShop;

        public FillShopCommand(ShopData shopToFill)
        {
            _Shop    = shopToFill;
            _OldShop = shopToFill.DeepCopy();
        }

        public void Execute()
        {
            _OldShop = _Shop.DeepCopy();

            _Shop = FillShop();
        }

        public void Undo()
        {
            _Shop.Sections = _OldShop.Sections;
        }

        /// <note>
        /// The usage of 'ToList()' function are here for create copy of list, and evitate the 'InvalidOperationException' when we try to modify the list.
        /// </note>
        private ShopData FillShop()
        {
            List<ShopSection> shopSections = _Shop.Sections.Values.ToList();

            foreach (ShopSection shopSection in shopSections) {
                foreach (var slot in shopSection.AvailableCards.ToList()) {
                    if (slot.Value != null)
                        continue; // Already have a card in the slots.

                    ICard card = DrawCard(shopSection);

                    shopSection.AvailableCards[slot.Key] = card;
                }
            }

            return _Shop;
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
