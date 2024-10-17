using UnityEngine;
using System;

#region PUN2 Imports
using Photon.Pun;
using Photon.Realtime;
#endregion

public class NetworkRoomManager : MonoBehaviourPunCallbacks
{
    #region Private Attributes
    /*
     * @brief This attribute is used to get the network settings.
     */
    private NetworkSettings _networkSettings;
    #endregion

    #region Public Attributes
    /*
     * @brief This event is triggered when the client has created a private room.
     */
    public event Action OnCreatePrivateRoomAction;

    /*
     * @brief This event is triggered when the client failed to create a private room.
     */
    public event Action OnCreatePrivateRoomFailedAction;

    /*
     * @brief This event is triggered when the client has joined a private room.
     */
    public event Action OnJoinedPrivateRoomAction;

    /*
     * @brief This event is triggered when the client failed to join a private room.
     */
    public event Action OnJoinPrivateRoomFailedAction;
    #endregion

    #region Methods Implementation
    private void Awake()
    {
        _networkSettings = Resources.Load<NetworkSettings>("Network/Settings/NetworkSettings");

        if (_networkSettings == null)
        {
            Debug.LogError("NetworkSettings not found in Resources folder !");
            return;
        }

        PhotonNetwork.NickName = "Player " + NetworkUtils.GenerateRandomCode(_networkSettings.keyStringCodeGeneration,
            _networkSettings.maxPlayerNicknameLength, _networkSettings.random);
    }

    /*
     * @brief This method is used to create a private room.
     * 
     * @param string roomCode (optional) default value is "".
     * @param int? maxPLayers (optional) default value is null.
     * @param bool? isVisible (optional) default value is null.
     * 
     * @return void
     */
    public void CreatePrivateRoom(string roomCode = "", int? maxPLayers = null, bool? isVisible = null)
    {
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby || PhotonNetwork.InRoom)
        {
            OnCreateRoomFailed(0, "Client not connected or already in a room !");
            return;
        }

        if (!_networkSettings.allowCustomRoomCodes && !string.IsNullOrEmpty(roomCode))
        {
            OnCreateRoomFailed(0, "Custom room codes are not allowed !");
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            roomCode = NetworkUtils.GenerateRandomCode(_networkSettings.keyStringCodeGeneration,
                _networkSettings.maxRoomCodeLength, _networkSettings.random);
        }

        if (maxPLayers != null && (maxPLayers < _networkSettings.minPlayers || maxPLayers > _networkSettings.maxPlayers))
        {
            OnCreateRoomFailed(0, "Invalid number of players !");
            return;
        }

        maxPLayers ??= _networkSettings.maxPlayers;
        isVisible ??= _networkSettings.defaultVisibleSetting;

        PhotonNetwork.CreateRoom(roomCode, new RoomOptions
        {
            MaxPlayers = (int)maxPLayers,
            IsVisible = (bool)isVisible,
        });
    }

    /*
     * @brief This method is used to join a private room.
     * 
     * @param string roomCode
     * 
     * @return void
     */
    public void JoinPrivateRoom(string roomCode)
    {
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby || PhotonNetwork.InRoom)
        {
            OnJoinRoomFailed(0, "Client not connected or already in a room !");
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            OnJoinRoomFailed(0, "Room code is empty !");
            return;
        }

        if (roomCode.Length < _networkSettings.minRoomCodeLength || roomCode.Length > _networkSettings.maxRoomCodeLength)
        {
            OnJoinRoomFailed(0, "Invalid room code length !");
            return;
        }

        PhotonNetwork.JoinRoom(roomCode);
    }

    /*
     * @brief This method is used to load a level.
     * 
     * @param string levelName
     * 
     * @return void
     */
    public void LoadLevel(string levelName)
    {
        PhotonNetwork.LoadLevel(levelName);
    }
    #endregion

    #region PunCallbacks
    public override void OnCreatedRoom()
    {
        OnCreatePrivateRoomAction?.Invoke();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OnCreatePrivateRoomFailedAction?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        OnJoinedPrivateRoomAction?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        OnJoinPrivateRoomFailedAction?.Invoke();
    }
    #endregion
}
