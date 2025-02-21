namespace Vermines.CardSystem.Enumerations {

    /// <summary>
    /// Enumeration of the different card types.
    /// </summary>
    /// <note>
    /// If a card has a partisan type, he also need a family.
    /// </note>
    public enum CardType {
        None      = -1, // This card has no type.
        Partisan  =  0,
        Equipment =  1,
        Tools     =  2,
        Count // Don't use it in UI, it's just a counter.
    }
}
