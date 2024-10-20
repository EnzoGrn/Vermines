using TMPro;
using UnityEngine;

public class RoomManager : ARoomManager
{
    #region [SerializeField] Private Fields
    /*
     * @brief This attribute is used to store the private room code to join.
     */
    [SerializeField] private TMP_InputField _privateRoomCodeToJoin;
    
    /*
     * @brief This attribute is used to store the message text to inform player about what is happening.
     */
    [SerializeField] private TMP_Text _messageText;

    /*
     * @brief This attribute is used to store the scene to load.
     */
    [SerializeField] private string _sceneToLoad;
    #endregion

    #region Private Attributes
    /*
     * @brief This attribute is used to get the network settings.
     */
    private NetworkSettings _networkSettings;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        _networkSettings = Resources.Load<NetworkSettings>("Network/Settings/NetworkSettings");

        if (_networkSettings == null)
        {
            Debug.LogError("NetworkSettings not found in Resources folder !");
            return;
        }
    }

    /*
     * @brief This method is used to setup the code room input field before Update method is called the first time.
     * 
     * @param void
     * 
     * @return void
     */
    void Start()
    {
        _privateRoomCodeToJoin.characterLimit = _networkSettings.maxRoomCodeLength;
        _privateRoomCodeToJoin.contentType = TMP_InputField.ContentType.Password;
    }

    #region NetworkManagerCallbacks
    public override void OnCreatePrivateRoom()
    {
        _messageText.text = "Room created !";
        _messageText.color = Color.green;
        _networkRoomManager.LoadLevel(_sceneToLoad);
    }

    public override void OnCreatePrivateRoomFailed()
    {
        _messageText.text = "Room creation failed !";
        _messageText.color = Color.red;
    }

    public override void OnJoinedPrivateRoom()
    {
        _messageText.text = "Room joinned !";
        _messageText.color = Color.green;
    }

    public override void OnJoinPrivateRoomFailed()
    {
        _messageText.text = "Join private room failed !";
        _messageText.color = Color.red;
    }
    #endregion

    #region Events
    /*
     * @brief This method is used to handle the event when the client clicks on the create room button.
     * 
     * @param void
     * 
     * @return void
     */
    public void OnClickJoinRoomButton()
    {
        _networkRoomManager.JoinPrivateRoom(_privateRoomCodeToJoin.text);
    }
    #endregion
}
