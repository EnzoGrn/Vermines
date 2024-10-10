using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    #region Events
    /*
     * @brief This method is used to handle the event when the client clicks on the play online button, it will load the lobby scene.
     * 
     * @param void
     * 
     * @return void
     */
    public void OnPlayOnlineButtonPressed()
    {
        Sceneloader.LoadScene(Sceneloader.Scene.Lobby);
    }
    #endregion
}
