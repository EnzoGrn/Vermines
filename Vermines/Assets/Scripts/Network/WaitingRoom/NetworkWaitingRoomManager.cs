using UnityEngine;
using System;

#region PUN2 Imports
using Photon.Pun;
using Photon.Realtime;
#endregion

public class NetworkWaitingRoomManager : MonoBehaviourPunCallbacks
{
    #region Public Attributes
    /*
     * @brief This event is triggered when a player left the room.
     */
    public event Action OnPlayerLeftRoomAction;
    /*
     * @brief This event is triggered when the client left the room.
     */
    public event Action OnLeftRoomAction;
    /*
     * @brief This event is triggered when a player enter in the room.
     */
    public event Action OnPlayerEnteredRoomAction;

    #endregion

    #region Methods Implementation
    /*
     * @brief This method is used to leave the waiting room.
     * 
     * @param void
     * 
     * @return void
     */
    public void LeaveWaitingRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Client leaving room");
            PhotonNetwork.LeaveRoom();
        }
    }

    /*
     * @brief This method is used to get the room code.
     * 
     * @param void
     * 
     * @return string
     */
    public string GetRoomCode()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            return PhotonNetwork.CurrentRoom.Name;
        }
        return new string('*', PhotonNetwork.CurrentRoom.Name.Length);

    }

    /*
     * @brief This method is used to get the max players in the room.
     * 
     * @param void
     * 
     * @return int
     */
    public int GetMaxPlayersRoom()
    {
        return PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    /*
     * @brief This method is used to get the player count in the room.
     * 
     * @param void
     * 
     * @return int
     */
    public int GetPlayerCount()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    /*
     * @brief This method is used to check if the client is the master client.
     * 
     * @param void
     * 
     * @return bool
     */
    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    /*
     * @brief This method is used to get the players names in the room.
     * 
     * @param void
     * 
     * @return string
     */
    public string GetPlayersNames()
    {
        string playersNames = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log("Players name : " + player.NickName);
            playersNames += player.NickName + "\n";
        }
        return playersNames;
    }

    /*
     * @brief This method is used to start the match.
     * 
     * @param void
     * 
     * @return void
     */
    public void LoadLevel(string levelName)
    {
        PhotonNetwork.LoadLevel(levelName);
    }

    #endregion

    #region PunCallbacks
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoomDebug");
        OnLeftRoomAction?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoomDebug");
        OnPlayerLeftRoomAction?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player " + newPlayer.NickName + " joined the room !");
        OnPlayerEnteredRoomAction?.Invoke();
    }
    #endregion

}
