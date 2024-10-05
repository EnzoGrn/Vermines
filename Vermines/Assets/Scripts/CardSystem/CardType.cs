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
    Bee,
    Cricket,
    Equipment,
    Firefly,
    Fly,
    Ladybug,
    Mosquito,
    Scarab,
    Tools,

    Count // This is just a count of the number of card types
}
