namespace OMGG.DesignPattern {

    /*
     * @brief Singleton design pattern implementation as a template class.
     * Only for non-Monobehaviour classes.
     */
    public class Singleton<T> where T : class, new() {

        /*
         * @brief Lock object to make the singleton thread-safe.
         */
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

            /*
             * We don't want to set the instance from outside.
             * 
             * @brief Protect the instance setter.
             */
            private set => _Instance = value;
        }
    }
}
