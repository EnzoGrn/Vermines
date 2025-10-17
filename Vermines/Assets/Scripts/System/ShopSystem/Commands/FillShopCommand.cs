using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {

    using Vermines.ShopSystem.Data;

    public class FillShopCommand : ACommand {

        private readonly ShopData _Shop;

        public FillShopCommand(ShopData shopToFill)
        {
            _Shop = shopToFill;
        }

        public override CommandResponse Execute()
        {
            foreach (var section in _Shop.Sections) {
                section.Value.Refill();

                GameEvents.OnShopRefilled.Invoke(section.Key, _Shop.GetDisplayCards(section.Key));
            }

            return new CommandResponse(CommandStatus.Success, "");
        }
    }
}
