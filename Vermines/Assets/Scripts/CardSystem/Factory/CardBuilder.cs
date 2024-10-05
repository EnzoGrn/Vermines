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
                _Card = new PartisanCard();
                break;
            case CardType.Cricket:
                _Card = new PartisanCard();
                break;
            case CardType.Firefly:
                _Card = new PartisanCard();
                break;
            case CardType.Fly:
                _Card = new PartisanCard();
                break;
            case CardType.Ladybug:
                _Card = new PartisanCard();
                break;
            case CardType.Mosquito:
                _Card = new PartisanCard();
                break;
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
        if (_Card != null)
            _Card.Data = data;

        return this;
    }

    public Card Build()
    {
        return _Card;
    }
}
