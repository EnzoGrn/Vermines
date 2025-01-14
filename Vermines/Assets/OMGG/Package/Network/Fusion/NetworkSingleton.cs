using UnityEngine;
using Fusion;

namespace OMGG.Network.Fusion {

    /// <summary>
    /// Network Singleton.
    /// Network is for Networkbehaviour not a singleton sync in network.
    /// </summary>
    /// <typeparam name="T">NetworkBehaviour (Fusion)</typeparam>
    public static class NetworkSingleton<T> where T : NetworkBehaviour {

        private static readonly object _Lock = new();

        private static T _Instance;
        public static T Instance
        {
            get
            {
                if (_Instance == null) {
                    lock (_Lock) {
                        _Instance = (T)Object.FindAnyObjectByType(typeof(T));

                        if (_Instance == null) {
                            GameObject go = new($"Auto-generated {typeof(T).Name}");

                            if (typeof(NetworkBehaviour).IsAssignableFrom(typeof(T)))
                                go.AddComponent<NetworkObject>();
                            _Instance = go.AddComponent<T>();
                        }
                    }
                }

                return _Instance;
            }
            private set => _Instance = value;
        }
    }
}
