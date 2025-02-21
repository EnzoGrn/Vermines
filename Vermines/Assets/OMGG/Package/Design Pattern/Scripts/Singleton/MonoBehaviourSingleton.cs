using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace OMGG.DesignPattern {

    /*
     * @brief Singleton design pattern implementation as a template class.
     * Only for Monobehaviour classes.
     */
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {

        /*
         * @brief Lock object to make the singleton thread-safe.
         */
        private static readonly object _Lock = new();

        private static T _Instance;

        public static T Instance
        {
            get
            {
                // If the instance is null, we try to find it in the scene.
                if (_Instance == null) {
                    lock (_Lock) {
                        _Instance = (T)FindAnyObjectByType(typeof(T));

                        // If the instance is still null, we create a new GameObject with the name: "Auto-generated {className}", and we add the component to it.
                        if (_Instance == null) {
                            GameObject go = new("Auto-generated " + typeof(T).Name);

                            _Instance = go.AddComponent<T>();
                        }
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

        public void Awake()
        {
            if (_Instance != null && _Instance != this) {

                // Destroy during the runtime.
                if (Application.isPlaying)
                    Destroy(gameObject);
                return;
            }

            lock (_Lock) {

                // Here we cast our instance into class type T.
                // Why we do not 'GetComponent<T>()'?
                // Because a casting is more optimized, and we already know that the instance is of type T.
                _Instance = this as T;

                DontDestroyOnLoad(gameObject);
            }
        }

        /*
         * @brief Function for destroy the instance when the object is destroyed.
         */
        public void OnDestroy()
        {
            if (_Instance == this)
                _Instance = null;
        }
    }
}
