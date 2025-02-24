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
            _OldShop   = shop?.DeepCopy() ?? null;
            _ShopType  = shopType;
            _SlotIndex = slotIndex;
        }

        public CommandResponse Execute()
        {
            _OldShop = _Shop?.DeepCopy() ?? null;

            CommandResponse response = ChangeCard(_ShopType, _SlotIndex);

            return response;
        }

        public void Undo()
        {
            _Shop.Sections = _OldShop.Sections;
        }

        private CommandResponse ChangeCard(ShopType type, int slotIndex)
        {
            if (!_Shop.Sections.ContainsKey(type))
                return new CommandResponse(CommandStatus.Invalid, $"Shop does not have a section of type {type}.");
            if (!_Shop.Sections[type].AvailableCards.ContainsKey(slotIndex))
                return new CommandResponse(CommandStatus.Invalid, $"Shop does not have a slot {slotIndex} in section {type}.");
            ICard card = _Shop.Sections[type].AvailableCards[slotIndex];

            if (card == null)
                return new CommandResponse(CommandStatus.Invalid, $"Shop does not have a card in slot {slotIndex} in section {type}.");
            _Shop.Sections[type].DiscardDeck.Add(card);
            _Shop.Sections[type].AvailableCards[slotIndex] = DrawCard(_Shop.Sections[type]);

            return new CommandResponse(CommandStatus.Success, $"Card in slot {slotIndex} in section {type} has been changed.");
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
