using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines;

public class GameManager : MonoBehaviourPunCallbacks {

    static public GameManager Instance;

    private List<Vermines.Player> _ConnectedPlayer = new();

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        _ConnectedPlayer = new();

        /*if (Constants.PlayGround == null)
            Instantiate(Resources.Load<GameObject>(Constants.PlayGroundPref));
        AddNewPlayerToList(PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);*/
    }

    private void AddNewPlayerToList(string nickname, int playerID)
    {
        GameObject playerObj = PhotonNetwork.Instantiate(Constants.PlayerPref, Vector3.zero, Quaternion.identity);

        Vermines.Player player = playerObj.GetComponent<Vermines.Player>();

        Debug.Assert(player != null, "Player not found");

        Config.PlayerProfile profile = new() {
            Nickname = nickname,
            PlayerID = playerID
        };

        player.SetProfile(profile);

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

    public List<CardType> GetAllFamilyPlayed()
    {
        List<CardType> cardTypes = new();

        foreach (Vermines.Player player in _ConnectedPlayer)
            cardTypes.Add(player.FamilyTypes);
        return cardTypes;
    }

    private void SetFamilyTypes()
    {
        CardType[] types     = Constants.FamilyTypes;
        int playerCount      = GetNumbersOfPlayer;
        List<int> selectedFamily = new();
        System.Random random     = new();

        for (int i = 0; i < playerCount; i++) {
            while (selectedFamily.Count == i) {
                int randomIndex = random.Next(types.Length);

                if (!selectedFamily.Contains(randomIndex)) {
                    Vermines.Player player = GetPlayer(i);

                    if (player != null)
                        player.FamilyTypes = types[randomIndex];
                    selectedFamily.Add(randomIndex);
                }
            }
        }
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
