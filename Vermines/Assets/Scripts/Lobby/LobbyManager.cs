using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : ALobbyManager
{
    #region [SerializeField] Private Attributes
    /*
     * @brief This attribute is used to display a message to the client.
     */
    [SerializeField] private TMP_Text _messageText;


    /*
     * @brief This attribute is used to store the start match button.
     */
    [SerializeField] private Button _createPrivateRoomButton;

    /*
     * @brief This attribute is used to store the start match button.
     */
    [SerializeField] private Button _joinPrivateRoomButton;
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
        ChangeButtonState(false);
    }

    private void ChangeButtonState(bool state)
    {
        _createPrivateRoomButton.interactable = state;
        _joinPrivateRoomButton.interactable = state;
        _createPrivateRoomButton.enabled = state;
        _joinPrivateRoomButton.enabled = state;
    }

    #region NetworkManagerCallbacks
    public override void OnClientConnectedToServer()
    {
        _messageText.text = "Client connected to server";
        _messageText.color = Color.green;
        ChangeButtonState(true);
    }
    public override void OnClientDisconnectedFromServer()
    {
        _messageText.text = "Client disconnected to server";
        _messageText.color = Color.red;
        Sceneloader.sceneLoaderInstance.LoadScene(Sceneloader.Scene.Menu);
    }
    #endregion
}
