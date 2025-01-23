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

        private void Start()
        {
            if (playerNameObject != null)
            {
                // get playerNameText parent
                playerNameObject.SetActive(false);
            }
        }

        public void ShowPlayerName()
        {
            if (playerNameObject != null)
            {
                playerNameObject.SetActive(true);
            }
        }

        public void HidePlayerName()
        {
            if (playerNameObject != null)
            {
                playerNameObject.SetActive(false);
            }
        }

        public void Setup(Player player)
        {
            //playerNameText.text = player.Nickname;
            playerNameObject.GetComponent<TextMeshProUGUI>().text = player.Nickname;
            playerEloquenceText.text = player.Eloquence.ToString();
            playerSoulsText.text = player.Souls.ToString();
            name = player.Nickname;
        }

        public void SetSize(bool isActive)
        {
            rectTransform.localScale = isActive ? Vector3.one * 1.275f : Vector3.one * 1.1f;
        }
    }
}