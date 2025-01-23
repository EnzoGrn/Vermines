using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vermines.HUD
{
    public class PlayerBanner : MonoBehaviour
    {
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI playerEloquenceText;
        public TextMeshProUGUI playerSoulsText;
        public RectTransform rectTransform;

        public void Setup(Player player)
        {
            playerNameText.text = player.Nickname;
            playerEloquenceText.text = player.Eloquence.ToString();
            playerSoulsText.text = player.Souls.ToString();
            name = player.Nickname;
        }

        public void SetSize(bool isActive)
        {
            rectTransform.localScale = isActive ? Vector3.one * 1.5f : Vector3.one * 1.2f;
        }
    }
}