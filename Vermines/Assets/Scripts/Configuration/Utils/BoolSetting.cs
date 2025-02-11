
using Defective.JSON;
using UnityEngine;

namespace Vermines.Config.Utils {

    [System.Serializable]
    public class BoolSetting : ASetting<bool> {

        [SerializeField]
        public bool Value;

        public BoolSetting(string name, bool value, string category)
        {
            Name     = name;
            Value    = value;
            Category = category;
            Type     = SettingType.Bool;
        }

        public override void RestrictionCheck(bool Value) {}

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
                Value = json.GetField("value").boolValue;
            }
        }
    }
}
