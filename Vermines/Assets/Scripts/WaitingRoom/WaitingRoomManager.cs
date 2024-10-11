using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoomManager : AWaitingRoomManager
{
    #region [SerializeField] Private Attributes
    /*
     * @brief This attribute is used to store and display the code of the actual room.
     */
    [SerializeField] private TMP_Text _roomCodeText;

    /*
     * @brief This attribute is used to store and display the total player in the room.
     */
    [SerializeField] private TMP_Text _totalPlayerText;

    /*
     * @brief This attribute is used to store and display the players name in the room.
     */
    [SerializeField] private TMP_Text _playersNameText;

    /*
     * @brief This attribute is used to store the start match button.
     */
    [SerializeField] private Button _startMatchButton;

    #endregion

    void Start()
    {
        SetUpStartMatchButton();
        DisplayRoomCode();
        DisplayTotalPlayer();
        DisplayPlayersName();

    }

    void Update()
    {
        
    }

    /*
     * @brief This method is used to setUp the state of the start match button, if the client isn't the master one he won't be able to interact with it.
     * 
     * @param void
     * 
     * @return void
     */
    private void SetUpStartMatchButton()
    {
        if (_networkWaitingRoomManager.IsMasterClient())
        {
            _startMatchButton.interactable = true;
            _startMatchButton.enabled = true;
            _startMatchButton.GetComponentInChildren<TMP_Text>().text = "Start Match";

        }
        else
        {
            _startMatchButton.interactable = false;
            _startMatchButton.enabled = false;
            _startMatchButton.GetComponentInChildren<TMP_Text>().text = "Waiting for host...";
        }
    }

    /*
     * @brief This method is used to set the room code to the room code text.
     * 
     * @param void
     * 
     * @return void
     */
    private void DisplayRoomCode()
    {
        _roomCodeText.text = "Room Code : " + _networkWaitingRoomManager.GetRoomCode();
    }


    /*
     * @brief This method is used to set the total player to the total player text.
     * 
     * @param void
     * 
     * @return void
     */
    private void DisplayTotalPlayer()
    {
        _totalPlayerText.text = "Total Players : " + _networkWaitingRoomManager.GetPlayerCount() + "/" + _networkWaitingRoomManager.GetMaxPlayersRoom();
    }

    /*
     * @brief This method is used to set the players name to the players name text.
     * 
     * @param void
     * 
     * @return void
     */
    private void DisplayPlayersName()
    {
        _playersNameText.text = "Players :\n" + _networkWaitingRoomManager.GetPlayersNames();
    }

    #region NetworkManagerCallbacks
    public override void OnLeftRoom()
    {
        Sceneloader.sceneLoaderInstance.LoadScene(Sceneloader.Scene.Lobby);
    }

    public override void OnPlayerLeftRoom()
    {
        Debug.Log("Player left room need to redisplay the code to the new host");
        DisplayRoomCode();
        DisplayPlayersName();
        DisplayTotalPlayer();
        SetUpStartMatchButton();
    }

    public override void OnPlayerEnteredRoom()
    {
        Debug.Log("Player entered room need to redisplay the code to the new host");
        DisplayPlayersName();
        DisplayTotalPlayer();
    }
    #endregion
}
