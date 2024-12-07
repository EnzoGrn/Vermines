using UnityEngine;

public class Startup {

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InstantiatePrefabs()
    {
        Debug.Log("-- Instantiating objects --");

        // Instantiate all prefabs in the InstantiatedOnLoad folder
        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("Prefabs/InstantiatedOnLoad/");

        foreach (GameObject prefab in prefabsToInstantiate) {
            Debug.Log($"Creating {prefab.name}");

            GameObject.Instantiate(prefab);
        }

        Debug.Log("-- Instantiating objects done --");
    }
}
