using UnityEngine;
using Fusion;
using TMPro;

namespace Vermines.Configuration.Network {

    public abstract class NetworkSettingBehaviour<T> : NetworkBehaviour where T: unmanaged {

        #region Variables

        [SerializeField]
        protected string _LabelText;

        [SerializeField]
        protected string _FieldName;

        public TMP_Text Label;

        public TMP_InputField InputField;

        protected ISetting<T> _Setting;

        protected SettingsManager _Manager;

        #endregion

        #region Methods

        public void Initialize(ISetting<T> setting, SettingsManager manager)
        {
            _Setting = setting;
            _Manager = manager;

            if (Label)
                Label.text = _LabelText;
            if (InputField)
                InputField.text = setting.Value.ToString();
            InputField.interactable = HasStateAuthority;
        }

        protected abstract bool TryParse(string input, out T parsedValue);

        #endregion

        #region Overrides Methods

        public override void Spawned()
        {
            if (InputField && HasStateAuthority)
                InputField.onEndEdit.AddListener(OnValueChanged);
        }

        public override void FixedUpdateNetwork()
        {
            if (InputField != null && _Setting != null && !HasStateAuthority) {
                var newText = _Setting.Value.ToString();

                if (InputField.text != newText)
                    InputField.text = newText;
            }
        }

        #endregion

        #region Events

        protected virtual void OnValueChanged(string input)
        {
            if (!HasStateAuthority)
                return;
            if (TryParse(input, out T parsedValue)) {
                _Setting.Value = parsedValue;

                if (typeof(T) == typeof(int)) {
                    var config = _Manager.NetworkConfig;

                    ((ISetting<int>)_Setting).ApplyTo(ref config);

                    _Manager.SetConfiguration(config);
                }
            } else {
                Debug.LogWarning($"[NetworkSettingBehaviour] Failed to parse input '{input}' for setting '{_Setting.FieldName}'. Expected type: {typeof(T)}");
            }
        }

        #endregion
    }
}
