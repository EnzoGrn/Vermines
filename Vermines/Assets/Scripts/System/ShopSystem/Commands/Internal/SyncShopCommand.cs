using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands.Internal {

    using Vermines.ShopSystem.Data;

    public class SyncShopCommand : ACommand {

        private ShopData _Shop;

        private readonly string _Data;

        public SyncShopCommand(ShopData shopToSync, string data)
        {
            _Shop     = shopToSync;
            _Data     = data;
        }

        public override CommandResponse Execute()
        {
            ShopData newShop = ShopData.Deserialize(_Data);

            _Shop.CopyFrom(newShop);

            if (_Shop == null)
                return new CommandResponse(CommandStatus.Failure, $"Failed to sync the shop.");
            return new CommandResponse(CommandStatus.Success, $"Shop synced.");
        }
    }
}
