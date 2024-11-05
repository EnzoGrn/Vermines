using UnityEngine;

/*
 * @brief Singleton design pattern implementation as a template class.
 */
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

    private static T _Instance;

    public static T Instance
    {
        get
        {
            // If the instance is null, we try to find it in the scene.
            if (_Instance == null) {
                _Instance = FindObjectOfType<T>();

                // If the instance is still null, we create a new GameObject with the name of the class and add the component to it.
                if (_Instance == null) {
                    GameObject go = new(typeof(T).Name);

                    _Instance = go.AddComponent<T>();
                }
            }

            return _Instance;
        }
    }
}
