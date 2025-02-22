using OMGG.DesignPattern;
using System.Linq;

namespace Vermines.ShopSystem.Commands
{

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;
    using Vermines.HUD.Card;
    using Vermines.ShopSystem.Data;
    using Vermines.Test;

    public class FillShopCommand : ICommand
    {

        private ShopData _Shop;
        private ShopData _OldShop;

        public FillShopCommand(ShopData shopToFill)
        {
            _Shop = shopToFill;
            _OldShop = shopToFill.DeepCopy();
        }

        public bool Execute()
        {
            _OldShop = _Shop.DeepCopy();

            _Shop = FillShop();

            return true;
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
            foreach (var shopSection in _Shop.Sections)
            {
                foreach (var slot in shopSection.Value.AvailableCards.ToList())
                {
                    if (slot.Value != null)
                        continue; // Already have a card in the slots.

                    ICard card = DrawCard(shopSection.Value);

                    shopSection.Value.AvailableCards[slot.Key] = card;

                    GameEvents.OnShopsEvents[shopSection.Key].Invoke(slot.Key, card);
                }


                if (!TestMode.IsTesting)
                    CardSpawner.Instance.UpdateSpecificShop(shopSection.Value.AvailableCards.ToDictionary(x => x.Key, x => x.Value), shopSection.Key);
            }

            return _Shop;
        }

        private ICard DrawCard(ShopSection shopSection)
        {
            if (shopSection.Deck.Count == 0)
            {
                shopSection.DiscardDeck.Reverse();
                shopSection.Deck.Merge(shopSection.DiscardDeck);

                if (shopSection.Deck.Count == 0)
                {
                    // TODO: Notify UI, there is no more card available in this sections

                    return null;
                }
            }
            return shopSection.Deck.Draw();
        }
    }
}