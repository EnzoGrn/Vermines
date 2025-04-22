using Fusion;
using TMPro;
using UnityEngine;
using Vermines.Player;

namespace Vermines.UI
{
    public class PlayerBannerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI eloquenceText;
        [SerializeField] private TextMeshProUGUI soulsText;
        [SerializeField] private RectTransform root;

        [Header("Scale Settings")]
        [SerializeField] private float normalScale = 1.1f;
        [SerializeField] private float activeScale = 1.275f;

        private int _playerId;

        public void Initialize(PlayerData playerData, int playerId)
        {
            _playerId = playerId;
            nicknameText.text = playerData.Nickname;
            UpdateStats(playerData);
        }

        public void UpdateStats(PlayerData playerData)
        {
            eloquenceText.text = playerData.Eloquence.ToString();
            soulsText.text = playerData.Souls.ToString();
        }

        public void SetActive(bool isActive)
        {
            root.localScale = Vector3.one * (isActive ? activeScale : normalScale);
        }

        public int GetPlayerId() => _playerId;
    }
}
