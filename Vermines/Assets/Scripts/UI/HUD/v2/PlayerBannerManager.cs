using Fusion;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Vermines.Player;

namespace Vermines.UI
{
    public class PlayerBannerManager : MonoBehaviour
    {
        [SerializeField] private PlayerBannerUI playerBannerPrefab;
        [SerializeField] private Transform bannerParent;
        [SerializeField] private float spacing = 100f;
        [SerializeField] private float transitionDuration = 0.4f;

        private readonly List<PlayerBannerUI> _banners = new();

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

        public void NextTurn()
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
