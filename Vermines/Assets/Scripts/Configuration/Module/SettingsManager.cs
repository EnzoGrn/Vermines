using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Configuration.Network {

    public class SettingsManager : NetworkBehaviour {

        #region Variables

        [Networked, OnChangedRender(nameof(OnConfigChanged))]
        public GameSettingsData NetworkConfig { get; private set; }

        public void SetConfiguration(GameSettingsData config)
        {
            if (HasStateAuthority)
                NetworkConfig = config;
        }

        [SerializeField]
        private GameConfiguration _DefaultConfiguration;

        private readonly List<ISetting<int>> _IntSettings = new();

        #endregion

        #region Methods

        private void InitializeAllSettings()
        {
            foreach (var setting in GetComponentsInChildren<NetworkedIntSettingBehaviour>()) {
                var intSetting = setting.CreateSetting();

                _IntSettings.Add(intSetting);

                intSetting.LoadFrom(NetworkConfig);
                setting.Initialize(intSetting, this);
            }
        }

        private void UpdateSettingsFromNetworkConfig()
        {
            foreach (var setting in _IntSettings)
                setting.LoadFrom(NetworkConfig);
        }

        public void ApplyChanges()
        {
            if (!HasStateAuthority)
                return;
            foreach (var setting in _IntSettings) {
                // Copy to avoid CS0206 error where ref need to have a ref-returning value for store the changing data.
                var configCopy = NetworkConfig;

                setting.ApplyTo(ref configCopy);

                NetworkConfig = configCopy;
            }
        }

        #endregion

        #region Overrides Methods

        public override void Spawned()
        {
            if (HasStateAuthority)
                NetworkConfig = _DefaultConfiguration.ToGameSettingsData();
            InitializeAllSettings();
        }

        #endregion

        #region Events

        /// <summary>
        /// Automatically called when the NetworkConfig is changed.
        /// <see cref="NetworkConfig" />
        /// </summary>
        private void OnConfigChanged()
        {
            UpdateSettingsFromNetworkConfig();
        }

        #endregion
    }
}
