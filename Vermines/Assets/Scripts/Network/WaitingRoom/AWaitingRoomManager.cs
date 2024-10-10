using UnityEngine;

public class AWaitingRoomManager : MonoBehaviour
{
    #region [SerializeField] Private Attributes 
    /*
     * @brief This attribute is used to store the network waiting room manager.
     */
    [SerializeField] protected NetworkWaitingRoomManager _networkWaitingRoomManager;
    #endregion


    /*
     * @brief This method is used to start observing events, when the scirpt istance is being loaded.
     * The base is mandatory to use Observable pattern.
     * 
     * @param void
     * 
     * @return void
     */
    protected virtual void Awake()
    {
        if (!TryGetComponent<NetworkWaitingRoomManager>(out _networkWaitingRoomManager))
        {
            Debug.LogError("NetworkLobbyManager is not set");
            return;
        }

        _networkWaitingRoomManager.OnPlayerLeftRoomAction += OnPlayerLeftRoom;
        _networkWaitingRoomManager.OnLeftRoomAction += OnLeftRoom;
        _networkWaitingRoomManager.OnPlayerEnteredRoomAction += OnPlayerEnteredRoom;
    }

    #region Callbacks  
    /*
     * @brief This method is an event handler for when the player left the room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnPlayerLeftRoom()
    {
        Debug.Log("¨Player left room");
    }

    /*
     * @brief This method is an event handler for when the client left the room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnLeftRoom()
    {
        Debug.Log("Left room");
    }

    /*
     * @brief This method is an event handler for when a player entered the room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnPlayerEnteredRoom()
    {
        Debug.Log("Player entered room");
    }
    #endregion

    /*
     * @brief This method is used to stop observing events, when the monobehaviour will be destroyed.
     * The base is mandatory to use Observable pattern.
     * 
     * @param void
     * 
     * @return void
     */
    protected virtual void OnDestroy()
    {
        _networkWaitingRoomManager.OnPlayerLeftRoomAction -= OnPlayerLeftRoom;
        _networkWaitingRoomManager.OnLeftRoomAction -= OnLeftRoom;
        _networkWaitingRoomManager.OnPlayerEnteredRoomAction -= OnPlayerEnteredRoom;
    }
}
