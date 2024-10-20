using UnityEngine;
using System;

#region PUN2 Imports
using Photon.Pun;
using Photon.Realtime;
#endregion

/*
 * @brief This class is used to manage the lobby, it inherits from MonoBehaviourPunCallbacks.
 */
public class NetworkLobbyManager : MonoBehaviourPunCallbacks
{
    #region Public Attributes
    /*
     * @brief This event is triggered when the client has joined the lobby.
     */
    public event Action OnJoinedLobbyAction;
    /*
     * @brief This event is triggered when the client has disconnected from the server.
     */
    public event Action OnDisconnectedAction;

    /*
     * @brief This event is triggered when the client failed to join a room.
     */
    public event Action OnJoinRoomFailedAction;
    #endregion

    #region Private Attributes
    private NetworkSettings _networkSettings;
    #endregion


    #region Methods Implementation
    /*
     * @brief This method is used to connect client to the server.
     * 
     * @param void
     * 
     * @return void
     */
    private void Awake()
    {
        _networkSettings = Resources.Load<NetworkSettings>("Network/Settings/NetworkSettings");

        if (_networkSettings == null)
        {
            Debug.LogError("NetworkSettings not found in Resources folder !");
            return;
        }
    }

    public void ConnectClientToServer()
    {
        // Remove in the future: actually here for purpose test
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "jp";

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Client already connected to master server");
            OnConnectedToMaster();
            return;
        }

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = PhotonNetwork.GameVersion;
    }
    /*
     * @brief This method is used to disconnect client from the server by leaving current room, lobby and then disconnect from master server.
     * 
     * @param void
     * 
     * @return void
     */
    public void DisconnectClientFromServer()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Client leaving room");
            PhotonNetwork.LeaveRoom();
        }

        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Client leaving lobby");
            PhotonNetwork.LeaveLobby();
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Client disconnecting from the master server");
            PhotonNetwork.Disconnect();
        }
    }
    #endregion

    /*
     * @brief This method is used to connect client to the lobby.
     * 
     * @param void
     * 
     * @return void
     */
    private void ConnectToLobby()
    {
        PhotonNetwork.AutomaticallySyncScene = _networkSettings.automaticallySyncScene;

        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Client already in lobby");
            OnJoinedLobby();
            return;
        }

        PhotonNetwork.JoinLobby();
    }

    #region PunCallbacks

    public override void OnConnectedToMaster()
    {
        ConnectToLobby();
    }

    public override void OnJoinedLobby()
    {
        OnJoinedLobbyAction?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause.ToString());
        OnDisconnectedAction?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join room failed : " + message);

        // When fail to join a room Pun2 seems disconnect the client from the lobby.
        if (!PhotonNetwork.InLobby)
        {
            Debug.Log("Client not in a lobby");
            ConnectToLobby();
        }
    }

    #endregion
}
