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
    private PlayerData _PlayerData;

    public PlayerSetupView View;

    #endregion

    #region Methods

    public override void OnEnable()
    {
        _POV = GetComponent<PhotonView>();
    }

    public override void OnDisable() { }

    public void Init()
    {
        View.enabled = true;

        if (_POV.IsMine) {
            _PlayerData.Data.Profile.Nickname = PhotonNetwork.LocalPlayer.NickName;
            _PlayerData.Data.Profile.PlayerID = PhotonNetwork.LocalPlayer.ActorNumber;

            View.EditView(_PlayerData.Data);
        }
    }

    public void Start()
    {
        if (_POV.IsMine) {
            SyncPlayer(_PlayerData);

            localPlayer = this;
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

        _PlayerData.Data = _MyData;
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

    #region Setters

    public CardType Family
    {
        get => _PlayerData.Data.Family;
        set
        {
            if (value != _PlayerData.Data.Family) {
                _PlayerData.Data.Family = value;

                View.EditView(_PlayerData.Data);

                SyncPlayer(_PlayerData);
            }
        }
    }

    public int Eloquence
    {
        get => _PlayerData.Data.Eloquence;
        set
        {
            if (value != _PlayerData.Data.Eloquence) {
                _PlayerData.Data.Eloquence = value;

                View.EditView(_PlayerData.Data);

                SyncPlayer(_PlayerData);
            } 
        }
    }

    public Deck Deck
    {
        get => _PlayerData.Data.Deck;
        set
        {
            _PlayerData.Data.Deck = value;

            SyncPlayer(_PlayerData);
        }
    }

    #endregion
}
