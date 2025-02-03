using Defective.JSON;
using Fusion;
using System;
using System.Diagnostics;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using static System.Collections.Specialized.BitVector32;

namespace Vermines.Settings
{
    [System.Serializable]
    public class IntSetting : ASetting
    {
        public int Value;
        public int MinValue;
        public int MaxValue;

        public IntSetting(string name, int value, int minValue, int maxValue, string category)
        {
            Name = name;
            Category = category;
            Type = SettingType.Int; ;

            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void RestrictionCheck<T>(T value)
        {
            // Check if T is an int
            if (value is not int)
            {
                throw new System.Exception("Value may be wrong.");
            }

            int intValue = (int)(object)value;

            if (intValue < MinValue || intValue > MaxValue)
            {
                throw new System.Exception($"Value of {Name} must be between {MinValue} and {MaxValue}");
            }
            else if (MinValue > MaxValue)
            {
                throw new System.Exception("Min value cannot be greater than max value");
            }
        }

        public override void Serialize(JSONObject json)
        {
            JSONObject fieldJson = new(JSONObject.Type.Object);

            fieldJson.AddField("type", (int)Type); // Enum
            fieldJson.AddField("name", Name);
            fieldJson.AddField("category", Category);
            
            fieldJson.AddField("value", Value);
            fieldJson.AddField("minValue", MinValue);
            fieldJson.AddField("maxValue", MaxValue);

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
                Value = (int)json.GetField("value").intValue;
            }

            if (json.HasField("minValue"))
            {
                MinValue = (int)json.GetField("minValue").intValue;
            }

            if (json.HasField("maxValue"))
            {
                MaxValue = (int)json.GetField("maxValue").intValue;
            }
        }
    }
}

