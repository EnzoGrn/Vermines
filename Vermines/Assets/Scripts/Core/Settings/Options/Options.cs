using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Core.Settings {

    using Vermines.Core.Settings;
    using Vermines.Utils;

    public sealed class Options {

        #region Attributes

        public event Action ChangesSaved;
        public event Action ChangesDiscarded;

        private Dictionary<string, OptionsValue> _Values = new(128);
        private Dictionary<string, OptionsValue> _DirtyValues = new(128);

        private OptionsData _OptionsData;

        private bool _EnablePersistency;

        private string _PersistencyPrefix;

        #endregion

        #region Getters & Setters

        public bool HasUnsavedChanges
        {
            get => _DirtyValues.Count > 0;
        }

        public string PersistencyPrefix
        {
            get => _PersistencyPrefix;
        }

        public OptionsValue GetValue(string key)
        {
            if (_DirtyValues.TryGetValue(key, out OptionsValue dirtyValue))
                return dirtyValue;
            if (_Values.TryGetValue(key, out OptionsValue value))
                return value;
            Debug.LogError($"Missing options value with key {key}");

            return default;
        }

        public bool GetBool(string key)
        {
            return GetValue(key).BoolValue;
        }

        public float GetFloat(string key)
        {
            return GetValue(key).FloatValue.Value;
        }

        public int GetInt(string key)
        {
            return GetValue(key).IntValue.Value;
        }

        public string GetString(string key)
        {
            return GetValue(key).StringValue;
        }

        public void Set<T>(string key, T value, bool saveImmediately)
        {
            if (string.IsNullOrEmpty(key)) {
                Debug.LogError("PlayerOptions.Set - Missing key");

                return;
            }
            OptionsValue originalValue = default;

            _Values.TryGetValue(key, out originalValue);

            OptionsValue newValue = originalValue;

            if (value is bool boolValue) {
                newValue.Type = OptionsValueType.Bool;
                newValue.BoolValue = boolValue;
            } else if (value is float floatValue) {
                newValue.Type = OptionsValueType.Float;
                newValue.FloatValue.Value = floatValue;
            } else if (value is int intValue) {
                newValue.Type = OptionsValueType.Int;
                newValue.IntValue.Value = intValue;
            } else if (value is string stringValue) {
                newValue.Type = OptionsValueType.String;
                newValue.StringValue = stringValue;
            } else if (value is OptionsValue optionsValueNew) {
                newValue = optionsValueNew;
            } else {
                throw new NotSupportedException(string.Format("Unsupported type, Type: {0} Key: {1}", typeof(T), key));
            }

            if (newValue.Equals(originalValue)) {
                _DirtyValues.Remove(key); // Remove previous modification if exists

                return;
            }

            if (originalValue.Type == OptionsValueType.None || originalValue.Type == newValue.Type)
                _DirtyValues[newValue.Key] = newValue;
            else
                Debug.LogError($"Trying to write incorrect type of value, Type: {typeof(T)} Key: {key}");
            if (saveImmediately == true)
                SaveChanges();
        }

        public bool IsDirty(string key)
        {
            return _DirtyValues.ContainsKey(key);
        }

        #endregion

        #region Methods

        public void Initialize(OptionsData optionsData, bool enablePersistency, string persistencyPrefix)
        {
            _OptionsData       = optionsData;
            _EnablePersistency = enablePersistency;
            _PersistencyPrefix = persistencyPrefix;

            optionsData.Initialize();

            LoadValues();
        }

        public void SaveChanges()
        {
            if (_DirtyValues.Count == 0)
                return;
            foreach (var pair in _DirtyValues) {
                var value = pair.Value;

                AddValue(value);

                if (!_EnablePersistency)
                    continue;
                if (!_OptionsData.TryGet(value.Key, out OptionsValue defaultValue))
                    continue; // Only values that are stored in options data are stored as persistent
                string key = _PersistencyPrefix + value.Key;

                if (value.Equals(defaultValue)) {
                    // Value is default, remove it from persistent storage
                    PersistentStorage.Delete(key);

                    continue;
                }

                switch (value.Type) {
                    case OptionsValueType.Bool:
                        PersistentStorage.SetBool(key, value.BoolValue, false);

                        break;
                    case OptionsValueType.Float:
                        PersistentStorage.SetFloat(key, value.FloatValue.Value, false);

                        break;
                    case OptionsValueType.Int:
                        PersistentStorage.SetInt(key, value.IntValue.Value, false);

                        break;
                    case OptionsValueType.String:
                        PersistentStorage.SetString(key, value.StringValue, false);

                        break;
                }
            }

            if (_EnablePersistency)
                PersistentStorage.Save();
            _DirtyValues.Clear();

            ChangesSaved?.Invoke();
        }

        public void DiscardChanges()
        {
            _DirtyValues.Clear();

            ChangesDiscarded?.Invoke();
        }

        public void ResetValueToDefault(string key, bool saveImmediately)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (_OptionsData.TryGet(key, out OptionsValue value))
                Set(value.Key, value, saveImmediately);
        }

        public void ResetAllValuesToDefault()
        {
            List<OptionsValue> values = _OptionsData.Values;

            foreach (OptionsValue value in values)
                Set(value.Key, value, false);
            SaveChanges();
        }

        public void AddDefaultValue(OptionsValue value)
        {
            if (_Values.ContainsKey(value.Key))
                return;
            _OptionsData.AddRuntimeValue(value);

            LoadValue(value);
        }

        private void AddValue(OptionsValue value)
        {
            _Values[value.Key] = value;
        }

        private void LoadValues()
        {
            _Values.Clear();
            _DirtyValues.Clear();

            List<OptionsValue> values = _OptionsData.Values;

            foreach (OptionsValue value in values)
                LoadValue(value);
        }

        private void LoadValue(OptionsValue value)
        {
            if (value.Type == OptionsValueType.None || string.IsNullOrEmpty(value.Key)) {
                Debug.LogError($"Incorrect options value, Type: {value.Type} Key: {value.Key}");

                return;
            }

            if (_EnablePersistency) {
                string key = _PersistencyPrefix + value.Key;

                switch (value.Type) {
                    case OptionsValueType.Bool:
                        value.BoolValue = PersistentStorage.GetBool(key, value.BoolValue);

                        break;
                    case OptionsValueType.Float:
                        value.FloatValue.Value = PersistentStorage.GetFloat(key, value.FloatValue.Value);

                        break;
                    case OptionsValueType.Int:
                        value.IntValue.Value = PersistentStorage.GetInt(key, value.IntValue.Value);

                        break;
                    case OptionsValueType.String:
                        value.StringValue = PersistentStorage.GetString(key, value.StringValue);

                        break;
                }
            }

            AddValue(value);
        }

        #endregion
    }
}
