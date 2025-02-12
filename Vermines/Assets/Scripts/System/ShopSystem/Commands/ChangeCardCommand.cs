using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;

    public class ChangeCardCommand : ICommand {

        private ShopData _Shop;
        private ShopData _OldShop;

        private readonly ShopType _ShopType;

        private readonly int _SlotIndex;

        public ChangeCardCommand(ShopData shop, ShopType shopType, int slotIndex)
        {
            _Shop      = shop;
            _OldShop   = shop.DeepCopy();
            _ShopType  = shopType;
            _SlotIndex = slotIndex;
        }

        public bool Execute()
        {
            _OldShop = _Shop.DeepCopy();

            ShopData shop = ChangeCard(_ShopType, _SlotIndex);

            if (shop == null)
                return false;
            _Shop = shop;

            return true;
        }

        public void Undo()
        {
            _Shop.Sections = _OldShop.Sections;
        }

        private ShopData ChangeCard(ShopType type, int slotIndex)
        {
            if (!_Shop.Sections.ContainsKey(type))
                return null;
            if (!_Shop.Sections[type].AvailableCards.ContainsKey(slotIndex))
                return null;
            ICard card = _Shop.Sections[type].AvailableCards[slotIndex];

            if (card == null)
                return null;
            _Shop.Sections[type].DiscardDeck.Add(card);
            _Shop.Sections[type].AvailableCards[slotIndex] = DrawCard(_Shop.Sections[type]);

            return _Shop;
        }

        private ICard DrawCard(ShopSection shopSection)
        {
            if (shopSection.Deck.Count == 0) {
                shopSection.DiscardDeck.Reverse();
                shopSection.Deck.Merge(shopSection.DiscardDeck);
            }

            return shopSection.Deck.Draw();
        }
    }
}
