using System.Collections.Generic;

namespace Vermines.Gameplay.Cards {
    using System.Linq;
    using Vermines.CardSystem.Elements;

    /// <summary>
    /// Store all cards played by the player
    /// </summary>
    public class CardTracker {

        private readonly HashSet<ICard> _Cards = new();

        public void AddCard(ICard card)
        {
            _Cards.Add(card);
        }

        public bool HasCard(ICard card)
        {
            return _Cards.FirstOrDefault(c => c.Data.Name == card.Data.Name) != null;
        }

        public void Reset()
        {
            _Cards.Clear();
        }
    }
}
