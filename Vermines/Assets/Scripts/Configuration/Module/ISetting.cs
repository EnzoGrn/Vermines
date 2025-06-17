namespace Vermines.Configuration.Network {

    public interface ISetting<T> {

        /// <summary>
        /// String value that represents the setting.
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// The value of the setting.
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// Apply the modification to the given configuration.
        /// </summary>
        /// <param name="config">The current network configuration</param>
        /// <returns>The value that was applied to the configuration.</returns>
        T ApplyTo(ref GameSettingsData config);

        /// <summary>
        /// Load the setting from the given configuration.
        /// </summary>
        /// <param name="config">The current network configuration</param>
        void LoadFrom(GameSettingsData config);

        /// <summary>
        /// Check if the current setting is equal to the given data.
        /// </summary>
        /// <param name="data">The current network configuration</param>
        /// <returns>True if the current setting is equal to the given data, false otherwise.</returns>
        bool EqualsData(GameSettingsData config);
    }
}
