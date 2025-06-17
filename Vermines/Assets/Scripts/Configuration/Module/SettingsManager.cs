using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace Vermines.Configuration.Network {

    public class SettingsManager : NetworkBehaviour {

        #region Variables

        [Networked]
        public ref GameSettingsData NetworkConfig => ref MakeRef<GameSettingsData>();

        public void SetConfiguration(GameSettingsData config)
        {
            if (!HasStateAuthority)
                return;
            NetworkConfig = config;
        }

        [SerializeField]
        private GameConfiguration _DefaultConfiguration;

        private readonly List<ISetting<int>> _IntSettings = new();

        #endregion

        #region Methods

        private void InitializeAllSettings()
        {
            foreach (var setting in GetComponentsInChildren<NetworkedIntSettingBehaviour>(true)) {
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

        #endregion

        #region Overrides Methods

        public override void Spawned()
        {
            if (HasStateAuthority)
                NetworkConfig = _DefaultConfiguration.ToGameSettingsData();
            InitializeAllSettings();
            UpdateSettingsFromNetworkConfig();
        }

        #endregion

        #region Events

        /// <summary>
        /// Automatically called when the NetworkConfig is changed.
        /// <see cref="NetworkConfig" />
        /// </summary>
        private void OnConfigChanged()
        {
                Debug.Log("[SettingsManager] OnConfigChanged triggered by network update");

            UpdateSettingsFromNetworkConfig();
        }

        #endregion
    }
}
