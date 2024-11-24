namespace OMGG.Network {

    /*
     * @brief ITimeSync is an interface that represents network time synchronization.
     * It provides a common network clock.
     * It also manages latency differences.
     */
    public interface ITimeSync {

        /*
         * @brief Get the time since the last sync with the server
         */
        float NetworkTime { get; }

        /*
         * @brief Sync the time with the server
         */
        void SyncTime();
    }
}
