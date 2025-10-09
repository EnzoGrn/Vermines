using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

public struct CardSelectedEffectContext {

    public CardType Type;
    public ICard Card;

    public CardSelectedEffectContext(CardType type, ICard card)
    {
        Type = type;
        Card = card;
    }
}
