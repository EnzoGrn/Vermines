namespace OMGG.DesignPattern {

    /*
     * @brief Singleton design pattern implementation as a template class.
     * Only for non-Monobehaviour classes.
     */
    public class Singleton<T> where T : class, new() {

        /// <summary>
        /// Lock object to make the singleton thread-safe.
        /// </summary>
        private static readonly object _Lock = new();

        private static T _Instance;

        public static T Instance
        {
            get
            {
                // Double-check locking for thread safety.
                // Because we want to avoid the overhead of locking the object every time.
                if (_Instance == null) {
                    lock (_Lock) {

                        // If instance is null, we create a new instance.
                        if (_Instance == null)
                            _Instance = new T();
                    }
                }

                return _Instance;
            }

            private set => _Instance = value;
        }

        /// <summary>
        /// Call this function to reset the instance of the singleton.
        /// </summary>
        public void Reset()
        {
            _Instance = null;
        }
    }
}
