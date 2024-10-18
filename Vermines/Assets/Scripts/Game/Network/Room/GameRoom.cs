using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoom : AGameRoomManager {

    #region Methods

    protected override void Awake()
    {
        base.Awake();

        GetNetworkManager().InitializePlayer();
    }

    /*
     * @brief This method is an event called by the InitializePlayer method.
     * It will create the player on the scene and setup it.
     */
    public override void OnPlayerEnteredRoom()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(Constants.PlayerPref, Vector3.zero, Quaternion.identity);

        playerObj.GetComponent<PlayerSetup>().IsLocalPlayer();
    }

    #endregion
}
