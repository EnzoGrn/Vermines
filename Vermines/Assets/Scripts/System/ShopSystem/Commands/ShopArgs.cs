namespace Vermines.ShopSystem {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;

    public struct ShopArgs {

        public ShopData Shop;

        public ShopType ShopType;

        public int CardId;

        public ShopArgs(ShopData shop, ShopType shopType, int cardId)
        {
            Shop      = shop;
            ShopType  = shopType;
            CardId    = cardId;
        }
    }
}
