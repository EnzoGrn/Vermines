using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

namespace Vermines.Gameplay.Cards.Effect
{
    public enum EffectPromptKind
    {
        Reborn,
        Copy,
        Remove,
        Sacrifice,
        Discard,
        Spend
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

        // Spend prompts only.
        public readonly DataType DataToSpend;
        public readonly DataType DataToEarn;
        public readonly int Multiplicator;

        public EffectPrompt(EffectPromptKind kind, CardType cardType, ICard source)
        {
            Kind = kind;
            CardType = cardType;
            Source = source;
            DataToSpend = default;
            DataToEarn = default;
            Multiplicator = 0;
        }

        private EffectPrompt(ICard source, DataType dataToSpend, DataType dataToEarn, int multiplicator)
        {
            Kind = EffectPromptKind.Spend;
            CardType = CardType.None;
            Source = source;
            DataToSpend = dataToSpend;
            DataToEarn = dataToEarn;
            Multiplicator = multiplicator;
        }

        public static EffectPrompt Spend(ICard source, DataType dataToSpend, DataType dataToEarn, int multiplicator)
            => new EffectPrompt(source, dataToSpend, dataToEarn, multiplicator);
    }
}
