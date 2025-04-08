using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;

namespace Vermines.UI
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        [SerializeField] private PlayerBannerManager _bannerManager;

        private int _currentPlayerIndex = 0;
        private List<PlayerData> _orderedPlayers = new();
        private Dictionary<int, PlayerData> _players = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            GameEvents.OnTurnChanged.AddListener(NextTurn);
        }

        public void Init(NetworkDictionary<PlayerRef, PlayerData> playerData)
        {
            _orderedPlayers.Clear();
            _players.Clear();

            foreach (var player in playerData)
            {
                _players[player.Value.PlayerRef.PlayerId] = player.Value;
                _orderedPlayers.Add(player.Value);
            }
            foreach (var player in _players)
            {
                _bannerManager.CreateBanner(player.Value, player.Key);
            }
        }

        public void UpdateAllPlayers(NetworkDictionary<PlayerRef, PlayerData> playerData)
        {
            foreach (var player in playerData)
            {
                if (_players.ContainsKey(player.Value.PlayerRef.PlayerId))
                {
                    _players[player.Value.PlayerRef.PlayerId] = player.Value;
                    _bannerManager.UpdateBanner(player.Value, player.Key.PlayerId);
                }
            }
        }

        public void UpdatePlayer(PlayerData playerData)
        {
            if (_players.ContainsKey(playerData.PlayerRef.PlayerId))
            {
                _players[playerData.PlayerRef.PlayerId] = playerData;
                _bannerManager.UpdateBanner(playerData, playerData.PlayerRef.PlayerId);
            }
        }

        public void AttemptToNextPhase()
        {
            GameEvents.OnAttemptNextPhase?.Invoke();
        }

        public void NextTurn(int currentPlayerIndex)
        {
            _currentPlayerIndex = currentPlayerIndex;

            _bannerManager.NextTurn();

            Debug.Log($"[TurnManager] Next turn for player {GetCurrentPlayer().Nickname} ({GetCurrentPlayer().PlayerRef.PlayerId})");
        }

        public PlayerData GetCurrentPlayer() => _orderedPlayers[_currentPlayerIndex];
    }
}