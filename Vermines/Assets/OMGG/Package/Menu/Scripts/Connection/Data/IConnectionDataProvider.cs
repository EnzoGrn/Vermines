namespace OMGG.Menu.Connection.Data {

    /// <summary>
    /// Interface for providing connection-related data such as username, region, app version, and max player count.
    /// Allows abstraction over data storage mechanisms (e.g., PlayerPrefs, Steam, PlayFab).
    /// </summary>
    public interface IConnectionDataProvider {

        /// <summary>
        /// Gets the current username.
        /// </summary>
        /// <returns>The username as a string.</returns>
        string GetUsername();

        /// <summary>
        /// Sets the current username.
        /// </summary>
        /// <param name="username">The username to set.</param>
        void SetUsername(string username);

        /// <summary>
        /// Gets the user's preferred region.
        /// </summary>
        /// <returns>The preferred region as a string.</returns>
        string GetPreferredRegion();

        /// <summary>
        /// Sets the user's preferred region.
        /// </summary>
        /// <param name="region">The preferred region to set.</param>
        void SetPreferredRegion(string region);

        /// <summary>
        /// Gets the maximum player count for the session.
        /// </summary>
        /// <returns>The maximum number of players.</returns>
        int GetMaxPlayerCount();

        /// <summary>
        /// Sets the maximum player count for the session.
        /// </summary>
        /// <param name="count">The number of players to set.</param>
        void SetMaxPlayerCount(int count);
    }
}
