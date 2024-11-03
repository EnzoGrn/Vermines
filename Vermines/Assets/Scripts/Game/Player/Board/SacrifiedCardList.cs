using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class SacrifiedCardList : MonoBehaviourPunCallbacks
{
    public List<CardView> sacrifiedCards = new List<CardView>();

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
        sacrifiedCards.Clear();

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
        Debug.Log("Scycing data ... total of player cards -> " + data.SacrifiedDeck.Cards.Count);

        if (sacrifiedCards.Count < 0 || data == null)
        {
            return;
        }

        for (int i = 0; i < data.SacrifiedDeck.Cards.Count; i++)
        {
            CardView card = Instantiate(Resources.Load<GameObject>(Constants.CardPref), transform.position, Quaternion.identity).GetComponent<CardView>();
            card.SetCard(data.SacrifiedDeck.Cards[i]);

            // Set IsAnonyme to false
            card.GetCard().IsAnonyme = false;
            card.transform.position = new Vector3(transform.position.x, (float)(transform.position.y + i * 0.05 + 0.2), transform.position.z);
            card.transform.Rotate(90, 180, 0);
            card.gameObject.SetActive(true);
            card.gameObject.transform.Find("Back").gameObject.SetActive(false);
        }
    }


    private void UpdateCardPosition()
    {
        for (int i = 0; i < sacrifiedCards.Count; i++)
        {
            sacrifiedCards[i].transform.position = new Vector3(transform.position.x, (float)(transform.position.y + i * 0.05 + 0.2), transform.position.z);
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

            _PlayerData.Data.SacrifiedDeck.AddCard(card.GetCard());
            sacrifiedCards.Add(card);
            UpdateCardPosition();
        }
    }

    public void RemoveCard(CardView card)
    {
        if (_POV.IsMine)
        {
            sacrifiedCards.Remove(card);
            _PlayerData.Data.SacrifiedDeck.RemoveCard(card.GetCard());
            UpdateCardPosition();
        }
    }
}
