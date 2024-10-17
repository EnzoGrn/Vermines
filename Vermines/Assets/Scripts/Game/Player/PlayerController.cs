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

    public Vermines.PlayerData PlayerData;

    #endregion

    #region Methods

    public override void OnEnable()
    {
        _POV = GetComponent<PhotonView>();

        PlayerData.enabled = true;
    }

    public override void OnDisable()
    {
        PlayerData.enabled = false;
    }

    public void Start()
    {
        /*if (_POV.IsMine) {
            localPlayer = this;

            Debug.Log("Nickname: " + Vermines.PlayerData.Instance.Data.Profile.Nickname); // Print the nickname of the player

            SyncPlayer(Vermines.PlayerData.Instance);
        }*/
    }

    #endregion

    #region RPC functions

    [SerializeField]
    private Vermines.Data _MyData;

    public void SyncPlayer(PlayerData player)
    {
        string syncJson = player.DataToString();

        _POV.RPC("RPC_SyncPlayer", RpcTarget.OthersBuffered, syncJson);
    }

    [PunRPC]
    public void RPC_SyncPlayer(string data)
    {
        _MyData = JsonUtility.FromJson<Vermines.Data>(data);

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

            _MyData = JsonUtility.FromJson<Vermines.Data>(data);
        }
    }

    #endregion
}
