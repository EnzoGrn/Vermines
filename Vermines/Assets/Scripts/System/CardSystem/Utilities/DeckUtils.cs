using System.Collections.Generic;
using System.Linq;

namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Elements;

    static public class DeckUtils {

        static public void Shuffle(this List<ICard> deck, int seed)
        {
            System.Random rand = new(seed);

            deck = deck.OrderBy(card => rand.Next()).ToList();
        }

        static public void Merge(this List<ICard> deck, List<ICard> cards)
        {
            deck.AddRange(cards.Where(card => card != null));
            cards.Clear();
        }
    }
}
