using Vermines.CardSystem.Elements;

namespace Vermines.UI.Shop
{
    public class ShopCardEntry
    {
        public ICard Data;
        public bool IsNew;
        public int StackCount;

        public ShopCardEntry(ICard data, bool isNew = false, int stackCount = 1)
        {
            Data = data;
            IsNew = isNew;
            StackCount = stackCount;
        }
    }
}
