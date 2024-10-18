using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectLoader {

    /*
     * @brief Load a criptable object from a path.
     * When it's load, he instantiate it for not modify the original object.
     */
    public T LoadScriptableObject<T>(string path) where T : ScriptableObject
    {
        T loadedSO = Resources.Load<T>(path);

        if (loadedSO != null)
            return Object.Instantiate(loadedSO);
        return null;
    }

    /*
     * @brief Load all scriptable object from a path.
     * When it's load, he instantiate individualy each object for not modify the original object.
     */
    public T[] LoadAllScriptableObject<T>(string path) where T : ScriptableObject
    {
        T[] sObjects = Resources.LoadAll<T>(path);

        if (sObjects.Length == 0)
            return null;
        for (int i = 0; i < sObjects.Length; i++)
            sObjects[i] = Object.Instantiate(sObjects[i]);
        return sObjects;
    }
}
