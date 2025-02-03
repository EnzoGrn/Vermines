
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
    }
}
