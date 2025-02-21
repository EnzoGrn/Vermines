namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;

    public class CardBuilder {

        /// <summary>
        /// Represent the elements to build.
        /// </summary>
        /// <note>
        /// You can only build one card at a time.
        /// To get the instance of the current card, call 'Build'.
        /// </note>
        private ICard _Card;

        /// <summary>
        /// Create a new card to build.
        /// BECARE, if a card is already created, it will be replaced (so every data of the old card will be lost).
        /// </summary>
        /// <note>
        /// Call 'Build' to get the card instance.
        /// </note>
        /// <param name="type">The type of card</param>
        /// <returns>Builder instance</returns>
        public CardBuilder CreateCard(CardType type)
        {
            // -- Reset the card to build
            _Card = null;

            // -- Create the card depending on the type
            _Card = type switch {
                CardType.Partisan  => new PartisanCard(),
                CardType.Equipment => new EquipmentCard(),
                CardType.Tools     => new ToolCard(),
                _ => throw new System.Exception("Card type not found.")
            };

            return this;
        }

        /// <summary>
        /// Link the data to the card.
        /// </summary>
        /// <note>
        /// Call 'CreateCard' before.
        /// Call 'Build' to get the card instance.
        /// </note>
        /// <returns>Builder instance</returns>
        public CardBuilder SetCardData(CardData data)
        {
            if (_Card != null)
                _Card.Data = data;
            return this;
        }

        /// <summary>
        /// Build the card and return it.
        /// </summary>
        /// <returns>The instance of the card</returns>
        public ICard Build()
        {
            return _Card;
        }
    }
}
