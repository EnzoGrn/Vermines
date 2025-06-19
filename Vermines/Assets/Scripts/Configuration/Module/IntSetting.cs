using UnityEngine;
using System;

namespace Vermines.Configuration.Network {

    public class IntSetting : SettingBase<int> {

        public int MinValue { get; }
        public int MaxValue { get; }

        public IntSetting(string fieldName, int minValue, int maxValue, int defaultValue = 0) : base(fieldName, defaultValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override int ApplyTo(ref GameSettingsData config)
        {
            var field = typeof(GameSettingsData).GetField(FieldName);

            if (field != null && field.FieldType == typeof(int)) {
                field.SetValueDirect(__makeref(config), Value);

                return Value;
            } else {
                Debug.LogError(
                    $"[IntSetting] Field '{FieldName}' not found or is not of type int in GameSettingsData. Please check the field name and type.\n" +
                    $"Expected type: int, Actual type: {field?.FieldType}\n" +
                    $"Config Type: {config.GetType()}"
                );

                throw new InvalidOperationException($"Field '{FieldName}' not found or is not of type int in GameSettingsData. Please check the field name and type.");
            }
        }

        public override void LoadFrom(GameSettingsData config)
        {
            var field = typeof(GameSettingsData).GetField(FieldName);

            if (field != null && field.FieldType == typeof(int)) {
                Value = (int)field.GetValue(config);
            } else {
                Debug.LogError(
                    $"[IntSetting] Field '{FieldName}' not found or is not of type int in GameSettingsData. Please check the field name and type.\n" +
                    $"Expected type: int, Actual type: {field?.FieldType}\n" +
                    $"Config Type: {config.GetType()}"
                );
            }
        }

        public override bool EqualsData(GameSettingsData config)
        {
            var field = typeof(GameSettingsData).GetField(FieldName);

            if (field != null && field.FieldType == typeof(int)) {
                return (int)field.GetValue(config) == Value;
            } else {
                Debug.LogError(
                    $"[IntSetting] Field '{FieldName}' not found or is not of type int in GameSettingsData. Please check the field name and type.\n" +
                    $"Expected type: int, Actual type: {field?.FieldType}\n" +
                    $"Config Type: {config.GetType()}"
                );

                return false;
            }
        }
    }
}
