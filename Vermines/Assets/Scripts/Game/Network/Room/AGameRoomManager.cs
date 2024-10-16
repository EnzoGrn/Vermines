using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGameRoomManager : MonoBehaviour {

    #region Attributes 

    /*
     * @brief This attribute is used to store the network game room manager.
     */
    [SerializeField]
    protected NetworkGameRoomManager _networkManager;

    #endregion

    #region Methods

    /*
     * @brief This method is used to start observing events, when the script instance is being loaded.
     */
    protected virtual void Awake()
    {
        if (!TryGetComponent<NetworkGameRoomManager>(out _networkManager)) {
            Debug.LogError("NetworkGameRoomManager is not set.");
            return;
        }

        _networkManager.OnPlayerLeftRoomAction    += OnPlayerLeftRoom;
        _networkManager.OnPlayerEnteredRoomAction += OnPlayerEnteredRoom;
    }

    /*
     * @brief This method is used to stop observing events, when the monobehaviour will be destroyed.
     */
    protected virtual void OnDestroy()
    {
        _networkManager.OnPlayerLeftRoomAction    -= OnPlayerLeftRoom;
        _networkManager.OnPlayerEnteredRoomAction -= OnPlayerEnteredRoom;
    }

    #endregion

    #region Getters

    public NetworkGameRoomManager GetNetworkManager()
    {
        return _networkManager;
    }

    #endregion

    #region Callbacks  

    /*
     * @brief This method is an event handler called when a player left the room.
     */
    public virtual void OnPlayerLeftRoom()
    {
        Debug.Log("Player has left the room.");
    }

    /*
     * @brief This method is an event handler called when a player entered the room.
     */
    public virtual void OnPlayerEnteredRoom()
    {
        Debug.Log("Player has joined the room.");
    }

    #endregion
}
