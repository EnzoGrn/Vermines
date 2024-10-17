using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Vermines;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable {

    #region Attributes

    public static PlayerController localPlayer;

    private PhotonView _POV;

    [SerializeField]
    public PlayerData _PlayerData;

    public PlayerSetupView View;

    #endregion

    #region Methods

    public override void OnEnable()
    {
        _POV = GetComponent<PhotonView>();

        View.enabled = true;

        if (_POV.IsMine) {
            _PlayerData.enabled = true;

            _PlayerData.Data.Profile.Nickname = PhotonNetwork.LocalPlayer.NickName;
            _PlayerData.Data.Profile.PlayerID = PhotonNetwork.LocalPlayer.ActorNumber;

            View.EditView(_PlayerData.Data);
        }
    }

    public override void OnDisable() {}

    public void Start()
    {
        if (_POV.IsMine) {
            localPlayer = this;

            SyncPlayer(_PlayerData);
        }
    }

    #endregion

    #region RPC functions

    [SerializeField]
    private Data _MyData;

    public void SyncPlayer(PlayerData player)
    {
        string syncJson = player.DataToString();

        _POV.RPC("RPC_SyncPlayer", RpcTarget.OthersBuffered, syncJson);
    }

    [PunRPC]
    public void RPC_SyncPlayer(string data)
    {
        _MyData = JsonUtility.FromJson<Data>(data);

        View.EditView(_MyData);

        Debug.Log("Nickname: " + _MyData.Profile.Nickname);
    }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(JsonUtility.ToJson(_MyData));
        } else {
            string data = (string)stream.ReceiveNext();

            _MyData = JsonUtility.FromJson<Data>(data);
        }
    }

    #endregion
}
