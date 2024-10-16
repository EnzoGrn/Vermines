using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines {

    public class PlayerData : MonoBehaviour {

        /*
         * @brief Singleton pattern for the PlayerData class.
         * This instance is used to store all data of the player.
         * And it store only on local, so other player can't access to it.
         *
         * @note It's this instance, that we want to send to the server.
         */
        public static PlayerData Instance;

        /*
         * @brief Data of the player.
         */
        public Data Data;

        /*
         * @brief Function called when the object is enabled.
         */
        private void OnEnable()
        {
            Data = new();

            PlayerData.Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        /*
         * @brief Function that convert the player data to a string.
         * The string is in JSON format.
         */
        public string DataToString()
        {
            return JsonUtility.ToJson(Data);
        }
    }

    /*
     * @brief Data of the player.
     */
    public class Data {

        // -- Player's Profile -- //

        /*
         * @brief The profile is not the profile of the player in the game.
         * It's the nickname and the playerID of the player in the server.
         * Their are use for recognize all the player in the party.
         */
        public Config.PlayerProfile Profile;

        // -- Player's Stats (Money & Victory Points) -- //

        /*
         * @brief The current eloquence of the player.
         * It use when the player try to buy something in the market or when a card give him eloquence.
         */
        public int Eloquence = 0;

        /*
         * @brief The current souls of the player.
         * The souls are the victory point of the player.
         * In the classic rules the souls needed for win the game is 100.
         */
        public int Souls = 0;

        /*
         * @brief The family of the player.
         * The family of a player allow him to earns 5 extra point when he sacrifices a card of this family.
         * In the classic rules, their can't be two players with the same family.
         * 
         * @warning If the player already have a family 'None', it's an error.
         * So the GameManager need to handle it, and give him a new family, for allow the player to play.
         */
        public CardType Family = CardType.None;

        // -- Every Player's Deck -- //

        // @Warning: This part will may be change in the future, because right now, that only erase the old deck and create a new one.

        /*
         * @brief The deck containing all the cards of the player's hand.
         */
        public Deck HandDeck;

        /*
         * @brief The deck containing all the cards of the player.
         * When it's empty, we refill it with the discard deck,
         * by merging the two decks in one, and shuffle it.
         */
        public Deck Deck;

        /*
         * @brief The deck containing all the discarded cards of the player.
         * In the screen we can only saw the last card of this deck,
         * and when their is no more card in his deck,
         * the player merge his deck with his discard deck, and shuffle it.
         */
        public Deck DiscardDeck;

        /*
         * @brief The deck containing all the sacrificed cards that the player has made.
         * It can be use to get the amount of Bee the player has sacrificed for example.
         */
        public Deck SacrifiedDeck;
    }
}
