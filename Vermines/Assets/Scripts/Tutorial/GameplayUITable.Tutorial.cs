using UnityEngine.UI;

namespace Vermines.UI.Screen {

    using Vermines.Tutorial.Integration;
    using Vermines.CardSystem.Elements;
    using Vermines.UI.Card;
    using Vermines.Player;

    public partial class GameplayUITable {

        public Button TutorialCloseButton => _CloseButton;

        public CardSlotBase TutorialDiscardSlot => discardSlot;

        /// <summary>
        /// A lesson can start with partisans already on the table (VerminesPlayerSetup.StartingPartisans):
        ///   the table only displays the cards dropped on it, so the missing ones are shown here.
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
}
