namespace Vermines.UI.Screen {

    using Vermines.CardSystem.Elements;
    using Vermines.Player;
    using Vermines.Tutorial.Integration;
    using Vermines.UI.Card;

    public partial class GameplayUITable {

        public UnityEngine.UI.Button TutorialCloseButton => _CloseButton;

        public CardSlotBase TutorialDiscardSlot => discardSlot;

        /// <summary>
        /// A lesson can start with partisans already on the table (ScriptedPlayerSetup.startingPartisans):
        /// the table only displays the cards dropped on it, so the missing ones are shown here.
        /// </summary>
        partial void ShowUser()
        {
            if (!TutorialMatch.IsActive || PlayerController.Local == null)
                return;
            foreach (ICard card in PlayerController.Local.PlayedCards) {
                if (card == null || IsDisplayed(card))
                    continue;
                TableCardSlot slot = partisanSlots.Find(s => s.CardDisplay == null || !s.CardDisplay.gameObject.activeSelf);

                if (slot == null)
                    return;
                slot.Init(card, false, new TableCardClickHandler(slot.GetIndex()));
            }
        }

        private bool IsDisplayed(ICard card)
        {
            foreach (TableCardSlot slot in partisanSlots) {
                if (slot.CardDisplay != null && slot.CardDisplay.gameObject.activeSelf && slot.CardDisplay.Card != null && slot.CardDisplay.Card.ID == card.ID)
                    return true;
            }

            return false;
        }
    }

    public partial class GameplayUIShop {

        public UnityEngine.GameObject TutorialCloseButton => _CloseButton;

        /// <summary>
        /// "shop_opened" / "shop_closed" with the arg courtyard or market:
        /// more reliable for the steps than the camera, whose way back does not always report its arrival.
        /// </summary>
        partial void ShowUser()
        {
            if (TutorialMatch.IsActive)
                Vermines.Tutorial.TutorialEventBus.Publish(TutorialGameEventRelay.ShopOpened, TutorialShopName(_shopType));
        }

        partial void HideUser()
        {
            if (TutorialMatch.IsActive)
                Vermines.Tutorial.TutorialEventBus.Publish(TutorialGameEventRelay.ShopClosed, TutorialShopName(_shopType));
        }

        private static string TutorialShopName(Vermines.ShopSystem.Enumerations.ShopType shopType)
        {
            return shopType == Vermines.ShopSystem.Enumerations.ShopType.Courtyard ? "courtyard" : shopType == Vermines.ShopSystem.Enumerations.ShopType.Market ? "market" : shopType.ToString();
        }
    }

    public partial class GameplayUIGainSummary {

        public UnityEngine.UI.Button TutorialCloseButton => _CloseButton;

        /// <summary>
        /// Tutorial only: closes the summary as the close button does (next phase).
        /// </summary>
        public void TutorialClose()
        {
            OnCloseButtonPressed();
        }
    }
}

namespace Vermines.UI.Popup {

    public partial class GameplayUIDualButtonPopup {

        public UnityEngine.UI.Button TutorialCancelButton => _ButtonLeft;

        public UnityEngine.UI.Button TutorialConfirmButton => _ButtonRight;
    }
}
