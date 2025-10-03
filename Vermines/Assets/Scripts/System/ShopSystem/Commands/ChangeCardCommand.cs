using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Utilities;
    using Vermines.ShopSystem.Data;

    public class CLIENT_ChangeCardCommand : ACommand {

        private ShopArgs _Parameters;

        private ShopSection _Backup;

        public CLIENT_ChangeCardCommand(ShopArgs parameters)
        {
            _Parameters = parameters;
        }

        public override CommandResponse Execute()
        {
            ShopSection shop = _Parameters.Shop.Sections[_Parameters.ShopType];

            _Backup = shop.DeepCopy();

            ICard oldCard = shop.AvailableCards[_Parameters.SlotIndex];

            shop.DiscardDeck.Add(oldCard);

            ICard newCard = DrawCard(shop);

            shop.AvailableCards[_Parameters.SlotIndex] = newCard;

            return new CommandResponse(CommandStatus.Success, "", _Parameters.ShopType.ToString(), oldCard.Data.Name, newCard.Data.Name);
        }

        public override void Undo()
        {
            _Parameters.Shop.Sections[_Parameters.ShopType] = _Backup;
        }

        private ICard DrawCard(ShopSection shopSection)
        {
            if (shopSection.Deck.Count == 0) {
                shopSection.DiscardDeck.Reverse();
                shopSection.Deck.Merge(shopSection.DiscardDeck);

                if (shopSection.Deck.Count == 0)
                    return null;
            }

            return shopSection.Deck.Draw();
        }
    }

    /// <summary>
    /// Execute only on state authority (server or host).
    /// </summary>
    public class ADMIN_CheckChangeCardCommand : ACommand {

        public ShopArgs Args;

        public ADMIN_CheckChangeCardCommand(ShopArgs shopArgs)
        {
            Args = shopArgs;
        }

        public override CommandResponse Execute()
        {
            // 0. Check if the shop exist
            if (Args.Shop == null)
                return new CommandResponse(CommandStatus.CriticalError, "Shop_ShopNotExist", Args.ShopType.ToString());

            // 1. Check if the shop and slot exist
            if (!Args.Shop.Sections.ContainsKey(Args.ShopType))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_ShopNotExist", Args.ShopType.ToString());
            if (!Args.Shop.HasCardAtSlot(Args.ShopType, Args.SlotIndex))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_SlotEmpty", Args.ShopType.ToString());
            return new CommandResponse(CommandStatus.Success, string.Empty);
        }
    }
}
