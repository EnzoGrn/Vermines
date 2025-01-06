using UnityEngine;
using OMGG.DesignPattern;
using System.Collections.Generic;

namespace Vermines {

    /// <summary>
    /// The card database will be a centralised structure containing information on all the cards required for the game.
    /// It will be synchronised between the server and the clients.
    /// </summary>
    public class CardDataBase : MonoBehaviourSingleton<CardDataBase> {

        /// <summary>
        /// The dictionary containing all the cards in the game.
        /// 
        /// Key: The card ID,
        /// Value: Scriptable object containing the card data.
        /// </summary>
        public Dictionary<int, CardData> Cards { get; private set; } = new();

        /// <summary>
        /// Initializes the card database with the provided families and their cards.
        /// </summary>
        /// <param name="familiesToLoad">List of families to include in the database.</param>
        public void InitializeDatabase(List<CardType> familiesToLoad)
        {
            // Clear the current card database
            Cards.Clear();

            // Load cards dynamically based on the families
            foreach (var family in familiesToLoad) {
                LoadCardsFromFamily(family, 1);
                LoadCardsFromFamily(family, 2);
            }

            Debug.Log($"[CardDataBase]: Database initialized with {Cards.Count} cards.");
        }

        /// <summary>
        /// Loads all cards of a specific family into the database.
        /// </summary>
        /// <param name="family">The card family to load.</param>
        /// <param name="cardLevel">The card level (all cards have differents levels depending on their abilities).</param>
        private void LoadCardsFromFamily(CardType family, int cardLevel)
        {
            CardData[] familyCards = Resources.LoadAll<CardData>($"ScriptableObjects/Cards/Partisans/{cardLevel}/Family"); // TODO: Create a constant path in a Utils namespace

            foreach (var card in familyCards) {
                if (!Cards.ContainsKey(card.ID)) {
                    Cards.Add(card.ID, card);
                }
            }
        }

        /// <summary>
        /// Fetches a card by its ID.
        /// </summary>
        /// <param name="id">The card ID.</param>
        /// <returns>The corresponding CardData.</returns>
        public CardData GetCardByID(int id)
        {
            return Cards.TryGetValue(id, out var card) ? card : null;
        }
    }
}
