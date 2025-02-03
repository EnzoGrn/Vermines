
using Defective.JSON;

namespace Vermines.Settings
{
    [System.Serializable]
    public enum SettingType
    {
        Bool,
        Int,
    }

    [System.Serializable]
    public abstract class ASetting
    {
        public string Category;
        public string Name;
        
        public SettingType Type; // Store enum as int (Fusion supports int, not enums)

        public abstract void RestrictionCheck<T>(T Value);

        public abstract void Serialize(JSONObject json);

        public abstract void Deserialize(JSONObject json);
    }
}

