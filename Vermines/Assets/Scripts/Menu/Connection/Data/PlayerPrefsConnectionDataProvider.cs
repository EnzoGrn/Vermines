using UnityEngine;

namespace Vermines.Menu.Connection.Data {

    using OMGG.Menu.Connection.Data;

    /// <summary>
    /// Default implementation of <see cref="IConnectionDataProvider"/> using Unity's <see cref="PlayerPrefs"/> for storage.
    /// </summary>
    public class PlayerPrefsConnectionDataProvider : IConnectionDataProvider {

        public PlayerPrefsConnectionDataProvider() { }

        /// <summary>
        /// Gets the stored username from PlayerPrefs.
        /// </summary>
        /// <returns>The stored username, or an empty string if not set.</returns>
        public string GetUsername() => PlayerPrefs.GetString("OMGG.Profile.Username", string.Empty);

        /// <summary>
        /// Saves the username to PlayerPrefs.
        /// </summary>
        /// <param name="username">The username to store.</param>
        public void SetUsername(string username) => PlayerPrefs.SetString("OMGG.Profile.Username", username);

        /// <summary>
        /// Gets the preferred region from PlayerPrefs.
        /// </summary>
        /// <returns>The preferred region, or an empty string if not set.</returns>
        public string GetPreferredRegion() => PlayerPrefs.GetString("Vermines.Menu.Region", string.Empty);

        /// <summary>
        /// Saves the preferred region to PlayerPrefs.
        /// </summary>
        /// <param name="region">The region to store.</param>
        public void SetPreferredRegion(string region) => PlayerPrefs.SetString("Vermines.Menu.Region", region);

        /// <summary>
        /// Gets the stored maximum player count from PlayerPrefs.
        /// </summary>
        /// <returns>The max player count, or 4 if not set.</returns>
        public int GetMaxPlayerCount() => PlayerPrefs.GetInt("Vermines.MaxPlayerCount", 4);

        /// <summary>
        /// Saves the maximum player count to PlayerPrefs.
        /// </summary>
        /// <param name="count">The max player count to store.</param>
        public void SetMaxPlayerCount(int count) => PlayerPrefs.SetInt("Vermines.MaxPlayerCount", count);
    }
}
