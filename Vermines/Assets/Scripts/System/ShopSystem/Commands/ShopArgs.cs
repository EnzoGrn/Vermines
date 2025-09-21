namespace Vermines.ShopSystem {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;

    public struct ShopArgs {

        public ShopData Shop;

        public ShopType ShopType;

        public int SlotIndex;

        public ShopArgs(ShopData shop, ShopType shopType, int slotIndex)
        {
            Shop      = shop;
            ShopType  = shopType;
            SlotIndex = slotIndex;
        }
    }
}
