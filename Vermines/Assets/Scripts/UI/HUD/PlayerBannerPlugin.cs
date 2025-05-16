using DG.Tweening;
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;
using Vermines.UI;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;

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

        /// <summary>
        /// The spacing between banners.
        /// </summary>
        [InlineHelp, SerializeField]
        protected float spacing = 100f;

        /// <summary>
        /// The duration of the DO tween transition.
        /// </summary>
        [InlineHelp, SerializeField]
        protected float transitionDuration = 0.4f;

        private Dictionary<int, PlayerData> _players = new();
        private readonly List<PlayerBannerUI> _banners = new();

        /// <summary>
        /// The parent screen is shown.
        /// Cache the connection object.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);

            GameEvents.OnTurnChanged.AddListener(NextTurn);
            GameEvents.OnPlayerInitialized.AddListener(Init);
            GameEvents.OnPlayerUpdated.AddListener(UpdatePlayer);
        }

        /// <summary>
        /// The parent screen is hidden. Clear the connection object.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public override void Hide(GameplayUIScreen screen)
        {
            base.Hide(screen);

            GameEvents.OnTurnChanged.RemoveListener(NextTurn);
            GameEvents.OnPlayerInitialized.RemoveListener(Init);
            GameEvents.OnPlayerUpdated.RemoveListener(UpdatePlayer);
        }

        public void Init()
        {
            _players.Clear();

            foreach (var player in GameManager.Instance.PlayerTurnOrder)
            {
                if (GameDataStorage.Instance.PlayerData.TryGet(player, out PlayerData playerData) == false)
                {
                    continue;
                }
                Debug.Log($"[UI]: Player {player} found in the PlayerData.");

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
            RepositionBanners();
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
            Sequence seq = DOTween.Sequence();

            // Leave on the left
            seq.Append(banner.transform.DOLocalMoveX(-300f, transitionDuration / 2).SetEase(Ease.InBack));
            // Disparition
            seq.Join(banner.transform.DOScale(0.8f, transitionDuration / 2));
            seq.AppendCallback(() =>
            {
                // Replace the banner at the end of the list
                banner.transform.SetAsLastSibling();
                RepositionBanners();
            });
        }

        private void RepositionBanners()
        {
            for (int i = 0; i < _banners.Count; i++)
            {
                var banner = _banners[i];
                float yPos = -i * spacing;
                banner.transform.DOLocalMoveY(yPos, transitionDuration).SetEase(Ease.OutQuad);
                banner.SetActive(i == 0);
            }
        }
    }
}