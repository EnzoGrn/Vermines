using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.Player;

namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Manages the display of player banners in the gameplay screen.
    /// </summary>
    public class PlayerBannerPlugin : GameplayScreenPlugin
    {
        /// <summary>
        /// The player banner prefab.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected PlayerBannerUI playerBannerPrefab;

        /// <summary>
        /// The parent transform for the banners.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Transform bannerParent;

        private Dictionary<int, PlayerData> _players = new();
        private readonly List<PlayerBannerUI> _banners = new();

        #region Override Methods

        /// <summary>
        /// Shows the plugin.
        /// </summary>
        /// <param name="screen">The parent screen that this plugin is attached to.</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);

            GameEvents.OnTurnChanged.AddListener(NextTurn);
            GameEvents.OnPlayerInitialized.AddListener(Init);
            GameEvents.OnPlayerUpdated.AddListener(UpdatePlayer);
        }

        /// <summary>
        /// Hides the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            GameEvents.OnTurnChanged.RemoveListener(NextTurn);
            GameEvents.OnPlayerInitialized.RemoveListener(Init);
            GameEvents.OnPlayerUpdated.RemoveListener(UpdatePlayer);
        }

        #endregion

        public void Init()
        {
            _players.Clear();

            foreach (var player in GameManager.Instance.PlayerTurnOrder)
            {
                if (GameDataStorage.Instance.PlayerData.TryGet(player, out PlayerData playerData) == false)
                {
                    continue;
                }

                _players[player.PlayerId] = playerData;
            }
            foreach (var player in _players)
            {
                CreateBanner(player.Value, player.Key);
            }
        }

        public void CreateBanner(PlayerData playerData, int playerId)
        {
            if (_banners.Count >= 4) // TODO: Replace with a config value
            {
                Debug.LogWarning("Maximum number of _banners reached.");
                return;
            }
            var banner = Instantiate(playerBannerPrefab, bannerParent);
            banner.Initialize(playerData, playerId);
            _banners.Add(banner);
            banner.Show();
            banner.SetActive(_banners.Count == 1); // Only the first banner is active initially
            banner.onHideComplete.AddListener(() =>
            {

                // Réorganisation
                banner.transform.SetAsLastSibling();

                // Rejoue Show
                banner.Show();

                // Applique l’état actif uniquement au nouveau premier
                for (int i = 0; i < _banners.Count; i++)
                {
                    _banners[i].SetActive(i == 0);
                }
            });
        }

        public void UpdateBanner(PlayerData playerData, int playerId)
        {
            var banner = _banners.Find(b => b.GetPlayerId() == playerId);
            if (banner != null)
            {
                banner.UpdateStats(playerData);
            }
        }

        public void UpdateAllPlayers()
        {
            foreach (var player in GameDataStorage.Instance.PlayerData)
            {
                if (_players.ContainsKey(player.Value.PlayerRef.PlayerId))
                {
                    _players[player.Value.PlayerRef.PlayerId] = player.Value;
                    UpdateBanner(player.Value, player.Key.PlayerId);
                }
            }
        }

        public void UpdatePlayer(PlayerData playerData)
        {
            if (_players.ContainsKey(playerData.PlayerRef.PlayerId))
            {
                _players[playerData.PlayerRef.PlayerId] = playerData;
                UpdateBanner(playerData, playerData.PlayerRef.PlayerId);
            }
        }

        public void NextTurn(int currentPlayerIndex)
        {
            if (_banners.Count == 0) return;

            var current = _banners[0];
            _banners.RemoveAt(0);
            _banners.Add(current);

            AnimateBannerExitAndReorder(current);
        }

        private void AnimateBannerExitAndReorder(PlayerBannerUI banner)
        {
            banner.Hide();
        }
    }
}