using Vermines.CardSystem.Enumerations;
using Vermines.UI.Card;

namespace Vermines.UI
{
    /// <summary>
    /// Represents an equipment slot in the book UI.
    /// Displays a card visually using a CardDisplay prefab.
    /// </summary>
    public class DeckBookSlot : CardSlotBase
    {
        /// <summary>
        /// Clears the slot and hides its card.
        /// </summary>
        public void Clear()
        {
            ResetSlot();
        }

        /// <summary>
        /// Optionally configure the slot with index and accepted card type.
        /// </summary>
        /// <param name="index">Slot index.</param>
        public void Configure(int index)
        {
            SetIndex(index);
            SetInteractable(true); // usually no drag/drop for book view
        }
    }
}
