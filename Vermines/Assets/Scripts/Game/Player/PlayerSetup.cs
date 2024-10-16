using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * @brief This class have the objective to setup the player when he join the game.
 */
public class PlayerSetup : MonoBehaviour {

    public void IsLocalPlayer()
    {
        Debug.Log($"Hi {PhotonNetwork.LocalPlayer.NickName}, here is your player instance.");
    }
}
