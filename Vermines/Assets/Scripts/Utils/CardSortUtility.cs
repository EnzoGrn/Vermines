using System.Collections.Generic;
using System.Linq;
using Vermines.CardSystem.Elements;

namespace Vermines {

    public static class CardSortUtility {
        
        public static List<ICard> SortDeckPerType(IEnumerable<ICard> deck)
        {
            return deck.OrderBy(c => c.Data.Type).ThenBy(c => c.ID).ToList();
        }
    }
}
