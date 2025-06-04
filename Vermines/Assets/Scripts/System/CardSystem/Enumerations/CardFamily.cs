namespace Vermines.CardSystem.Enumerations {

    /// <summary>
    /// Enumeration of the different card family.
    /// </summary>
    /// <note>
    /// A card have a family only if he has a partisan type.
    /// </note>
    public enum CardFamily {
        None     = -1, // This card has no family (no partisan type).
        Bee      =  0,
        Cricket  =  1,
        Firefly  =  2,
        Fly      =  3,
        Ladybug  =  4,
        Mosquito =  5,
        Scarab   =  6,
        Count // Don't use it in UI, it's just a counter.
    }
}
