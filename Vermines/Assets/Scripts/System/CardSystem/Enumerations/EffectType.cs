namespace Vermines.CardSystem.Enumerations {

    public enum EffectType {
        None      = -1, // This card has no type.
        Play      =  0, // Starting turn effect.
        Discard   =  1, // On discard effect.
        Sacrifice =  2, // On sacrifice effect.
        Activate  =  3, // Starting turn effect + Choice of activation.
        Passive   =  4, // Passive effect.
    }
}
