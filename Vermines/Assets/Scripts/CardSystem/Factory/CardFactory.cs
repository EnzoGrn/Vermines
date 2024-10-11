using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CardFactory {

    static private CardData CardParser(string cardConfig)
    {
        try {
            JSONCardParser parser = new();

            return parser.Parse(cardConfig);
        } catch (System.Exception e) {
            Debug.LogError("Error: " + e.Message);

            return null;
        }
    }

    static private Card CardBuilder(CardData cardConfig)
    {
        CardBuilder builder = new();

        if (cardConfig == null)
            return null;
        return builder
            .CreateCard(cardConfig.Type)
            .SetCardData(cardConfig)
            .Build();
    }

    static public Card CreateCard(string cardConfig /*, bool isAnonyme = false */) // Maybe in the future... Currently thinking on the best way to do that.
    {
        CardData config = CardParser(cardConfig);

        return CardBuilder(config);
    }

    static public Card CreateCard(CardData cardConfig /*, bool isAnonyme = false */) // Maybe in the future... Currently thinking on the best way to do that.
    {
        return CardBuilder(cardConfig);
    }
}
