using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CardFactory {

    #region Attributes

    static private int            _CardCount = 0;
    static private JSONCardParser _Parser    = new();

    #endregion

    #region Methods

    static private CardData CardParser(string cardConfig)
    {
        try {
            return _Parser.Parse(cardConfig);
        } catch (System.Exception e) {
            Debug.LogError("Error: " + e.Message);

            return null;
        }
    }

    static private ICard CardBuilder(CardData cardConfig)
    {
        CardBuilder builder = new();

        if (cardConfig == null)
            return null;
        return builder
            .CreateCard(cardConfig.Type)
            .SetCardData(cardConfig)
            .Build();
    }

    static public CardData CreateCardData(string cardConfig)
    {
        return CardParser(cardConfig);
    }

    static public ICard CreateCard(string cardConfig /*, bool isAnonyme = false */) // Maybe in the future... Currently thinking on the best way to do that.
    {
        CardData config = CardParser(cardConfig);

        _CardCount++;

        return CardBuilder(config);
    }

    static public ICard CreateCard(CardData cardConfig /*, bool isAnonyme = false */) // Maybe in the future... Currently thinking on the best way to do that.
    {
        _CardCount++;

        return CardBuilder(cardConfig);
    }

    #endregion

    #region Getters

    static public int CardCount
    {
        get => _CardCount;
        set => _CardCount = value;
    }

    #endregion
}
