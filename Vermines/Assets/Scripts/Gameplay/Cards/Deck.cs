using UnityEngine;
using System.Collections.Generic;
using System;
using Fusion;

namespace Vermines {

    public static class Deck {

        /// <summary>
        /// Shuffles the deck
        /// </summary>
        /// <param name="deck">The deck to shuffle</param>
        /// <param name="count">The count of cards in the deck</param>
        public static void Shuffle(NetworkArray<int> deck, int count)
        {
            System.Random random = new();
            int len = count;

            for (; len > 1; --len) {
                int k = random.Next(len + 1);

                (deck[len], deck[k]) = (deck[k], deck[len]);
            }
        }

        /// <summary>
        /// Add a card to the deck
        /// </summary>
        /// <param name="deck">The deck to add the card to</param>
        /// <param name="cardID">The card ID to add</param>
        /// <param name="count">The count of cards in the deck</param>
        public static void AddCard(NetworkArray<int> deck, int cardID, ref int count)
        {
            if (count >= deck.Length)
                return;
            deck[count++] = cardID;
        }

        /// <summary>
        /// Remove a card from the deck
        /// </summary>
        /// <param name="deck">The deck to remove the card from</param>
        /// <param name="cardID">The card ID to remove</param>
        /// <param name="count">The count of cards in the deck</param>
        public static void RemoveCard(NetworkArray<int> deck, int cardID, ref int count)
        {
            for (int i = 0; i < count; ++i) {
                if (deck[i] == cardID) {
                    deck[i] = deck[count - 1];
                    deck[count - 1] = -1;

                    --count;

                    break;
                }
            }
        }
    }
}
