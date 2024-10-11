using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief The different types of cards that can be in the game.
 * 
 * @warning If you add a new card type here, don't forget to add a folder in Card Asset.
 * This folder needs to have the same name as the card type.
 * And contains a 'Background.jpg' and a 'Icon.png'.
 */
public enum CardType {
    Bee = 0,
    Cricket = 1,
    Equipment = 2,
    Firefly = 3,
    Fly = 4,
    Ladybug = 5,
    Mosquito = 6,
    Scarab = 7,
    Tools = 8,

    Count // This is just a count of the number of card types
}

static public class CardTypeMethods {

    /**
     * @brief Transform a string into a CardType.
     * 
     * @param cardType The string to transform.
     * 
     * @return The CardType value.
     */
    // TODO: Try to find a better way to do this
    static public CardType ToEnum(string cardType)
    {
        CardType cardTypeValue = (CardType)System.Enum.Parse(typeof(CardType), cardType, true);

        return cardTypeValue;

        /*if (cardType.Equals("Bee"))
            return CardType.Bee;
        else if (cardType.Equals("Cricket"))
            return CardType.Cricket;
        else if (cardType.Equals("Equipment"))
            return CardType.Equipment;
        else if (cardType.Equals("Firefly"))
            return CardType.Firefly;
        else if (cardType.Equals("Fly"))
            return CardType.Fly;
        else if (cardType.Equals("Ladybug"))
            return CardType.Ladybug;
        else if (cardType.Equals("Mosquito"))
            return CardType.Mosquito;
        else if (cardType.Equals("Scarab"))
            return CardType.Scarab;
        else if (cardType.Equals("Tools"))
            return CardType.Tools;
        Debug.Assert(false, "The card type [" + cardType + "] is not recognized.");

        return CardType.Count;*/
    }
}

