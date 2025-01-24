using OMGG.DesignPattern;
using UnityEngine;

namespace Vermines.ShopSystem.Commands.Internal {

    using Vermines.ShopSystem.Data;

    public class SyncShopCommand : ICommand {

        private readonly string   _Data;

        private ShopData _OldShop;

        public SyncShopCommand(string data)
        {
            _Data = data;
        }

        public void Execute()
        {
            _OldShop = GameDataStorage.Instance.Shop;

            GameDataStorage.Instance.Shop = CreateNewShop(_Data);
        }

        public void Undo()
        {
            GameDataStorage.Instance.Shop = _OldShop;
        }

        private ShopData CreateNewShop(string data)
        {
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            shop.Initialize(GameManager.Instance.Config.NumerOfCardsProposed);
            shop.Deserialize(data);

            return shop;
        }
    }
}
