using Defective.JSON;
using System;
using UnityEngine;

namespace Vermines.Config.Utils {

    [Serializable]
    public abstract class ASettingBase {

        [Serializable]
        public enum SettingType {
            Bool,
            Int,
        }

        [SerializeField, HideInInspector]
        public string Category;

        [SerializeField, HideInInspector]
        public string Name;

        [SerializeField, HideInInspector]
        public SettingType Type;

        public abstract void Serialize(JSONObject json);

        public abstract void Deserialize(JSONObject json);
    }

    [System.Serializable]
    public abstract class ASetting<T> : ASettingBase where T : struct {

        public abstract void RestrictionCheck(T Value);
    }
}
