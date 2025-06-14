using System.Collections.Generic;
using OMGG.Network.Fusion;
using UnityEngine;
using Fusion;

namespace Vermines {
    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;
    using Vermines.ShopSystem.Data;

    public class GameDataStorage : NetworkBehaviour {

        #region Editor

        /// <summary>
        /// The player controller script links to the player prefab.
        /// </summary>
        public PlayerController PlayerPrefabs;

        #endregion

        #region Singleton

        public static GameDataStorage Instance => NetworkSingleton<GameDataStorage>.Instance;

        #endregion

        #region Player's Data

        [Networked, OnChangedRender(nameof(OnPlayerDataUpdated))]
        [Capacity(32)]
        [HideInInspector]
        public NetworkDictionary<PlayerRef, PlayerData> PlayerData { get; }

        public Dictionary<PlayerRef, PlayerDeck> PlayerDeck { get; set; } = new Dictionary<PlayerRef, PlayerDeck>();

        #endregion

        public void OnPlayerDataUpdated()
        {
            GameEvents.InvokeOnPlayerUpdated(PlayerData);

            // Check if a player has won the game

            foreach (var playerData in PlayerData)
            {
                if (PlayerData.TryGet(playerData.Key, out PlayerData data) == false)
                    continue;

                if (data.IsConnected == false)
                    continue;

                if (data.Souls >= GameManager.Instance.Config.MaxSoulsToWin.Value)
                {
                    Debug.Log($"[SERVER]: Player {data.Nickname} has won the game with {data.Souls} souls.");
                    GameEvents.InvokeOnPlayerWin(playerData.Key, Runner.LocalPlayer);
                    return; // Exit after finding the first winner
                }
            }
        }

        public void AddPlayer(PlayerRef player, string username, CardFamily family = CardFamily.None)
        {
            if (HasStateAuthority == false)
                return;
            if (PlayerData.TryGet(player, out PlayerData data) == false) {
                data = new PlayerData(player);

                PlayerData.Set(player, data);
            }

            data.PlayerRef = player;
            data.Nickname  = username;
            data.Family    = family;

            PlayerData.Set(player, data);
        }

        public List<CardFamily> GetPlayersFamily()
        {
            List<CardFamily> families = new();

            foreach (var playerData in PlayerData) {
                if (playerData.Value.Family != CardFamily.None)
                    families.Add(playerData.Value.Family);
            }

            return families;
        }

        #region Shop's Data

        public ShopData Shop { get; set; }

        #endregion

        #region Override Methods

        public override void Spawned()
        {
            PlayerDeck = new();
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false)
                return;
            PlayerManager<PlayerController>.UpdatePlayerConnections(Runner, SpawnPlayer, DespawnPlayer);
        }

        #endregion

        #region Player connection callbacks

        public void SpawnPlayer(PlayerRef playerRef)
        {
            if (HasStateAuthority == false)
                return;
            if (PlayerData.TryGet(playerRef, out PlayerData data) == false)
                data = new PlayerData(playerRef);
            if (data.IsConnected == true) // Already connected
                return;
            Debug.LogWarning($"[SERVER]: {playerRef} connected.");

            data.IsConnected = true;

            PlayerData.Set(playerRef, data);

            var player = Runner.Spawn(PlayerPrefabs, Vector3.zero, Quaternion.identity, playerRef);

            // Set player instance as PlayerObject so we can easily get it from other locations.
            Runner.SetPlayerObject(playerRef, player.Object);
        }

        public void DespawnPlayer(PlayerRef playerRef, PlayerController player)
        {
            if (HasStateAuthority == false)
                return;
            if (PlayerData.TryGet(playerRef, out PlayerData data) == true) {
                if (data.IsConnected == true)
                    Debug.LogWarning($"[SERVER]: {playerRef} disconnected.");
                data.IsConnected = false;

                PlayerData.Set(playerRef, data);
            }

            Runner.Despawn(player.Object);
        }

        #endregion

        #region Player's Data Methods

        public void SetEloquence(PlayerRef player, int eloquence)
        {
            if (HasStateAuthority == false)
                return;
            if (PlayerData.TryGet(player, out PlayerData data) == true) {
                int maxEloquence = GameManager.Instance.Config.MaxEloquence.Value;

                if (maxEloquence < eloquence)
                    eloquence = maxEloquence;
                data.Eloquence = eloquence;

                PlayerData.Set(player, data);
            }
        }

        public void SetSouls(PlayerRef player, int souls)
        {
            if (HasStateAuthority == false)
                return;
            if (PlayerData.TryGet(player, out PlayerData data) == true) {
                data.Souls = souls;

                PlayerData.Set(player, data);
            }
        }

        #endregion

        #region Serialization

        public string SerializeDeck(bool debugging = false)
        {
            string data = string.Empty;

            foreach (var player in PlayerDeck) {
                // Player reference
                data += player.Key.RawEncoded + ":";

                // Decks serializer
                data += player.Value.Serialize();

                // Decks separator
                data += "|";
            }

            if (debugging == true)
                Debug.LogWarning($"[SERVER]: Player's deck - {data}");
            return data;
        }

        public string SerializeShop(bool debugging = false)
        {
            string data = Shop.Serialize();

            if (debugging == true)
                Debug.LogWarning($"[SERVER]: Shop - {data}");
            return data;
        }

        #endregion
    }
}
