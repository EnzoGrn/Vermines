using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CardFactory {

    static private CardData CardParser(string cardConfig)
    {
        JSONCardParser parser = new();

        return parser.Parse(cardConfig);
    }

    static private Card CardBuilder(CardData cardConfig)
    {
        CardBuilder builder = new();

        return builder
            .CreateCard(cardConfig.Type)
            .SetCardData(cardConfig)
            .Build();
    }

    static public Card CreateCard(string cardConfig)
    {
        CardData config = CardParser(cardConfig);

        return CardBuilder(config);
    }

    static public Card CreateCard(CardData cardConfig)
    {
        return CardBuilder(cardConfig);
    }
}
