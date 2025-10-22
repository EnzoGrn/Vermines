using System.Collections.Generic;
using Fusion.Sockets;
using Fusion;
using WebSocketSharp;
using UnityEngine;

namespace Vermines.Core {

    using Vermines.Player;
    using Vermines.Extension;

    public sealed class NetworkGame : ContextBehaviour, IPlayerJoined, IPlayerLeft {

        #region Attributes

        private PlayerRef _LocalPlayer;

        public List<PlayerController> ActivePlayers = new();

        #region Prefabs

        [SerializeField]
        private PlayerController _PlayerPrefab;

        [SerializeField]
        private GameplayMode[] _ModePrefabs;

        #endregion

        private Dictionary<PlayerRef, PlayerController> _PendingPlayers = new();
        private Dictionary<string, PlayerController> _DisconnectedPlayers = new();

        private List<PlayerController> _SpawnedPlayers = new(byte.MaxValue);

        private GameplayMode _Gameplay;

        private bool _IsActive;

        #endregion

        #region Methods

        public void Initialize(GameplayType type)
        {
            if (HasStateAuthority) {
                var prefab = _ModePrefabs.Find(t => t.Type == type);

                _Gameplay = Runner.Spawn(prefab);
            }
        }

        public void Activate()
        {
            _IsActive = true;

            _Gameplay.Activate();

            foreach (PlayerRef playerRef in Runner.ActivePlayers)
                SpawnPlayer(playerRef);
        }

        private void SpawnPlayer(PlayerRef playerRef)
        {
            if (GetPlayer(playerRef) != null || _PendingPlayers.ContainsKey(playerRef)) {
                Log.Error($"Player for {playerRef} is already spawned!");

                return;
            }

            PlayerController player = Runner.Spawn(_PlayerPrefab, inputAuthority: playerRef);

            _PendingPlayers[playerRef] = player;

            #if UNITY_EDITOR
                player.gameObject.name = $"Player Unknown (Pending)";
            #endif
        }

        #endregion

        #region Getters & Setters

        public PlayerController GetPlayer(PlayerRef playerRef)
        {
            if (!playerRef.IsRealPlayer || Object == null)
                return default;
            _SpawnedPlayers.Clear();

            Runner.GetAllBehaviours<PlayerController>(_SpawnedPlayers);

            foreach (PlayerController player in _SpawnedPlayers)
                if (player.Object.InputAuthority == playerRef)
                    return player;
            return default;
        }

        #endregion

        #region Interface

        void IPlayerJoined.PlayerJoined(PlayerRef playerRef)
        {
            if (!Runner.IsServer || !_IsActive)
                return;
            SpawnPlayer(playerRef);
        }

        void IPlayerLeft.PlayerLeft(PlayerRef playerRef)
        {
            if (!playerRef.IsRealPlayer || !Runner.IsServer || !_IsActive)
                return;
            OnPlayerLeft(GetPlayer(playerRef));
        }

        #endregion

        #region Events

        private void OnPlayerLeft(PlayerController player)
        {
            if (player == null)
                return;
            ActivePlayers.Remove(player);

            if (!player.UserID.IsNullOrEmpty()) {
                _DisconnectedPlayers[player.UserID] = player;

                _Gameplay.PlayerLeft(player);

                player.Object.RemoveInputAuthority();

                #if UNITY_EDITOR
                    player.gameObject.name = $"{player.gameObject.name} (Disconnected)";
                #endif
            } else { // Player wasn't initialized properly, safe despawn.
                _Gameplay.PlayerLeft(player);

                Runner.Despawn(player.Object);
            }
        }

        private void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason _)
        {
            if (runner != null)
                runner.SetLocalPlayer(_LocalPlayer);
        }

        #endregion
    }
}
