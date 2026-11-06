using UnityEngine;

namespace Vermines.UI.Card {

    using Vermines.CardSystem.Elements;
    using Vermines.UI.GameTable;

    /// <summary>
    /// Drop zone of the discard. When a <see cref="PUI_DiscardPile"/> is set, the slot no longer shows the last card itself:
    /// the cards it receives are handed to the pile, which shows the whole discard.
    /// </summary>
    public class DiscardCardSlot : CardSlotBase {

        /// <summary>
        /// Pile showing every discarded card (optional: without it the slot shows the last card, as before).
        /// </summary>
        [SerializeField]
        private PUI_DiscardPile _Pile;

        /// <summary>
        /// Pile showing every discarded card (may be null).
        /// </summary>
        public PUI_DiscardPile Pile => _Pile;

        public override void Init(ICard card, bool isNew, ICardClickHandler clickHandler = null)
        {
            if (_Pile == null) {
                base.Init(card, isNew, clickHandler);

                return;
            }

            if (card != null)
                _Pile.AddPending(card);
        }
    }
}
