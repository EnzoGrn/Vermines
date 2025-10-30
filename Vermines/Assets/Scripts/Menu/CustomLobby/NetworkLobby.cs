using System.Collections.Generic;
using Fusion.Sockets;
using Fusion;
using UnityEngine;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Extension;
    using Vermines.Core;

    public sealed class NetworkLobby : ContextBehaviour, IPlayerJoined, IPlayerLeft {

        #region Prefabs

        [SerializeField]
        private LobbyPlayerController _PlayerPrefab;

        [SerializeField]
        private LobbyManager _LobbyManagerPrefab;

        #endregion

        #region Attributes

        private PlayerRef _LocalPlayer;

        public List<LobbyPlayerController> ActivePlayers = new();
        private Dictionary<PlayerRef, LobbyPlayerController> _PendingPlayers = new();

        private List<LobbyPlayerController> _SpawnedPlayers = new(byte.MaxValue);

        private LobbyManager _LobbyManager;

        private bool _IsActive;

        #endregion

        #region Getters & Setters

        public LobbyPlayerController GetPlayer(PlayerRef playerRef)
        {
            if (!playerRef.IsRealPlayer || Object == null)
                return default;
            _SpawnedPlayers.Clear();

            Runner.GetAllBehaviours<LobbyPlayerController>(_SpawnedPlayers);

            foreach (LobbyPlayerController player in _SpawnedPlayers)
                if (player.Object.InputAuthority == playerRef)
                    return player;
            return default;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            if (HasStateAuthority)
                _LobbyManager = Runner.Spawn(_LobbyManagerPrefab);
            _LocalPlayer = Runner.LocalPlayer;

            ActivePlayers.Clear();
        }

        public void Activate()
        {
            _IsActive = true;

            foreach (PlayerRef playerRef in Runner.ActivePlayers)
                SpawnPlayer(playerRef);
        }

        private void SpawnPlayer(PlayerRef playerRef)
        {
            if (GetPlayer(playerRef) != null || _PendingPlayers.ContainsKey(playerRef)) {
                Log.Error($"Player for {playerRef} is already spawned!");

                return;
            }

            LobbyPlayerController player = Runner.Spawn(_PlayerPrefab, inputAuthority: playerRef);

            _PendingPlayers[playerRef] = player;

            #if UNITY_EDITOR
                player.gameObject.name = $"Player Unknown (Pending)";
            #endif
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

        private void OnPlayerLeft(LobbyPlayerController player)
        {
            if (player == null)
                return;
            ActivePlayers.Remove(player);

            Runner.Despawn(player.Object);
        }

        private void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason _)
        {
            if (runner != null)
                runner.SetLocalPlayer(_LocalPlayer);
        }

        #endregion
    }
}
