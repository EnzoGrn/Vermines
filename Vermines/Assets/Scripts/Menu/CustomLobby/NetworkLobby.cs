using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Fusion.Sockets;
using Fusion;
using UnityEngine;
using WebSocketSharp;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Extension;
    using Vermines.Core;
    using Vermines.Core.Network;
    using Vermines.Utils;

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
        private List<LobbyPlayerController> _AllPlayers     = new(byte.MaxValue);

        private LobbyManager _LobbyManager;

        private bool _IsActive;

        #endregion

        private FusionCallbacksHandler _FusionCallbacks = new();

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

            _FusionCallbacks.DisconnectedFromServer -= OnDisconnectedFromServer;
            _FusionCallbacks.DisconnectedFromServer += OnDisconnectedFromServer;

            Runner.RemoveCallbacks(_FusionCallbacks);
            Runner.AddCallbacks(_FusionCallbacks);

            ActivePlayers.Clear();
        }

        public void Activate()
        {
            _IsActive = true;

            foreach (PlayerRef player in Runner.ActivePlayers) {
                LobbyPlayerController controller = GetPlayer(player);

                if (controller != null)
                    controller.UpdateContext(Context);
            }
        }

        private void SpawnPlayer(PlayerRef playerRef, [CallerMemberName] string caller = "")
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

        public override void FixedUpdateNetwork()
        {
            if (Runner == null)
                return;
            _AllPlayers.Clear();

            Runner.GetAllBehaviours<LobbyPlayerController>(_AllPlayers);

            foreach (LobbyPlayerController player in _AllPlayers) {
                PlayerRef inputAuthority = player.Object.InputAuthority;

                if (inputAuthority.IsRealPlayer) {
                    if (HasInputAuthority && !Runner.IsPlayerValid(inputAuthority)) {
                        _AllPlayers.Remove(player);

                        OnPlayerLeft(player);
                    }
                } else {
                    _AllPlayers.Remove(player);
                }
            }

            ActivePlayers.Clear();

            foreach (LobbyPlayerController player in _AllPlayers) {
                if (player.UserID.IsNullOrEmpty())
                    continue;
                ActivePlayers.Add(player);
            }

            if (!HasStateAuthority || _PendingPlayers.Count == 0)
                return;
            var playersToRemove = ListPool.Get<PlayerRef>(128);

            foreach (var playerPair in _PendingPlayers) {
                var playerRef = playerPair.Key;
                var player    = playerPair.Value;

                if (!player.IsInitialized)
                    continue;
                playersToRemove.Add(playerRef);

                player.Refresh();

                Runner.SetPlayerObject(playerRef, player.Object);

                #if UNITY_EDITOR
                    player.gameObject.name = $"Player {player.Nickname}";
                #endif

                _LobbyManager.PlayerJoined(player);
            }

            for (int i = 0; i < playersToRemove.Count; i++)
                _PendingPlayers.Remove(playersToRemove[i]);
            ListPool.Return(playersToRemove);
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

            LobbyUIView view = Context.UI.Get<LobbyUIView>();

            if (view != null)
                view.OnPlayerStatesChanged();
        }

        private void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason _)
        {
            if (runner != null)
                runner.SetLocalPlayer(_LocalPlayer);
        }

        #endregion
    }
}
