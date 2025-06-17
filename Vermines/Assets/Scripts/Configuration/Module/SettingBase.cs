namespace Vermines.Configuration.Network {

    public abstract class SettingBase<T> : ISetting<T> {

        public string FieldName { get; protected set; }

        public T Value { get; set; }

        protected SettingBase(string fieldName, T defaultValue = default(T))
        {
            FieldName = fieldName;
            Value     = defaultValue;
        }

        public abstract T ApplyTo(ref GameSettingsData config);
        public abstract void LoadFrom(GameSettingsData config);
        public abstract bool EqualsData(GameSettingsData config);
    }
}
