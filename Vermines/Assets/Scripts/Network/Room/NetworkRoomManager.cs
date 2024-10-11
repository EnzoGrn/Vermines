using UnityEngine;
using System;
using System.Linq;

#region PUN2 Imports
using Photon.Pun;
using Photon.Realtime;
#endregion

public class NetworkRoomManager : MonoBehaviourPunCallbacks
{
    #region Private Attributes
    /*
     * @brief This string is used to generate a random code for the room.
     */
    private const string keyStringCodeGeneration = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    /*
     * @brief This attribute is used to generate a random number.
     */
    private System.Random _random = new();
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
    public void Start()
    {
        PhotonNetwork.NickName = "Player " + GenerateRandomCode(4);
    }

    /*
     * @brief This method is used to create a private room.
     * 
     * @param void
     * 
     * @return void
     */
    public void CreatePrivateRoom(string roomCode = "", bool isVisible = false, int maxPLayers = 2)
    {
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby || PhotonNetwork.InRoom)
        {
            OnCreateRoomFailed(0, "Client not connected or already in a room !");
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            roomCode = GenerateRandomCode(6);
        }

        PhotonNetwork.CreateRoom(roomCode, new RoomOptions
        {
            MaxPlayers = maxPLayers,
            IsVisible = isVisible,
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
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby || PhotonNetwork.InRoom || string.IsNullOrEmpty(roomCode))
        {
            OnJoinRoomFailed(0, "Client not connected or already in a room !");
            return;
        }

        PhotonNetwork.JoinRoom(roomCode);
    }

    /*
     * @brief This method is used to generate a random code.
     * 
     * @param int length
     * 
     * @return string
     */
    public string GenerateRandomCode(int length)
    {
        return new string(Enumerable.Repeat(keyStringCodeGeneration, length).Select(s =>
            s[_random.Next(0, keyStringCodeGeneration.Length)]).ToArray());
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
