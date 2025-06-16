namespace Vermines.Configuration.Network {

    public abstract class SettingBase<T> : ISetting<T> {

        public string Tooltip { get; protected set; }

        public string FieldName { get; protected set; }

        public T Value { get; set; }

        protected SettingBase(string fieldName, string tooltip, T defaultValue = default(T))
        {
            FieldName = fieldName;
            Tooltip = tooltip;
            Value = defaultValue;
        }

        public abstract void ApplyTo(ref GameSettingsData config);
        public abstract void LoadFrom(GameSettingsData config);
        public abstract bool EqualsData(GameSettingsData config);
    }
}
