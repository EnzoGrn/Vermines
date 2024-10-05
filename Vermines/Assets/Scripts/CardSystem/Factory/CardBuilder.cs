using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBuilder {

    private Card _Card; // <-- Card object to be built

    public CardBuilder CreateCard(CardType type)
    {
        switch (type) {
            case CardType.Tools:
                _Card = new ToolsCard();
                break;
            case CardType.Equipment:
                _Card = new EquipmentCard();
                break;
            case CardType.Bee:
            case CardType.Cricket:
            case CardType.Firefly:
            case CardType.Fly:
            case CardType.Ladybug:
            case CardType.Mosquito:
            case CardType.Scarab:
                _Card = new PartisanCard();
                break;
            default:
                break; // TODO: Anonymous Card
        }

        return this;
    }

    public CardBuilder SetCardData(CardData data)
    {
        _Card.Data = data;

        return this;
    }

    public Card Build()
    {
        return _Card;
    }
}
