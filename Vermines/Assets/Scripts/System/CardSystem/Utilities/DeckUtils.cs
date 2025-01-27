using System.Collections.Generic;
using System.Linq;

namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Elements;

    static public class DeckUtils {

        static public void Shuffle(this List<ICard> deck, int seed)
        {
            System.Random rand = new(seed);

            for (int i = 0; i < deck.Count; i++) {
                ICard      temp = deck[i];
                int randomIndex = rand.Next(i, deck.Count);

                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        static public void Merge(this List<ICard> deck, List<ICard> cards)
        {
            deck.AddRange(cards.Where(card => card != null));
            cards.Clear();
        }

        static public void Reverse(this List<ICard> deck)
        {
            deck.Reverse();
        }

        static public ICard Draw(this List<ICard> deck)
        {
            ICard card = deck[0];

            deck.RemoveAt(0);

            return card;
        }
    }
}
