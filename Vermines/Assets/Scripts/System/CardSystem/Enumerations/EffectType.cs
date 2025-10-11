using System;

namespace Vermines.CardSystem.Enumerations {

    [Flags]
    public enum EffectType {
        None = 0, // This card has no type.

        /// <summary>
        /// Effect that is automatically handle during the gain phase.
        /// </summary>
        Play = 1 << 0, // 00000001

        /// <summary>
        /// Effect that is handle when the card is discarded.
        /// </summary>
        Discard = 1 << 1, // 00000010

        /// <summary>
        /// Effect that is handle when the card is sacrified.
        /// </summary>
        Sacrifice = 1 << 2, // 00000100

        /// <summary>
        /// Effect that can be activated by the player during his gain phase.
        /// </summary>
        Activate = 1 << 3, // 00001000

        /// <summary>
        /// Effect that is always active while the card is in play.
        /// (In Vermines): This card is active at the start of your turn and does not require activation.
        ///                And desactivate when the card leaves play or if it's not your turn.
        /// </summary>
        Passive = 1 << 4, // 00010000

        /// <summary>
        /// Effect that is handle when an other card is sacrified.
        /// </summary>
        OnOtherSacrifice = 1 << 5, // 00100000

        /// <summary>
        /// Effect that is handle when an card is recycled.
        /// </summary>
        Recycle = 1 << 6, // 1000000

        /// <summary>
        /// Effect that is handle when another card is recycled.
        /// </summary>
        OnOtherRecycle = 1 << 7, // 10000000
    }
}
