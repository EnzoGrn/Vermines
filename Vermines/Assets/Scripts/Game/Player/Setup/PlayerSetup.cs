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
    public PlayerController PlayerController;
    //public PlayerSetupView  View;

    #endregion

    public void IsLocalPlayer()
    {
        PlayerController.enabled = true;
        //View.enabled             = true;
    }
}
