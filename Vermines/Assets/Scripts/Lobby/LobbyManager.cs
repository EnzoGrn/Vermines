using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Events
    public void OnMenuButtonPressed()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
    #endregion
}
