using Newtonsoft.Json;
using System;
using Vermines.CardSystem.Data;

namespace Vermines.CardSystem.Elements {

    public class CardJsonConverter : JsonConverter<ICard> {

        public override void WriteJson(JsonWriter writer, ICard value, JsonSerializer serializer)
        {
            if (value == null) {
                writer.WriteNull();

                return;
            }

            writer.WriteValue(value.ID);
        }

        public override ICard ReadJson(JsonReader reader, Type objectType, ICard existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            int     id = Convert.ToInt32(reader.Value);
            ICard card = CardSetDatabase.Instance.GetCardByID(id);

            return card;
        }
    }
}
