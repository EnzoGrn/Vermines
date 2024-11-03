using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief The data of a card.
 *
 * @details This class is a ScriptableObject, so it can be created in the Unity Editor.
 * It contains all the data of a card, like its name, description, type, eloquence, souls, sprite and effects.
 *
 * Know that the principle aim of this class is to be a data container instantiated during runtime thanks to a JSON file, give by the server.
 */
[CreateAssetMenu(fileName = "New Card Configuration", menuName = "Card Configuration")]
public class CardData : ScriptableObject {

    public int ID = 0; // !< The ID of the card, when fetch during initialisation the id represent the number of examplar that cards have, and after initialisation that represent his ID in the game.

    public string Name;        // !< The name of the card (e.g. "Bard)
    public string Description; // !< The description of the card (e.g. "Gagnez 8E.")

    public CardType Type; // !< The type of the card (e.g. CardType.Bee)

    public int Eloquence = -1; // !< The eloquence of the card (e.g. 14) (The eloquence is the cost of the card in the market) (-1 if the card has no cost)
    public int Souls;     // !< The souls of the card (e.g. 25) (The souls are the number of souls that the card gives when it is sacrificed)

    public string SpriteName;
    public Sprite Sprite; // !< The sprite of the card (e.g. The image of the card, nothing is displayed if it is null)

    // TODO: Maybe one day, the effect will be list for allow a card to have multiple same type of effect

    // All e.g. of this part are example of description, they will be develop as a class inheriting from the corresponding effect class
    // You can have an effect that does nothing and like just played a sound or a visual effect
    public PassiveEffect   PassiveEffect;   // !< The passive effect of the card (e.g. "Vous pouvez retirer du jeu 1 outil de votre défausse ou de votre main pour gagner 5A.")
    public TurnStartEffect TurnStartEffect; // !< The turn start effect of the card (e.g. "Gagnez 8E.")
    public PlayedEffect    PlayedEffect;    // !< The played effect of the card
    public DiscardEffect   DiscardEffect;   // !< The discard effect of the card (e.g. "Défaussez le villageois pour gagner 2E.")
    public SacrificeEffect SacrificeEffect; // !< The sacrifice effect of the card

    public bool IsAnonyme = false; // !< If the card is anonyme, it will display only the back of the card (by default, it's not anonyme)

    public void ChangeSprite()
    {
        Sprite = Resources.Load<Sprite>("Sprites/Card/" + Type.ToString().Trim('\"') + "/" + SpriteName);
    }
}
