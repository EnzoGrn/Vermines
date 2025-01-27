using OMGG.DesignPattern;
using UnityEngine;

namespace Vermines.ShopSystem.Commands.Internal {

    using Vermines.ShopSystem.Data;
    using Vermines.Config;

    public class SyncShopCommand : ICommand {

        private ShopData _Shop;
        private ShopData _OldShop;

        private readonly string _Data;

        private readonly GameConfig _Config;

        public SyncShopCommand(ShopData shopToSync, string data, GameConfig config)
        {
            _Shop    = shopToSync;
            _OldShop = shopToSync?.DeepCopy() ?? null;
            _Data    = data;
            _Config  = config;
        }

        public void Execute()
        {
            _OldShop = _Shop?.DeepCopy() ?? null;
            
            SyncShop(_Shop, _Data, _Config);
        }

        public void Undo()
        {
            _Shop = _OldShop;
        }

        private void SyncShop(ShopData shopToSync, string data, GameConfig config)
        {
            if (shopToSync == null)
                shopToSync = ScriptableObject.CreateInstance<ShopData>();
            shopToSync.Clear();
            shopToSync.Initialize(config.NumerOfCardsProposed);
            shopToSync.Deserialize(data);
        }
    }
}
