using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#region PUN2 Imports
using Photon.Pun;
using Photon.Realtime;
#endregion

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Singleton
    public static GameManager gameManagerInstance;
    #endregion

    #region [SerializeField] Private Fields
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _roomPasswordText;
    #endregion

    #region Private Fields
    private string[] _roomName;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (gameManagerInstance == null)
        {
            gameManagerInstance = this;
        }

        displayRoomInfo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void displayRoomInfo()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // parse with "_" and display name and password
            _roomName = PhotonNetwork.CurrentRoom.Name.Split('_');

            if (_roomName.Length == 3)
            {
                _roomNameText.text = "Room name : " + _roomName[1];
                _roomPasswordText.text = "Room password : " + _roomName[2];
            }
            else
            {
                OnLeavePrivateRoom();
            }
        }
    }

    #region Events
    public void OnLeavePrivateRoom()
    {
        PhotonNetwork.LeaveRoom(this);
    }
    #endregion

    #region Callbacks
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        displayRoomInfo();
    }
    #endregion

}
