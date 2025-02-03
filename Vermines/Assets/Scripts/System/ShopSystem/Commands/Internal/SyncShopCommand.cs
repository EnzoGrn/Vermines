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

        public bool Execute()
        {
            _OldShop = _Shop?.DeepCopy() ?? null;
            
            SyncShop(_Shop, _Data, _Config);

            return true;
        }

        public void Undo()
        {
            _Shop.Sections = _OldShop.Sections;
        }

        private void SyncShop(ShopData shopToSync, string data, GameConfiguration config)
        {
            if (shopToSync == null)
                shopToSync = ScriptableObject.CreateInstance<ShopData>();
            shopToSync.Clear();
            shopToSync.Initialize(ShopType.Market   , config.MaxMarketCards.Value);
            shopToSync.Initialize(ShopType.Courtyard, config.MaxCourtyardCards.Value);
            shopToSync.Deserialize(data);
        }
    }
}
