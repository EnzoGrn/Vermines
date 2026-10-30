using UnityEngine;

namespace Vermines.UI.Screen {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.Tutorial.Integration;

    public partial class GameplayUIShop {

        public GameObject TutorialCloseButton => _CloseButton;

        /// <summary>
        /// "shop_opened" / "shop_closed" with the arg courtyard or market:
        ///   more reliable for the steps than the camera, whose way back does not always report its arrival.
        /// </summary>
        partial void ShowUser()
        {
            if (TutorialMatch.IsActive)
                CelticEdge.Tutorial.TutorialEventBus.Publish(VerminesTutorialEvents.ShopOpened, TutorialShopName(_shopType));
        }

        partial void HideUser()
        {
            if (TutorialMatch.IsActive)
                CelticEdge.Tutorial.TutorialEventBus.Publish(VerminesTutorialEvents.ShopClosed, TutorialShopName(_shopType));
        }

        private static string TutorialShopName(ShopType shopType)
        {
            return shopType == ShopType.Courtyard ? "courtyard" : shopType == ShopType.Market ? "market" : shopType.ToString();
        }
    }
}
