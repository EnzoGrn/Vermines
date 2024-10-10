using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : ALobbyManager
{
    #region [SerializeField] Private Attributes
    /*
     * @brief This attribute is used to display a message to the client.
     */
    [SerializeField] private TMP_Text _messageText;

    /*
     * @brief This attribute is used to store the name of the menu scene, to potentially return to the main menu of the game.
     */
    [SerializeField] private string _menuSceneName;
    #endregion

    /*
     * @brief This method is used to start the client connection to the server before Update method is called the first time..
     * 
     * @param void
     * 
     * @return void
     */
    void Start()
    {
        _networkLobbyManagerPrefab.ConnectClientToServer();
    }

    #region NetworkManagerCallbacks
    public override void OnClientConnectedToServer()
    {
        _messageText.text = "Client connected to server";
        _messageText.color = Color.green;
    }
    public override void OnClientDisconnectedFromServer()
    {
        _messageText.text = "Client disconnected to server";
        _messageText.color = Color.red;
        SceneManager.LoadScene(_menuSceneName, LoadSceneMode.Single);
    }
    #endregion

    #region Events
    /*
     * @brief This method is used to handle the event when the client clicks on the menu button, it disconnect the player from the server.
     * 
     * @param void
     * 
     * @return void
     */
    public void OnClickMenuButton()
    {
        _networkLobbyManagerPrefab.DisconnectClientFromServer();
    }
    #endregion
}
