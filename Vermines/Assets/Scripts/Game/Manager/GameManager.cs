using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable {

    #region Attributes

    static public GameManager Instance;

    [SerializeField]
    private Dictionary<int, PlayerController> _Players = new();

    private bool _IsGameStarted = false;
	private int _currentPlayerIndex = 0;
	private List<int> _playerTurnOrder;

	public CardManager CardManager;
    public GameInfo    GameInfo;
    public TMP_Text    TurnText;

    public ShopManager Shop;

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

        if (PhotonNetwork.IsMasterClient) {
            GameObject shop = PhotonNetwork.Instantiate("Prefabs/Game/Shop/Shop", Vector3.zero, Quaternion.identity);

            Shop = shop.GetComponent<ShopManager>();
        } else {
            Shop = FindObjectOfType<ShopManager>();
        }

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
		TurnText.text = "Turn of " + _currentPlayerIndex;
	}

	private Action _StartingDraw;

    private void DrawCardBegin()
    {
        PlayerController.localPlayer.DrawCard(2);

        _StartingDraw -= DrawCardBegin;
    }

	private void StartTurn()
	{
        Shop.Refill();

        //Debug.Log("Starting turn for local player with ID " + PlayerController.localPlayer.Profile.PlayerID);
        PlayerController.localPlayer.Eloquence += 2;
		PlayerController.localPlayer.Sync();

		TurnText.text = "It's your turn!";
	}

	// TODO: Add additional turn-based actions here, and call it in button event.

	private void NextTurn()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			int currentIndex = _playerTurnOrder.IndexOf(_currentPlayerIndex);
			int nextIndex = (currentIndex + 1) % _playerTurnOrder.Count;
			_currentPlayerIndex = _playerTurnOrder[nextIndex];

			//Debug.Log("Next turn for player ID " + _currentPlayerIndex);
			photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, _currentPlayerIndex);
		}
	}

	private void InitializePlayerTurnOrder()
	{
		List<KeyValuePair<int, int>> playersWithEloquence = new List<KeyValuePair<int, int>>();

		foreach (var player in _Players)
		{
			//Debug.Log($"Player ID: {player.Key}, Eloquence: {player.Value.Eloquence}");
			playersWithEloquence.Add(new KeyValuePair<int, int>(player.Key, player.Value.Eloquence));
		}

		playersWithEloquence.Sort((a, b) =>
		{
			int compareEloquence = a.Value.CompareTo(b.Value);
			if (compareEloquence == 0)
			{
				return a.Key.CompareTo(b.Key);
			}
			return compareEloquence;
		});

		_playerTurnOrder = playersWithEloquence.ConvertAll(pair => pair.Key);
		_currentPlayerIndex = _playerTurnOrder[0];

		//Debug.Log("Turn order initialized. Starting with player ID: " + _currentPlayerIndex);
	}

	private void EndTurn()
	{
		photonView.RPC("RPC_EndTurn", RpcTarget.MasterClient, PlayerController.localPlayer.Profile.PlayerID);
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
        List<CardType> cardTypes = new();

        if (PhotonNetwork.IsMasterClient) {
            GameInfo.SelectEveryFamily(PhotonNetwork.CurrentRoom.PlayerCount);

            for (int i = 0; i < controllers.Length; i++) {
                int startingEloquence = i > 2 ? 2 : i;

                controllers[i].Family = GameInfo.FamilyPlayed[i];
                controllers[i].Eloquence = startingEloquence;
                controllers[i].Sync();

                _Players.Add(controllers[i].Profile.PlayerID, controllers[i]);
            }

            InitializePlayerTurnOrder();

            _Command = "ClientReady";

            SyncGameManager(_Command);
        } else {
            for (int i = 0; i < controllers.Length; i++)
                _Players.Add(controllers[i].Profile.PlayerID, controllers[i]);

            InitializePlayerTurnOrder();

            cardTypes.Clear();

            for (int i = 0; i < controllers.Length; i++) {
                int id              = _playerTurnOrder[i];
                PlayerController pc = GetPlayersByID(id);

                if (pc != null) {
                    if (pc.Family == CardType.None)
                        return;
                    cardTypes.Add(pc.Family);
                }
            }

            GameInfo.FamilyPlayed = cardTypes;

            _Command = "ClientReady";

			SyncGameManager(_Command);
        }

        CardManager.enabled = true;
        _EveryPlayersLoadCallbacks -= OnAllPlayersJoined;
    }

    private PlayerController GetPlayersByID(int id)
    {
        PlayerController pc = null;

        foreach (var player in _Players) {
            if (player.Value.Profile.PlayerID == id) {
                pc = player.Value;

                break;
            }
        }

        return pc;
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
            photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, _currentPlayerIndex);
        }

        _InitializationOfPlayersDeck -= InitPlayerDeck;
        _StartingDraw                += DrawCardBegin;

        if (Shop == null)
            Shop = FindObjectOfType<ShopManager>();

        Shop.enabled = true;
        _IsGameStarted = true;
	}

    #endregion

    #region Getters

    public List<CardType> GetAllFamilyPlayed()
    {
        return GameInfo.FamilyPlayed;
    }

    public bool IsMyTurn()
    {
        if (!_IsGameStarted)
            return false;
        return _currentPlayerIndex == PlayerController.localPlayer.Profile.PlayerID;
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

	[PunRPC]
	public void RPC_SetCurrentPlayerIndex(int newPlayerIndex)
	{
		_currentPlayerIndex = newPlayerIndex;
		//Debug.Log("Received updated turn index: " + _currentPlayerIndex);

		if (_currentPlayerIndex == PlayerController.localPlayer.Profile.PlayerID) {
            if (_IsGameStarted)
			//Debug.Log("It's the local player's turn. Starting turn for local player.");
    			StartTurn();
		}
		//else
		//{
		//	Debug.Log("Waiting for player " + _currentPlayerIndex + " to take their turn.");
		//}
	}

	[PunRPC]
	public void RPC_EndTurn(int playerID)
	{
		//Debug.Log("Player " + playerID + " has ended their turn.");

		if (_Players.TryGetValue(playerID, out PlayerController player)) {
			if (_currentPlayerIndex == playerID) {
				player.photonView.RPC("RPC_DrawCards", RpcTarget.All, 3);
				//Debug.Log("End turn for player " + player.Profile.Nickname);
				NextTurn();
			}
			//else
			//{
			//	Debug.Log("It's not your turn yet.");
			//}
		}
		else
		{
			Debug.LogError("Player with ID " + playerID + " not found.");
		}
	}

	private void HandleCommand()
    {
        if (_Command == "ClientReady")
            _InitializationOfPlayersDeck += InitPlayerDeck;

		_Command = string.Empty;
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
