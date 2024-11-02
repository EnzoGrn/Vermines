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

    public override void OnDisable() {}

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
            localPlayer = this;

            SyncPlayer(_PlayerData);
        }
    }

    /*
     * @brief Function that draw a number of card from the deck into the hand.
     */
    public void DrawCard(int numberOfCardToDraw)
    {
        if (_POV.IsMine) {
            for (int i = 0; i < numberOfCardToDraw; i++) {
                ICard card = _PlayerData.Data.Deck.PickACard();

                if (card == null) {
                    // TODO: Merge the discard pile with the deck
                    // Shuffle the deck and pick a card
                    break; // Temp break
                }

                card.IsAnonyme = false;

                _PlayerData.Data.HandDeck.AddCard(card);

                SyncPlayer(_PlayerData);

                View.AddCardToHand(card);
            }
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

    public void Sync()
    {
        SyncPlayer(_PlayerData);
    }

    [PunRPC]
    public void RPC_SyncPlayer(string playerDataJson)
    {
        if (!string.IsNullOrEmpty(playerDataJson)) {
            _MyData = JsonUtility.FromJson<Data>(playerDataJson);

            View.EditView(_MyData);
            View.EditHandView(_MyData);

            Debug.Log($"Player {_MyData.Profile.Nickname} has been updated");
            Debug.Log($"Data: {playerDataJson}");

            _PlayerData.Data = _MyData;
        }
    }

	[PunRPC]
	public void RPC_DrawCards(int numberOfCards)
	{
		DrawCard(numberOfCards);
	}

	#endregion

	#region IPunObservable implementation

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(JsonUtility.ToJson(_MyData));
        } else {
            string playerDataJson = (string)stream.ReceiveNext();

            _MyData = JsonUtility.FromJson<Data>(playerDataJson);
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
            }
        }
    }

    public Deck Deck
    {
        get => _PlayerData.Data.Deck;
        set
        {
            _PlayerData.Data.Deck = value;
        }
    }

    public Config.PlayerProfile Profile
    {
        get => _PlayerData.Data.Profile;
    }

	#endregion
}
