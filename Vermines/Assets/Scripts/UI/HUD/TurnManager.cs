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

        [SerializeField] private PlayerBannerManager bannerManager;

        private int currentPlayerIndex = 0;
        private Dictionary<int, PlayerData> players = new();

        private void Awake()
        {
            Instance = this;
        }

        public void Init(NetworkDictionary<PlayerRef, PlayerData> playerData)
        {
            players.Clear();
            foreach (var player in playerData)
            {
                players[player.Value.PlayerRef.PlayerId] = new PlayerData(player.Value.PlayerRef);
            }
            foreach (var player in players)
            {
                bannerManager.CreateBanner(player.Value, player.Key);
            }
        }

        public void UpdateAllPlayers(NetworkDictionary<PlayerRef, PlayerData> playerData)
        {
            foreach (var player in playerData)
            {
                if (players.ContainsKey(player.Value.PlayerRef.PlayerId))
                {
                    players[player.Value.PlayerRef.PlayerId] = player.Value;
                    bannerManager.UpdateBanner(player.Value, player.Key.PlayerId);
                }
            }
        }

        public void UpdatePlayer(PlayerData playerData)
        {
            if (players.ContainsKey(playerData.PlayerRef.PlayerId))
            {
                players[playerData.PlayerRef.PlayerId] = playerData;
                bannerManager.UpdateBanner(playerData, playerData.PlayerRef.PlayerId);
            }
        }

        public void AttemptToNextPhase()
        {
            GameEvents.OnAttemptNextPhase?.Invoke();
        }

        public void NextTurn()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

            bannerManager.NextTurn();

            Debug.Log(GetCurrentPlayer().Nickname + "'s turn");
        }

        public PlayerData GetCurrentPlayer() => players[currentPlayerIndex];
    }
}