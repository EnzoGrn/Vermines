using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks {

    private List<Vermines.Player> _ConnectedPlayer = new();

    private void Awake()
    {
        _ConnectedPlayer = new();

        if (Constants.PlayGround == null)
            Instantiate(Resources.Load<GameObject>(Constants.PlayGroundPref));
        AddNewPlayerToList(PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void AddNewPlayerToList(string nickname, int playerID)
    {
        GameObject playerObj = PhotonNetwork.Instantiate(Constants.PlayerPref, Vector3.zero, Quaternion.identity);

        Vermines.Player player = playerObj.GetComponent<Vermines.Player>();

        Debug.Assert(player != null, "Player not found");

        Config.PlayerProfile profile = new() {
            Nickname = nickname,
            PlayerID = playerID
        };

        player.SetProfile(profile);

        Debug.Log($"Player {nickname} {playerID} has been added to the game");
    }

    public void FixedUpdate()
    {
        if (_ConnectedPlayer.Count != PhotonNetwork.CurrentRoom.PlayerCount)
            UpdateConnectedPlayerList();
    }

    private void UpdateConnectedPlayerList()
    {
        _ConnectedPlayer.Clear();

        foreach (Vermines.Player player in FindObjectsOfType<Vermines.Player>()) {
            PhotonView photonView = player.GetComponent<PhotonView>();

            if (photonView == null)
                Debug.Assert(false, "PhotonView not found");
            else {
                Config.PlayerProfile profile = new() {
                    Nickname = photonView.Owner.NickName,
                    PlayerID = photonView.Owner.ActorNumber
                };

                player.SetProfile(profile);

                _ConnectedPlayer.Add(player);
            }
        }
    }
}
