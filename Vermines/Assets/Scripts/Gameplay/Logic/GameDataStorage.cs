using System.Collections.Generic;
using OMGG.Network.Fusion;
using UnityEngine;
using Fusion;

namespace Vermines {

    using Vermines.Player;

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

        [Networked]
        [Capacity(32)]
        [HideInInspector]
        public NetworkDictionary<PlayerRef, PlayerData> PlayerData { get; }

        public Dictionary<PlayerRef, PlayerDeck> PlayerDeck { get; set; }

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
            if (PlayerData.TryGet(playerRef, out PlayerData data) == true) {
                if (data.IsConnected == true)
                    Debug.LogWarning($"[SERVER]: {playerRef} disconnected.");
                data.IsConnected = false;

                PlayerData.Set(playerRef, data);
            }

            Runner.Despawn(player.Object);
        }

        #endregion
    }
}
