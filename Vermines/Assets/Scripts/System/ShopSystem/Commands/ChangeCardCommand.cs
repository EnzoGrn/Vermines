using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.ShopSystem.Data;

    public class CLIENT_ChangeCardCommand : ACommand {

        private ShopArgs _Parameters;

        private ShopSectionBase _Backup;

        public CLIENT_ChangeCardCommand(ShopArgs parameters)
        {
            _Parameters = parameters;
        }

        public override CommandResponse Execute()
        {
            ICard oldCard = CardSetDatabase.Instance.GetCardByID(_Parameters.CardId);
            ShopSectionBase shop = _Parameters.Shop.Sections[_Parameters.ShopType];

            _Backup = shop.DeepCopy();

            ICard newCard = shop.ChangeCard(oldCard);

            return new CommandResponse(CommandStatus.Success, "", _Parameters.ShopType.ToString(), oldCard.ID.ToString(), newCard.ID.ToString());
        }

        public override void Undo()
        {
            _Parameters.Shop.Sections[_Parameters.ShopType] = _Backup;
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

            // 1. Check if it's a shop that can have card replacement.
            if (Args.ShopType != Enumerations.ShopType.Courtyard)
                return new CommandResponse(CommandStatus.CriticalError, "Shop_Replace_NotAllowShop", Args.ShopType.ToString());

            // 2. Check if the shop contains the card.
            if (!Args.Shop.HasCard(Args.ShopType, Args.CardId))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_CardNotExist", Args.ShopType.ToString());
            return new CommandResponse(CommandStatus.Success, string.Empty);
        }
    }
}
