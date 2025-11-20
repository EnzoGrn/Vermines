using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.Core.Player;
using Vermines.Core.Scene;
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

        private Dictionary<int, PlayerStatistics> _players = new();
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
            GameEvents.OnPlayerUpdated.AddListener(UpdatePlayer);
        }

        /// <summary>
        /// Hides the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            GameEvents.OnTurnChanged.RemoveListener(NextTurn);
            GameEvents.OnPlayerUpdated.RemoveListener(UpdatePlayer);
        }

        public void Awake()
        {
            Init();
        }

        #endregion

        public void Init()
        {
            if (PlayerController.Local == null)
                return;
            _players.Clear();

            SceneContext context = PlayerController.Local.Context;

            List<PlayerController> players = context.Runner.GetAllBehaviours<PlayerController>();

            foreach (PlayerController player in players) {
                _players[player.Object.InputAuthority.PlayerId] = player.Statistics;

                CreateBanner(player);
            }
        }

        public void CreateBanner(PlayerController player)
        {
            if (_banners.Count >= 4) // TODO: Replace with a config value
            {
                Debug.LogWarning("Maximum number of _banners reached.");
                return;
            }
            var banner = Instantiate(playerBannerPrefab, bannerParent);
            banner.Initialize(player);
            _banners.Add(banner);
            banner.Show();
            banner.SetActive(_banners.Count == 1); // Only the first banner is active initially
            banner.onHideComplete.AddListener(() =>
            {

                banner.transform.SetAsLastSibling();

                banner.Show();

                for (int i = 0; i < _banners.Count; i++)
                {
                    _banners[i].SetActive(i == 0);
                }
            });
        }

        public void UpdateBanner(PlayerStatistics playerData, int playerId)
        {
            var banner = _banners.Find(b => b.GetPlayerId() == playerId);
            if (banner != null)
            {
                banner.UpdateStats(playerData);
            }
        }

        public void UpdateAllPlayers()
        {
            List<PlayerController> players = PlayerController.Local.Context.Runner.GetAllBehaviours<PlayerController>();

            foreach (PlayerController player in players)
                UpdatePlayer(player);
        }

        public void UpdatePlayer(PlayerController player)
        {
            int playerId = player.Object.InputAuthority.PlayerId;

            if (_players.ContainsKey(playerId)) {
                _players[playerId] = player.Statistics;

                UpdateBanner(player.Statistics, playerId);
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