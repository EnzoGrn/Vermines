using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class GameManager : MonoBehaviourPunCallbacks {

    #region Attributes

    static public GameManager Instance;

    [SerializeField]
    private Dictionary<int, PlayerController> _Players = new();

    private bool _IsGameStarted = false;

    public CardManager CardManager;
    public GameInfo    GameInfo;

    #endregion

    #region Action

    Action EveryPlayersLoadCallbacks;

    #endregion

    #region Methods

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        EveryPlayersLoadCallbacks += OnAllPlayersJoined;

        GameInfo.enabled = true;
    }

    public void FixedUpdate()
    {
        EveryPlayersLoadCallbacks?.Invoke(); // Call this event until all players have joined the game room, after that it will be removed from the callback list.
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
        if (!PhotonNetwork.IsMasterClient) {
            List<CardType> cardTypes = new();

            _Players.Clear();

            for (int i = 0; i < controllers.Length; i++)
                cardTypes.Add(controllers[i].Family);
            GameInfo.FamilyPlayed = cardTypes;

            CardManager.enabled = true;
        } else {
            GameInfo.SelectEveryFamily(PhotonNetwork.CurrentRoom.PlayerCount); // Select every family for each player, and sync with the other clients the family chosen.

            _Players.Clear();

            for (int i = 0; i < controllers.Length; i++) {
                int startingEloquence = i > 2 ? 2 : i; // Give eloquence to the player depending in their order (First: 0E, Secound: 1E, Third+: 2E).

                controllers[i].Family = GameInfo.FamilyPlayed[i];
                controllers[i].Eloquence = startingEloquence;

                _Players.Add(i, controllers[i]); // Currently the players are sort by an index number, maybe we should sort them by their playerID
            }

            CardManager.enabled = true;

            //for (int i = 0; i < controllers.Length; i++)
            //    controllers[i].Deck = CardManager.GetPlayerDeck(i);

            // TODO: Initialisation... Set all decks...
        }

        Debug.Log($"Here is all the card loaded for each player and the market: {CardFactory.CardCount}");

        EveryPlayersLoadCallbacks -= OnAllPlayersJoined;

        _IsGameStarted = true;
    }

    #endregion

    #region Getters

    public List<CardType> GetAllFamilyPlayed()
    {
        return GameInfo.FamilyPlayed;
    }

    #endregion













    private List<Vermines.Player> _ConnectedPlayer = new();

    private void UpdateConnectedPlayerList()
    {
        /*_ConnectedPlayer.Clear();

        foreach (Vermines.Player player in FindObjectsOfType<Vermines.Player>()) {
            PhotonView photonView = player.GetComponent<PhotonView>();

            if (photonView == null)
                Debug.Assert(false, "PhotonView not found");
            else {
                Config.PlayerProfile profile = new() {
                    Nickname = photonView.Owner.NickName,
                    PlayerID = photonView.Owner.ActorNumber
                };

                player.SetProfile(profile);

                _ConnectedPlayer.Add(player);
            }
        }

        SetFamilyTypes();*/
    }

    #region Event

    private void EventOnAllPlayersJoined()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        CardLoader cardLoader = new();

        // Send the update to all other clien
    }

    #endregion

    #region Getters & Setters

    public int GetNumbersOfPlayer => PhotonNetwork.CurrentRoom.PlayerCount;
    public Vermines.Player GetPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _ConnectedPlayer.Count)
            return null;
        Vermines.Player player = null;

        for (int i = 0; i < _ConnectedPlayer.Count; i++) {
            if (i == playerIndex) {
                player = _ConnectedPlayer[i];

                break;
            }
        }

        return player;
    }

    #endregion

    /*static public GameManager Instance;

    private List<PlayerController> _ConnectedPlayer = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        _ConnectedPlayer = new();

        if (Constants.PlayGround == null)
            Instantiate(Resources.Load<GameObject>(Constants.PlayGroundPref));
        AddNewPlayerToList(PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void AddNewPlayerToList(string nickname, int playerID)
    {
        GameObject playerObj = PhotonNetwork.Instantiate("Prefabs/Player/AU_Player", Vector3.zero, Quaternion.identity);

        PlayerController player = playerObj.GetComponent<PlayerController>();

        Debug.Assert(player != null, "Player not found");

        Config.PlayerProfile profile = new() {
            Nickname = nickname,
            PlayerID = playerID
        };

        player.PlayerData.Data.Profile = profile;

        Debug.Log($"Player {nickname} {playerID} has been added to the game");
    }

    public void FixedUpdate()
    {
        if (_ConnectedPlayer.Count != PhotonNetwork.CurrentRoom.PlayerCount) {
            UpdateConnectedPlayerList();

            if (_ConnectedPlayer.Count == PhotonNetwork.CurrentRoom.PlayerCount)
                EventOnAllPlayersJoined();
        }
    }

    private void UpdateConnectedPlayerList()
    {
        _ConnectedPlayer.Clear();

        foreach (PlayerController player in FindObjectsOfType<PlayerController>()) {
            Config.PlayerProfile profile = new() {
                Nickname = photonView.Owner.NickName,
                PlayerID = photonView.Owner.ActorNumber
            };

            player.PlayerData.Data.Profile = profile;

            _ConnectedPlayer.Add(player);
        }

        SetFamilyTypes();
    }

    #region Event

    private void EventOnAllPlayersJoined()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        CardLoader cardLoader = new();

        // Send the update to all other clien
    }

    #endregion

    #region Getters & Setters

    public int GetNumbersOfPlayer => PhotonNetwork.CurrentRoom.PlayerCount;

    public PlayerController GetPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _ConnectedPlayer.Count)
            return null;
        PlayerController player = null;

        for (int i = 0; i < _ConnectedPlayer.Count; i++) {
            if (i == playerIndex) {
                player = _ConnectedPlayer[i];

                break;
            }
        }

        return player;
    }

    public List<CardType> GetAllFamilyPlayed()
    {
        List<CardType> cardTypes = new();

        foreach (PlayerController player in _ConnectedPlayer)
            cardTypes.Add(player.PlayerData.Data.Family);
        return cardTypes;
    }

    private void SetFamilyTypes()
    {
        CardType[] types = Constants.FamilyTypes;
        int playerCount = GetNumbersOfPlayer;
        List<int> selectedFamily = new();
        System.Random random = new();

        for (int i = 0; i < playerCount; i++) {
            while (selectedFamily.Count == i) {
                int randomIndex = random.Next(types.Length);

                if (!selectedFamily.Contains(randomIndex)) {
                    GetPlayer(i).PlayerData.Data.Family = types[randomIndex];

                    selectedFamily.Add(randomIndex);
                }
            }
        }
    }

    #endregion*/
}
