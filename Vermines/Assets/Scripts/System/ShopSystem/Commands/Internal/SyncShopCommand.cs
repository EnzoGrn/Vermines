using OMGG.DesignPattern;
using UnityEngine;

namespace Vermines.ShopSystem.Commands.Internal {

    using Vermines.ShopSystem.Data;
    using Vermines.Config;
    using Vermines.ShopSystem.Enumerations;

    public class SyncShopCommand : ICommand {

        private ShopData _Shop;
        private ShopData _OldShop;

        private readonly string _Data;

        private readonly GameConfiguration _Config;

        public SyncShopCommand(ShopData shopToSync, string data, GameConfiguration config)
        {
            _Shop    = shopToSync;
            _OldShop = shopToSync?.DeepCopy() ?? null;
            _Data    = data;
            _Config  = config;
        }

        public CommandResponse Execute()
        {
            _OldShop = _Shop?.DeepCopy() ?? null;
            
            _Shop = SyncShop(_Shop, _Data, _Config);

            if (_Shop == null)
                return new CommandResponse(CommandStatus.Failure, $"Failed to sync the shop.");
            return new CommandResponse(CommandStatus.Success, $"Shop synced.");
        }

        public void Undo()
        {
            _Shop.Sections = _OldShop.Sections;
        }

        private ShopData SyncShop(ShopData shopToSync, string data, GameConfiguration config)
        {
            if (shopToSync == null)
                shopToSync = ScriptableObject.CreateInstance<ShopData>();
            shopToSync.Clear();
            shopToSync.Initialize(ShopType.Market   , config.MaxMarketCards.Value);
            shopToSync.Initialize(ShopType.Courtyard, config.MaxCourtyardCards.Value);
            shopToSync.Deserialize(data);

            return shopToSync;
        }
    }
}
