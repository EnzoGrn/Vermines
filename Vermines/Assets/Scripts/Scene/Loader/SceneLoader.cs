using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Sceneloader
{
    /*
     * @brief This class is used to access coroutine.
     */
    private class LoadingMonoBehaviour : MonoBehaviour {}

    /*
     * @brief This enum is used to store the scene name.
     */
    public enum Scene
    {
        Menu,
        Lobby,
        // WaitingRoom - > Is loaded by the RoomManager using Pun2
        // Game - > Is loaded by the WaitingRoomManager using Pun2
    }

    /*
     * @brief This method is used to load the scene asynchronously.
     * 
     * @param Scene scene
     * 
     * @return void
     */
    public static void LoadScene(Scene scene)
    {
        GameObject loadingMonoBehaviour =  new GameObject("Scene Loader");

        loadingMonoBehaviour.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));

    }

    /*
     * @brief This async method is used to load the scene asynchronously, it run each frame to check if the scene is loaded.
     *
     * @param Scene sceneName
     * 
     * @return IEnumerator
     */
    private static IEnumerator LoadSceneAsync(Scene sceneName)
    {

        Debug.Log("Loading scene : " + sceneName.ToString());

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName.ToString());
        while (asyncOperation != null && !asyncOperation.isDone)
        {
            yield return null;
        }

        GameObject sceneLoader = GameObject.Find("Scene Loader");

        if (sceneLoader != null)
        {
            Object.Destroy(sceneLoader);
        }

    }
}
