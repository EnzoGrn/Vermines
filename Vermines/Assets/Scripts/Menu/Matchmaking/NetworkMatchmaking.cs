using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines.Core;
using Vermines.Core.Network;
using Vermines.Extension;

namespace Vermines.Menu.Matchmaking {

    public class NetworkMatchmaking : ContextBehaviour, IPlayerJoined, IPlayerLeft {

        #region Attributes

        [SerializeField, Tooltip("Minimum number of players required to launch the game.")]
        private int _MinPlayersToStart = 2;

        private readonly List<PlayerRef> _ActivePlayers = new(byte.MaxValue);

        private bool _IsGameStarting;

        private bool _IsActive;

        #endregion

        #region Methods

        public void Initialize()
        {
            _ActivePlayers.Clear();

            _IsActive       = false;
            _IsGameStarting = false;

            Log.Info("[Matchmaking] Initialized and waiting for players...");
        }

        public void Activate()
        {
            _IsActive = true;
        }

        public void LeaveGame()
        {
            Global.Networking.StopGame();
        }

        public override void FixedUpdateNetwork()
        {
            if (!_IsActive || Runner == null)
                return;
            if (!_IsGameStarting && Runner.IsServer && _ActivePlayers.Count >= _MinPlayersToStart)
                StartGame();
        }

        private void StartGame()
        {
            _IsGameStarting = true;

            Log.Info($"[Matchmaking] Enough players ({_ActivePlayers.Count}), starting game...");

            // TODO: Launch the game by requesting the networking.
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
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (!_IsActive)
                return;
            if (_ActivePlayers.Contains(player))
                _ActivePlayers.Remove(player);
            Log.Warn($"[Matchmaking] Player left: {player} (Remaining: {_ActivePlayers.Count})");
        }

        #endregion
    }
}
