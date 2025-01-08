using Fusion;

namespace Vermines {
    using UnityEngine;
    using Vermines.Player;

    public class GameManager : NetworkBehaviour {

        #region Editor

        /// <summary>
        /// The player controller script links to the player prefab.
        /// </summary>
        public PlayerController PlayerPrefabs;

        #endregion

        [Networked]
        [Capacity(32)]
        [HideInInspector]
        public NetworkDictionary<PlayerRef, PlayerData> PlayerData { get; }


        #region Override Methods

        public override void Spawned()
        {
            if (Runner.Mode == SimulationModes.Server)
                Application.targetFrameRate = TickRate.Resolve(Runner.Config.Simulation.TickRateSelection).Server;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false)
                return;
            PlayerManager<PlayerController>.UpdatePlayerConnections(Runner, SpawnPlayer, DespawnPlayer);
        }

        #endregion

        #region Callbacks

        private void SpawnPlayer(PlayerRef playerRef)
        {
            if (PlayerData.TryGet(playerRef, out PlayerData data) == false) {
                data = new PlayerData() {
                    Nickname    = playerRef.ToString(),
                    PlayerRef   = playerRef,
                    IsConnected = false
                };
            }

            if (data.IsConnected == true) // Already connected
                return;
            Debug.LogWarning($"{playerRef} connected.");

            data.IsConnected = true;

            PlayerData.Set(playerRef, data);

            var player = Runner.Spawn(PlayerPrefabs, Vector3.zero, Quaternion.identity, playerRef);

            // Set player instance as PlayerObject so we can easily get it from other locations.
            Runner.SetPlayerObject(playerRef, player.Object);
        }

        private void DespawnPlayer(PlayerRef playerRef, PlayerController player)
        {
            if (PlayerData.TryGet(playerRef, out PlayerData data) == true) {
                if (data.IsConnected == true)
                    Debug.LogWarning($"{playerRef} disconnected.");
                data.IsConnected = false;

                PlayerData.Set(playerRef, data);
            }

            Runner.Despawn(player.Object);
        }

        #endregion
    }
}
