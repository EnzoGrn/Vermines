using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sceneloader : MonoBehaviour
{
    // Singleton
    public static Sceneloader sceneLoaderInstance;

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

    private void Awake()
    {
        if (sceneLoaderInstance == null)
        {
            sceneLoaderInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /*
     * @brief This method is used to load the scene asynchronously.
     * 
     * @param Scene scene
     * 
     * @return void
     */
    public void LoadScene(Scene scene)
    {
        // Prevent the Scene Loader from being destroyed during the scene transition
        StartCoroutine(LoadSceneAsync(scene));
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
    }
}
