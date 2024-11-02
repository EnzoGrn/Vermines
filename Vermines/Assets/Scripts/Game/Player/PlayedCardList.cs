using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Vermines;

public class PlayedCardList : MonoBehaviourPunCallbacks
{
    public List<CardView> playedCards = new List<CardView>();
    public float spaceBetween = 1.0f;
    public int maxCardNumber = 3; // Up to 4 with a specific god

    [SerializeField]
    private PhotonView _POV;

    [SerializeField]
    private PlayerData _PlayerData;
    
    public void Awake()
    {

    }

    public void Init()
    {
        Debug.Log("Init PlayedCardList");
        playedCards.Clear();

        // Enable the view
        gameObject.SetActive(true);
    }

    public void SyncPlayer(PlayerData player)
    {
        string syncJson = player.DataToString();

        _POV.RPC("SyncPlayedCard", RpcTarget.OthersBuffered , syncJson);
    }

    [PunRPC]
    public void SyncPlayedCard(string syncJson)
    {
        Data data = JsonUtility.FromJson<Data>(syncJson);
        UpdateReceivedCard(data);
    }

    private void UpdateReceivedCard(Data data)
    {
        Debug.Log("Scycing data ... total of player cards -> " + data.PlayedDeck.Cards.Count);

        if (playedCards.Count < 0 || data == null)
        {
            return;
        }

        for (int i = 0; i < data.PlayedDeck.Cards.Count; i++)
        {
            CardView card = Instantiate(Resources.Load<GameObject>(Constants.CardPref), transform.position, Quaternion.identity).GetComponent<CardView>();
            card.SetCard(data.PlayedDeck.Cards[i]);

            // Set IsAnonyme to false
            card.GetCard().IsAnonyme = false;
            card.transform.position = new Vector3(transform.position.x + i * spaceBetween - 6, transform.position.y + 1, transform.position.z);
            card.transform.Rotate(90, 180, 0);
            card.gameObject.SetActive(true);
            card.gameObject.transform.Find("Back").gameObject.SetActive(false);
        }
    }


    private void UpdateCardPosition()
    {
        for (int i = 0; i < playedCards.Count; i++)
        {
            playedCards[i].transform.position = new Vector3(transform.position.x + i * spaceBetween - 6 , transform.position.y + 1, transform.position.z);
        }

        if (_POV.IsMine)
        {
            // Sync Players
            Debug.Log("Need to sync Data !");
            SyncPlayer(_PlayerData);
        }
    }

    public void AddCard(CardView card)
    {
        if (_POV.IsMine)
        {
            if (playedCards.Count + 1 > maxCardNumber || card.GetCard() == null)
            {
                Destroy(card.gameObject);
                Debug.Log("Played card list is full");
                return;
            }
            card.gameObject.SetActive(true);

            _PlayerData.Data.PlayedDeck.AddCard(card.GetCard());
            playedCards.Add(card);
            UpdateCardPosition();
        }
    }

    public void RemoveCard(CardView card)
    {
        if (_POV.IsMine)
        {
            playedCards.Remove(card);
            // _data.PlayedDeck.RemoveCard(card.GetCard());
            UpdateCardPosition();
        }
    }
}
