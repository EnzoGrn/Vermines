using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class PlayedCardList : MonoBehaviourPunCallbacks
{
    #region Attributes
    public List<PlayedCell> playedCards = new List<PlayedCell>();
    public List<CardView> otherCards = new List<CardView>();

    public float spaceBetween = 1.0f;
    public int maxCardNumber = 3; // Up to 4 with a specific god

    [SerializeField]
    private PhotonView _POV;

    [SerializeField]
    private PlayerData _PlayerData;

    [SerializeField]
    private SacrifiedCardList _SacrifiedCardList;
    #endregion

    #region Methods
    public void Init()
    {
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
        if (data == null)
        {
            return;
        }

        // Check if card need to be removed
        for (int i = 0; i < otherCards.Count; i++)
        {
            // check if card is in the played deck
            if (!data.PlayedDeck.Cards.Contains(otherCards[i].GetCard()))
            {
                Destroy(otherCards[i].gameObject);
                otherCards.RemoveAt(i);
            }
        }

        // Check if card need to be added
        if (playedCards.Count != data.PlayedDeck.Cards.Count)
        {
            for (int i = 0; i < data.PlayedDeck.Cards.Count; i++)
            {
                // check if the card is contained in the otherCards list if not 
                if (!otherCards.Find(card => card.GetCard().ID == data.PlayedDeck.Cards[i].ID))
                {
                    CardView card = Instantiate(Resources.Load<GameObject>(Constants.CardPref), transform.position, Quaternion.identity).GetComponent<CardView>();
                    card.SetCard(data.PlayedDeck.Cards[i]);

                    card.GetCard().IsAnonyme = false;
                    card.transform.position = new Vector3(transform.position.x + i * spaceBetween - 6, transform.position.y + 0.5f, transform.position.z);
                    card.transform.Rotate(90, 180, 0);
                    card.gameObject.SetActive(true);
                    card.gameObject.transform.Find("Back").gameObject.SetActive(false);

                    otherCards.Add(card);
                }
            }
        }

        for (int i = 0; i < otherCards.Count; i++)
        {
            otherCards[i].transform.position = new Vector3(transform.position.x + i * spaceBetween - 6, transform.position.y + 0.5f, transform.position.z);
        }
    }


    private void UpdateCardPosition()
    {
        for (int i = 0; i < playedCards.Count; i++)
        {
            playedCards[i].transform.position = new Vector3(transform.position.x + i * spaceBetween - 6 , transform.position.y + 0.5f, transform.position.z);
        }

        if (_POV.IsMine)
        {
            // Sync Players
            SyncPlayer(_PlayerData);
            PlayerController.localPlayer.Sync();
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

            // Remove Card From the Hand
            _PlayerData.Data.HandDeck.RemoveCard(card.GetCard());

            card.gameObject.SetActive(true);

            _PlayerData.Data.PlayedDeck.AddCard(card.GetCard());

            PlayedCell playedCell = card.gameObject.AddComponent<PlayedCell>();
            playedCell.CardView = card;
            playedCell.OnClick += SacrifyCard;

            playedCards.Add(playedCell);
            UpdateCardPosition();
        }
    }

    public void SacrifyCard(PlayedCell card)
    {
        Debug.Log("Sacrify card");

        card.OnClick -= SacrifyCard;

        _SacrifiedCardList.AddCard(card.CardView);
        RemoveCard(card);
    }

    public void RemoveCard(PlayedCell cell)
    {
        if (_POV.IsMine)
        {
            playedCards.Remove(cell);
            _PlayerData.Data.PlayedDeck.RemoveCard(cell.CardView.GetCard());
            UpdateCardPosition();
        }
    }
    #endregion
}
