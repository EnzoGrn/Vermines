using System;

namespace Vermines.Core.Settings {

    public enum OptionsValueType : byte {
        None,
        Bool,
        Float,
        Int,
        String,
    }

    [Serializable]
    public struct OptionsValueFloat {
        public float Value;
        public float MinValue;
        public float MaxValue;
    }

    [Serializable]
    public struct OptionsValueInt {
        public int Value;
        public int MinValue;
        public int MaxValue;
    }

    [Serializable]
    public unsafe struct OptionsValue {

        #region Attributes

        public string Key;

        public OptionsValueType Type;

        public bool BoolValue;
        public string StringValue;

        public OptionsValueFloat FloatValue;
        public OptionsValueInt IntValue;

        #endregion

        #region Constructor

        public OptionsValue(string key, OptionsValueType type) : this()
        {
            Key  = key;
            Type = type;
        }

        public OptionsValue(string key, bool value) : this()
        {
            Key       = key;
            Type      = OptionsValueType.Bool;
            BoolValue = value;
        }

        public OptionsValue(string key, int value) : this()
        {
            Key            = key;
            Type           = OptionsValueType.Int;
            IntValue.Value = value;
        }

        public OptionsValue(string key, float value) : this()
        {
            Key              = key;
            Type             = OptionsValueType.Float;
            FloatValue.Value = value;
        }

        public OptionsValue(string key, string value) : this()
        {
            Key         = key;
            Type        = OptionsValueType.String;
            StringValue = value;
        }

        #endregion

        public bool Equals(OptionsValue other)
        {
            if (Key != other.Key || Type != other.Type)
                return false;
            switch (Type) {
                case OptionsValueType.Bool:
                    return BoolValue == other.BoolValue;
                case OptionsValueType.Float:
                    return FloatValue.Value == other.FloatValue.Value;
                case OptionsValueType.Int:
                    return IntValue.Value == other.IntValue.Value;
                case OptionsValueType.String:
                    return StringValue == other.StringValue;
                default:
                    break;
            }

            return true;
        }
    }
}
