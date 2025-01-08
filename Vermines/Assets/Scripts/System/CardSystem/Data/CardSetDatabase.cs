using System.Collections.Generic;
using OMGG.DesignPattern;

namespace Vermines.CardSystem.Data {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;

    /// <summary>
    /// Singleton database, that store in game card set.
    /// Card set represent every card playable in this game.
    /// </summary>
    public class CardSetDatabase : Singleton<CardSetDatabase> {

        /// <summary>
        /// Set of card for this game.
        /// </summary>
        private List<ICard> _Cards;

        /// <summary>
        /// Function to initialize the card set database.
        /// For that we need every family that will be used in the game.
        /// </summary>
        /// <param name="families">Families used in the current game</param>
        public void Initialize(List<CardFamily> families)
        {
            CardFactory.Reset();

            CardLoader loader = new();

            loader.Initialize(families);

            _Cards = loader.Load();
        }

        /// <summary>
        /// Check if a card with this identifiers exist in the current set of card.
        /// </summary>
        /// <param name="id">The identifiers of the card</param>
        public bool CardExist(int id)
        {
            if (_Cards == null || _Cards.Count == 0)
                return false;
            foreach (ICard card in _Cards) {
                if (card.ID == id)
                    return true;
            }
            return false;
        }

        public ICard GetCardByID(int id)
        {
            if (_Cards == null || _Cards.Count == 0)
                return null;
            foreach (ICard card in _Cards)
                if (card.ID == id)
                    return card;
            return null;
        }

        public int Size => _Cards != null ? _Cards.Count : 0;

        /// <summary>
        /// Call this function to reset the database value.
        /// </summary>
        public new void Reset()
        {
            // To reset the singleton
            base.Reset();

            // To reset the database
            _Cards = new List<ICard>();
        }
    }
}
