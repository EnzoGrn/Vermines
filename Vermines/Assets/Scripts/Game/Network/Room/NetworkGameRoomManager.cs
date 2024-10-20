using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkGameRoomManager : MonoBehaviourPunCallbacks {

    #region Attributes

    /*
     * @brief This event is triggered when a player left the room.
     */
    public event Action OnPlayerLeftRoomAction;

    /*
     * @brief This event is triggered when a player enter in the room.
     */
    public event Action OnPlayerEnteredRoomAction;

    #endregion

    #region Methods Implementation

    /*
     * @brief This method is used to leave the Game Room.
     * 
     * @warning Normaly, if you are deconnected from the room, you can still try to reconnect to it, until the game is over.
     * But if you want to leave without the possibility to reconnect, a 'Concede' button should be implemented.
     */
    public void LeaveGameRoom()
    {
        // TODO: Implemente concede button that will call this function.

        if (PhotonNetwork.InRoom) {
            Debug.Log($"Client {PhotonNetwork.LocalPlayer.NickName} leaving room.");

            PhotonNetwork.LeaveRoom();
        }
    }

    /*
     * @brief This method is used to initialize the player.
     * 
     * @note If it's note a private function, it's because some player do not past from the event function OnPlayerEnteredRoom,
     * because their are already registered in PhotonNetwork.
     * So, instead of calling the event, we call this function.
     */
    public void InitializePlayer()
    {
        OnPlayerEnteredRoomAction?.Invoke();

        Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} successfully joined the room.");
    }

    #endregion

    #region Getters & Setters

    /*
     * @brief This method is used to get the player count in the room.
     */
    public int GetPlayerCount()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    /*
     * @brief This method is used to get the max players in the room.
     */
    public int GetMaxPlayersRoom()
    {
        return PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    /*
     * @brief This method is used to check if the client is the master client.
     */
    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    #endregion

    #region PunCallbacks

    public override void OnLeftRoom()
    {
        OnPlayerLeftRoomAction?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player " + newPlayer.NickName + " joined the room!");

        InitializePlayer();
    }

    #endregion
}
