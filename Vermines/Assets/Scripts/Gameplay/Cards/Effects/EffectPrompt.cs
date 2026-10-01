using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

namespace Vermines.Gameplay.Cards.Effect
{
    /// <summary>
    /// The kind of choice an effect asks the local player to make.
    /// One value per prompt screen; Copy and Remove will be added when migrated.
    /// </summary>
    public enum EffectPromptKind
    {
        Reborn,
        Copy,
        Remove
    }

    /// <summary>
    /// Describes a choice an effect needs from the local player.
    /// Pure data: logic raises it, the UI decides how to show it.
    /// </summary>
    public readonly struct EffectPrompt
    {
        public readonly EffectPromptKind Kind;
        public readonly CardType CardType;
        public readonly ICard Source;

        public EffectPrompt(EffectPromptKind kind, CardType cardType, ICard source)
        {
            Kind = kind;
            CardType = cardType;
            Source = source;
        }
    }
}
