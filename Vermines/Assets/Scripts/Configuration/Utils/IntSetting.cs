using Defective.JSON;
using UnityEngine;

namespace Vermines.Config.Utils {

    [System.Serializable]
    public class IntSetting : ASetting<int> {

        [SerializeField]
        public int Value;
        
        [SerializeField]
        public int MinValue;
        
        [SerializeField]
        public int MaxValue;

        public IntSetting(string name, int value, int minValue, int maxValue, string category)
        {
            Name     = name;
            Value    = value;
            MinValue = minValue;
            MaxValue = maxValue;
            Category = category;
            Type     = SettingType.Int;
        }

        public override void RestrictionCheck(int value)
        {
            if (value < MinValue || value > MaxValue)
                throw new System.Exception($"Value of {Name} must be between {MinValue} and {MaxValue}");
            else if (MinValue > MaxValue)
                throw new System.Exception("Min value cannot be greater than max value");
        }

        public override void Serialize(JSONObject json)
        {
            JSONObject fieldJson = new(JSONObject.Type.Object);

            fieldJson.AddField("type", (int)Type); // Enum
            fieldJson.AddField("name", Name);
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

            if (json.HasField("value"))
            {
                Value = (int)json.GetField("value").intValue;
            }

            Debug.Log("Name -> " + Name + ", Value" +  Value);
        }
    }
}
