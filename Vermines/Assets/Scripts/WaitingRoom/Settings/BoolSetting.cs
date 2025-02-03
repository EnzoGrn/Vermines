using Defective.JSON;

namespace Vermines.Settings
{
    [System.Serializable]
    public class BoolSetting : ASetting
    {
        public bool Value;

        public BoolSetting(string name, bool value, string category)
        {
            Value = value;

            Name = name;
            Category = category;
        }

        public override void RestrictionCheck<T>(T Value)
        {
            if (Value is not bool)
            {
                throw new System.Exception($"Value of {Name} must be a boolean");
            }
        }

        public override void Serialize(JSONObject json)
        {
            JSONObject fieldJson = new(JSONObject.Type.Object);

            fieldJson.AddField("type", (int)Type); // Enum
            fieldJson.AddField("name", Name);
            fieldJson.AddField("category", Category);

            fieldJson.AddField("value", Value);

            json.Add(fieldJson);
        }

        public override void Deserialize(JSONObject json)
        {
            if (json.HasField("type"))
            {
                Type = (SettingType)json.GetField("type").intValue; // Convert JSON int to Enum
            }

            if (json.HasField("name"))
            {
                Name = json.GetField("name").stringValue;
            }

            if (json.HasField("category"))
            {
                Category = json.GetField("category").stringValue;
            }

            if (json.HasField("value"))
            {
                Value = json.GetField("value").boolValue;
            }
        }
    }
}

