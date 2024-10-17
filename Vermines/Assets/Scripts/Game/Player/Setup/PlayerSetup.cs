using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * @brief This class have the objective to setup the player when he join the game.
 */
public class PlayerSetup : MonoBehaviour {

    #region Attributes /* GameObject to setup */

    [Header("GameObject to setup")]
    public PlayerSetupView View;

    #endregion

    public void IsLocalPlayer()
    {
        Debug.Log($"Hi {PhotonNetwork.LocalPlayer.NickName}! Here is your player instance.");

        View.enabled = true;
    }
}
