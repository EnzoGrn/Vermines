using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class DiscardedCardList : MonoBehaviourPunCallbacks
{
    public List<CardView> discardedCards = new List<CardView>();

    [SerializeField]
    private PhotonView _POV;

    [SerializeField]
    private PlayerData _PlayerData;

    public void Awake()
    {

    }

    public void Init()
    {
        Debug.Log("Init DiscardedCardList");
        discardedCards.Clear();

        // Enable the view
        gameObject.SetActive(true);
    }

    public void SyncPlayer(PlayerData player)
    {
        string syncJson = player.DataToString();

        _POV.RPC("SyncPlayedCard", RpcTarget.OthersBuffered, syncJson);
    }

    [PunRPC]
    public void SyncPlayedCard(string syncJson)
    {
        Data data = JsonUtility.FromJson<Data>(syncJson);
        UpdateReceivedCard(data);
    }

    private void UpdateReceivedCard(Data data)
    {
        Debug.Log("Scycing data ... total of player cards -> " + data.DiscardDeck.Cards.Count);

        if (discardedCards.Count < 0 || data == null)
        {
            return;
        }

        for (int i = 0; i < data.DiscardDeck.Cards.Count; i++)
        {
            CardView card = Instantiate(Resources.Load<GameObject>(Constants.CardPref), transform.position, Quaternion.identity).GetComponent<CardView>();
            card.SetCard(data.DiscardDeck.Cards[i]);

            // Set IsAnonyme to false
            card.GetCard().IsAnonyme = false;
            card.transform.position = new Vector3(transform.position.x, (float)(transform.position.y + i * 0.05), transform.position.z);
            card.transform.Rotate(90, 180, 0);
            card.gameObject.SetActive(true);
            card.gameObject.transform.Find("Back").gameObject.SetActive(false);
        }
    }


    private void UpdateCardPosition()
    {
        for (int i = 0; i < discardedCards.Count; i++)
        {
            discardedCards[i].transform.position = new Vector3(transform.position.x, (float)(transform.position.y + i * 0.05), transform.position.z);
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
            if (card.GetCard() == null)
            {
                Destroy(card.gameObject);
                Debug.Log("Played card list is full");
                return;
            }
            card.gameObject.SetActive(true);

            _PlayerData.Data.DiscardDeck.AddCard(card.GetCard());
            discardedCards.Add(card);
            UpdateCardPosition();
        }
    }

    public void RemoveCard(CardView card)
    {
        if (_POV.IsMine)
        {
            discardedCards.Remove(card);
            _PlayerData.Data.DiscardDeck.RemoveCard(card.GetCard());
            UpdateCardPosition();
        }
    }
}
