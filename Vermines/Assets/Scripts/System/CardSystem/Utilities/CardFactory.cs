namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;

    static public class CardFactory {

        /// <summary>
        /// The number of cards created.
        /// </summary>
        /// <note>
        /// Don't forget to reset the factory if you want to restart the count.
        /// </note>
        static private int _CardCount = 0;
        static public int CardCount
        {
            get => _CardCount;
            private set => _CardCount = value;
        }

        static public void Reset()
        {
            _CardCount = 0;
        }

        static private ICard CardBuilder(CardData data)
        {
            if (data == null)
                return null;
            CardBuilder builder = new();

            return builder.CreateCard(data.Type).SetCardData(data).Build();
        }

        static public ICard CreateCard(CardData data)
        {
            ICard card = CardBuilder(data);

            if (card != null)
                _CardCount++;
            return card;
        }
    }
}
