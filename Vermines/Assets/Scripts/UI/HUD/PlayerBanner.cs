using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vermines.HUD
{
    public class PlayerBanner : MonoBehaviour
    {
        public GameObject playerNameObject;
        public TextMeshProUGUI playerEloquenceText;
        public TextMeshProUGUI playerSoulsText;
        public RectTransform rectTransform;
        public float scale = 1.1f;
        public float activeScale = 1.275f;

        public void Setup(PlayerData player)
        {
            playerNameObject.GetComponent<TextMeshProUGUI>().text = player.Nickname;
            playerEloquenceText.text = player.Eloquence.ToString();
            playerSoulsText.text = player.Souls.ToString();
            name = player.Nickname;
        }

        public void SetSize(bool isActive)
        {
            rectTransform.localScale = isActive ? Vector3.one * activeScale : Vector3.one * scale;
        }

        public void UpdateData(PlayerData playerData)
        {
            playerEloquenceText.text = playerData.Eloquence.ToString();
            playerSoulsText.text = playerData.Souls.ToString();
        }
    }
}