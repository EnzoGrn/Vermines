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
	private int _currentPlayerIndex = 0;

	public CardManager CardManager;
    public GameInfo    GameInfo;

    #endregion

    #region Action

    private Action _EveryPlayersLoadCallbacks;
    private Action _InitializationOfPlayersDeck;
	private Action _StartTurnAction;

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
		_StartTurnAction?.Invoke();
	}

	private Action _StartingDraw;

    private void DrawCardBegin()
    {
        PlayerController.localPlayer.DrawCard(2);

        _StartingDraw -= DrawCardBegin;
        // TODO: Gain 2 eloquence for the player.
        // TODO: Card effect for gaining eloquence.
        // TODO: Market phase.
        // TODO: End of the turn.
    }

    private void StartTurn()
	{
		var currentPlayer = GetCurrentPlayer();
		if (currentPlayer != null)
		{
            Debug.Log("Start turn for player " + currentPlayer.Profile.Nickname);
			currentPlayer.Eloquence += 2;
            currentPlayer.Sync();
			Debug.Log("Player eloquence: " + currentPlayer.Eloquence);

			// TODO: Implement other phases like card activation, market phase, etc.
			// End the turn and trigger the next player's turn in the NextTurn method or similar
			// /!\ It's better if EndTurn is called by clicking on a button or something like that, for now it's called automatically
			//EndTurn();
		}

		_StartTurnAction -= StartTurn;
	}

	private PlayerController GetCurrentPlayer()
	{
		return _Players.TryGetValue(_currentPlayerIndex, out PlayerController player) ? player : null;
        //return PlayerController.LocalPlayer;
	}

	private void NextTurn()
	{
		_currentPlayerIndex = (_currentPlayerIndex + 1) % _Players.Count;

        //_Players[i].Profile.PlayerID;

		SyncGameManager("ChangeTurn");
	}

	private void EndTurn()
	{
		var currentPlayer = GetCurrentPlayer();

		// TODO: Implement market refilling logic

		currentPlayer.DrawCard(3);
        Debug.Log("End turn for player " + currentPlayer.Profile.Nickname);

		NextTurn();
	}

	private void ChangeTurn()
	{
		_StartTurnAction += StartTurn;
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

				_Players.Add(i, controllers[i]); // Currently the players are sort by an index number, maybe we should sort them by their playerID
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

		if (PhotonNetwork.IsMasterClient)
			ChangeTurn();
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
		Debug.Log("Received RPC command: " + command);
		if (!string.IsNullOrEmpty(command)) {
            _Command = command;

            HandleCommand();
        }
	}

    private void HandleCommand()
    {
        if (_Command == "ClientReady")
            _InitializationOfPlayersDeck += InitPlayerDeck;
		else if (_Command == "ChangeTurn")
		{
			Debug.Log("Changing turn...");
			ChangeTurn();
		}
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(_Command);
			stream.SendNext(_currentPlayerIndex);
		} else {
            _Command = (string)stream.ReceiveNext();
			_currentPlayerIndex = (int)stream.ReceiveNext();
		}
    }

    #endregion
}
