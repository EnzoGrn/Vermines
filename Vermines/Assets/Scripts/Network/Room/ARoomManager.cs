using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ARoomManager : MonoBehaviour
{
    #region Protected Attributes
    /*
     * @brief This attribute is used to store the NetworkRoomManager.
     */
    protected NetworkRoomManager _networkRoomManager;
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
        if (!TryGetComponent<NetworkRoomManager>(out _networkRoomManager))
        {
            Debug.LogError("NetworkRoomManagerPrefab is not set");
            return;
        }

        _networkRoomManager.OnCreatePrivateRoomAction += OnCreatePrivateRoom;
        _networkRoomManager.OnCreatePrivateRoomFailedAction += OnCreatePrivateRoomFailed;
        _networkRoomManager.OnJoinedPrivateRoomAction += OnJoinedPrivateRoom;
        _networkRoomManager.OnJoinPrivateRoomFailedAction += OnJoinPrivateRoomFailed;
    }

    #region INetworkManagerCallbacks
    /*
     * @brief This method is an event handler for when the client created a private room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnCreatePrivateRoom()
    {
        Debug.Log("Client created room");
    }


    /*
     * @brief This method is an event handler for when the client failed to create a private room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnCreatePrivateRoomFailed()
    {
        Debug.Log("Client create room failed");
    }

    /*
     * @brief This method is an event handler for when the client left a room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnLeftRoom()
    {
        Debug.Log("Client left room");
    }

    /*
     * @brief This method is an event handler for when the client joined a room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnJoinedPrivateRoom()
    {
        Debug.Log("Client joined room");
    }


    /*
     * @brief This method is an event handler for when the client failed to join a room.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnJoinPrivateRoomFailed()
    {
        Debug.Log("Client join room failed");
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
        _networkRoomManager.OnCreatePrivateRoomAction -= OnCreatePrivateRoom;
        _networkRoomManager.OnCreatePrivateRoomFailedAction -= OnCreatePrivateRoomFailed;
        _networkRoomManager.OnJoinedPrivateRoomAction -= OnJoinedPrivateRoom;
        _networkRoomManager.OnJoinPrivateRoomFailedAction -= OnJoinPrivateRoomFailed;
    }

    #region Events
    public void OnClickCreateRoomButton()
    {
        _networkRoomManager.CreatePrivateRoom();
    }
    #endregion
}
