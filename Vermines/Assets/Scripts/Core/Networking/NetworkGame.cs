using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using Fusion.Sockets;
using Fusion;
using UnityEngine;

namespace Vermines.Core {

    using Vermines.Player;
    using Vermines.Extension;
    using Vermines.Core.Network;
    using Vermines.Utils;

    public partial class NetworkGame : ContextBehaviour, IPlayerJoined, IPlayerLeft {

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
        private List<PlayerController> _AllPlayers     = new(byte.MaxValue);

        // Note: Only host have this value instanciate.
        private GameplayMode _Gameplay;

        private bool _IsActive;

        private FusionCallbacksHandler _FusionCallbacks = new();

        [Networked, OnChangedRender(nameof(OnSeedChanged))]
        public int Seed { get; set; }

        public System.Random Random { get; set; }

        #endregion

        #region Methods

        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);

            if (HasStateAuthority)
                Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Random = new System.Random(Seed);
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner == null)
                return;
            _AllPlayers.Clear();

            Runner.GetAllBehaviours<PlayerController>(_AllPlayers);

            foreach (PlayerController player in _AllPlayers) {
                PlayerRef input = player.Object.InputAuthority;

                if (input.IsRealPlayer) {
                    if (HasStateAuthority && !Runner.IsPlayerValid(input)) {
                        _AllPlayers.Remove(player);

                        OnPlayerLeft(player);
                    }
                } else {
                    _AllPlayers.Remove(player);
                }
            }

            ActivePlayers.Clear();

            foreach (PlayerController player in _AllPlayers) {
                if (player.UserID.IsNullOrEmpty())
                    continue;
                ActivePlayers.Add(player);
            }

            if (!HasStateAuthority || _PendingPlayers.Count == 0)
                return;
            List<PlayerRef> playersToRemove = ListPool.Get<PlayerRef>(128);

            foreach (var kvp in _PendingPlayers) {
                PlayerRef     playerRef = kvp.Key;
                PlayerController player = kvp.Value;

                if (!player.IsInitialized)
                    continue;
                playersToRemove.Remove(playerRef);

                if (_DisconnectedPlayers.TryGetValue(player.UserID, out PlayerController disconnectedPlayer)) {
                    _DisconnectedPlayers.Remove(player.UserID);

                    int activePlayerIndex = ActivePlayers.IndexOf(player);

                    if (activePlayerIndex >= 0)
                        ActivePlayers[activePlayerIndex] = disconnectedPlayer;
                    disconnectedPlayer.OnReconnect(player);

                    player.Object.RemoveInputAuthority();
                    Runner.Despawn(player.Object);

                    player = disconnectedPlayer;

                    player.Object.AssignInputAuthority(playerRef);
                }

                player.Refresh();
                Runner.SetPlayerObject(playerRef, player.Object);

                #if UNITY_EDITOR
                    player.gameObject.name = $"Player {player.Nickname}";
                #endif

                _Gameplay.PlayerJoined(player);
            }

            foreach (PlayerRef playerToRemove in playersToRemove)
                _PendingPlayers.Remove(playerToRemove);
            ListPool.Return(playersToRemove);
        }

        public void Initialize(GameplayType type, string data = null)
        {
            if (HasStateAuthority) {
                var prefab = _ModePrefabs.Find(t => t.Type == type);

                _Gameplay = Runner.Spawn(prefab);

                if (data != null && data != "")
                    _Gameplay.Initialize(data);
                ResyncExistingPlayers();
            }

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

            if (!HasStateAuthority)
                return;
            ResyncExistingPlayers();
            
            // _Gameplay.Activate();

            // foreach (PlayerRef playerRef in Runner.ActivePlayers)
            //     SpawnPlayer(playerRef);
        }

        private void SpawnPlayer(PlayerRef playerRef)
        {
            if (GetPlayer(playerRef) != null || _PendingPlayers.ContainsKey(playerRef)) {
                Log.Error($"Player for {playerRef} is already spawned!");

                return;
            }

            PlayerController player = Runner.Spawn(_PlayerPrefab, inputAuthority: playerRef);

            #if UNITY_EDITOR
                player.gameObject.name = "$Player Unknown (Pending)";
            #endif
        }

        private void ResyncExistingPlayers()
        {
            if (Runner == null)
                return;
            _SpawnedPlayers.Clear();

            Runner.GetAllBehaviours<PlayerController>(_SpawnedPlayers);

            Dictionary<PlayerRef, PlayerController> existing = new();

            foreach (PlayerController player in _SpawnedPlayers) {
                if (player.Object == null)
                    continue;
                PlayerRef input = player.Object.InputAuthority;

                if (input.IsRealPlayer)
                    existing[input] = player;
            }

            foreach (PlayerRef playerRef in Runner.ActivePlayers) {
                if (!playerRef.IsRealPlayer)
                    continue;
                if (existing.TryGetValue(playerRef, out PlayerController playerController)) {
                    if (Runner.GetPlayerObject(playerRef) == null)
                        Runner.SetPlayerObject(playerRef, playerController.Object);
                    if (!ActivePlayers.Contains(playerController))
                        ActivePlayers.Add(playerController);
                    playerController.Refresh();

                    #if UNITY_EDITOR
                        playerController.gameObject.name = $"Player {playerController.Nickname}";
                    #endif

                    _Gameplay.PlayerJoined(playerController);

                    continue;
                }

                SpawnPlayer(playerRef);
            }

            foreach (PlayerController player in _SpawnedPlayers) {
                PlayerRef input = player.Object.InputAuthority;

                if (!Runner.ActivePlayers.Contains(input))
                    Runner.Despawn(player.Object);
            }
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

        public bool IsCustomGame()
        {
            GamePeer peer = Global.Networking?.GetPeer(Runner);

            if (peer.Request.IsCustom)
                return true;
            return false;
        }

        public List<PlayerController> GetConnectedPlayer()
        {
            if (Runner.IsServer)
                return ActivePlayers;
            List<PlayerController> players          = Context.Runner.GetAllBehaviours<PlayerController>();
            List<PlayerController> connectedPlayers = new();

            foreach (PlayerController player in players) {
                if (player.Statistics.IsConnected)
                    connectedPlayers.Add(player);
            }

            return connectedPlayers;
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

        private void OnSeedChanged()
        {
            Random = new System.Random(Seed);
        }

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
