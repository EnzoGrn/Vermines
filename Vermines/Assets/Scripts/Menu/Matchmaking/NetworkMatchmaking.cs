using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Menu.Matchmaking {

    using Vermines.Core;

    public class NetworkMatchmaking : ContextBehaviour, IPlayerJoined, IPlayerLeft {

        #region Attributes

        [SerializeField, Tooltip("Minimum number of players required to launch the game.")]
        private int _MinPlayersToStart = 2;

        [SerializeField, Tooltip("Delay in seconds before starting the game once enough players are present.")]
        private float _StartDelay = 5f;

        [SerializeField, Tooltip("Maximum wait time in seconds before leaving if no players joined.")]
        private float _TimeoutDelay = 30f;

        private readonly List<PlayerRef> _ActivePlayers = new(byte.MaxValue);

        private bool _IsGameStarting;
        private bool _IsActive;
        private float _StartTimer;
        private float _TimeoutTimer;

        #endregion

        #region Methods

        public void Initialize()
        {
            _ActivePlayers.Clear();

            _IsActive       = false;
            _IsGameStarting = false;
            _StartTimer     = 0f;
            _TimeoutTimer   = 0f;

            Log.Info("[Matchmaking] Initialized and waiting for players...");
        }

        public void Activate()
        {
            _IsActive     = true;
            _TimeoutTimer = 0f;
        }

        public void LeaveGame()
        {
            Global.Networking.StopGame();
        }

        public override void FixedUpdateNetwork()
        {
            if (!_IsActive || Runner == null)
                return;
            if (!_IsGameStarting && Runner.IsServer && _ActivePlayers.Count >= _MinPlayersToStart) {
                _StartTimer += Runner.DeltaTime;

                if (_StartTimer >= _StartDelay)
                    StartGame();
            }
            else
                _StartTimer = 0f;

            if (_ActivePlayers.Count <= 1 && !_IsGameStarting) {
                _TimeoutTimer += Runner.DeltaTime;


                if (_TimeoutTimer >= _TimeoutDelay)
                    LeaveGame();
            }
            else
                _TimeoutTimer = 0f;
        }

        private void StartGame()
        {
            _IsGameStarting = true;

            Log.Info($"[Matchmaking] Enough players ({_ActivePlayers.Count}), starting game...");

            RPC_ShowLoadingScreen();

            Context.SceneChangeController.RPC_RequestSceneChange(Context.GameScenePath, false, true, GameplayType.Standart, Context.MatchmakingScenePath, _ActivePlayers.Count);
        }

        #endregion

        #region Event

        public void PlayerJoined(PlayerRef player)
        {
            if (!_IsActive)
                return;
            if (!_ActivePlayers.Contains(player))
                _ActivePlayers.Add(player);
            Log.Info($"[Matchmaking] Player joined: {player} (Total: {_ActivePlayers.Count})");

            _TimeoutTimer = 0f;
            _StartTimer   = 0f;
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (!_IsActive)
                return;
            if (_ActivePlayers.Contains(player))
                _ActivePlayers.Remove(player);
            Log.Warn($"[Matchmaking] Player left: {player} (Remaining: {_ActivePlayers.Count})");

            if (_ActivePlayers.Count < _MinPlayersToStart)
                _StartTimer = 0f;
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowLoadingScreen()
        {
            StartCoroutine(Global.Networking.ShowLoadingSceneCoroutine(true));
        }

        #endregion
    }
}
