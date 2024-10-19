using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable {

    #region Attributes

    static public GameManager Instance;

    [SerializeField]
    private Dictionary<int, PlayerController> _Players = new();

    private bool _IsGameStarted = false;

    public CardManager CardManager;
    public GameInfo    GameInfo;

    #endregion

    #region Action

    private Action _EveryPlayersLoadCallbacks;
    private Action _InitializationOfPlayersDeck;

    #endregion

    #region Methods

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        _EveryPlayersLoadCallbacks += OnAllPlayersJoined;

        GameInfo.enabled = true;
    }

    public void FixedUpdate()
    {
        _EveryPlayersLoadCallbacks?.Invoke();   // Call this event until all players have joined the game room, after that it will be removed from the callback list.
        _InitializationOfPlayersDeck?.Invoke(); // Call this event client are ready to initialize their deck.
    }
    
    public void Update()
    {
        if (!_IsGameStarted)
            return;
        _StartingDraw?.Invoke();
    }

    private Action _StartingDraw;

    private void DrawCardBegin()
    {
        PlayerController.localPlayer.DrawCard(2);

        _StartingDraw -= DrawCardBegin;
    }

    /*
     * @brief Event called when all players have joined the game room.
     * If used for initialization, it should be removed from the callback list.
     * 
     * @note Only the master client can call this event.
     */
    private void OnAllPlayersJoined()
    {
        PlayerController[] controllers = FindObjectsOfType<PlayerController>();

        if (controllers.Length != PhotonNetwork.CurrentRoom.PlayerCount)
            return;
        _Players.Clear();

        if (!PhotonNetwork.IsMasterClient) {
            List<CardType> cardTypes = new();

            for (int i = 0; i < controllers.Length; i++) {
                if (controllers[i].Family == CardType.None)
                    return;
                cardTypes.Add(controllers[i].Family);
            }
            GameInfo.FamilyPlayed = cardTypes;

            _Command = "ClientReady";

            SyncGameManager(_Command);
        } else {
            GameInfo.SelectEveryFamily(PhotonNetwork.CurrentRoom.PlayerCount); // Select every family for each player, and sync with the other clients the family chosen.

            for (int i = 0; i < controllers.Length; i++) {
                int startingEloquence = i > 2 ? 2 : i; // Give eloquence to the player depending in their order (First: 0E, Secound: 1E, Third+: 2E).

                controllers[i].Family    = GameInfo.FamilyPlayed[i];
                controllers[i].Eloquence = startingEloquence;
                controllers[i].Sync();

                _Players.Add(i, controllers[i]); // Currently the players are sort by an index number, maybe we should sort them by their playerID
            }
        }

        CardManager.enabled = true;

        _EveryPlayersLoadCallbacks -= OnAllPlayersJoined;
    }

    private void InitPlayerDeck()
    {
        if (PhotonNetwork.IsMasterClient) {
            PlayerController[] controllers = FindObjectsOfType<PlayerController>();

            for (int i = 0; i < controllers.Length; i++) {
                controllers[i].Deck = CardManager.GetPlayerDeck(i);
                controllers[i].Sync();
            }
            _Command = "ClientReady";

            SyncGameManager(_Command);
        }
        _InitializationOfPlayersDeck -= InitPlayerDeck;
        _StartingDraw                += DrawCardBegin;
        _IsGameStarted = true;
    }

    #endregion

    #region Getters

    public List<CardType> GetAllFamilyPlayed()
    {
        return GameInfo.FamilyPlayed;
    }

    #endregion

    #region IPunObservable implementation

    private string _Command = string.Empty;

    private void SyncGameManager(string command)
    {
        // RPC without photonView

        photonView.RPC("RPC_SyncGameManager", RpcTarget.OthersBuffered, command);
    }

    [PunRPC]
    public void RPC_SyncGameManager(string command)
    {
        if (!string.IsNullOrEmpty(command)) {
            _Command = command;

            HandleCommand();
        }
    }

    private void HandleCommand()
    {
        if (_Command == "ClientReady")
            _InitializationOfPlayersDeck += InitPlayerDeck;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(_Command);
        } else {
            _Command = (string)stream.ReceiveNext();
        }
    }

    #endregion
}
