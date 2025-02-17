using UnityEngine;
using UnityEngine.UI;

namespace Vermines.HUD
{
    using Vermines.HUD.Card;
    
    public class Shop : MonoBehaviour
    {
        [SerializeField] private GameObject cardBuyOverlay;

        [Header("Sprite")]
        [SerializeField] private Sprite spriteCharacterLeft;
        [SerializeField] private Sprite spriteCharacterRight;

        [SerializeField] private GameObject CharacterLeft;
        [SerializeField] private bool flipCharacterLeft = false;
        [SerializeField] private GameObject CharacterRight;
        [SerializeField] private bool flipCharacterRight = false;

        // TODO: Add the Localization for the shop name

        private void Start()
        {
            if (cardBuyOverlay != null)
            {
                cardBuyOverlay.SetActive(false);
            }
            else
            {
                Debug.LogError("CardBuyOverlay GameObject not found.");
            }
            if (spriteCharacterLeft != null)
            {
                Image image = CharacterLeft.GetComponent<Image>();
                if (image == null)
                {
                    Debug.LogError("Image component not found on CharacterLeft GameObject. Check the children or add the Image component to the GameObject.");
                    return;
                }
                image.sprite = spriteCharacterLeft;
                image.SetNativeSize();
                if (flipCharacterLeft)
                {
                    image.transform.localScale = new Vector3(-5, 5, 5);
                }
            } else
            {
                CharacterLeft.SetActive(false);
            }
            if (spriteCharacterRight != null) {
                Image image = CharacterRight.GetComponent<Image>();
                if (image == null)
                {
                    Debug.LogError("Image component not found on CharacterRight GameObject. Check the children or add the Image component to the GameObject.");
                    return;
                }
                image.sprite = spriteCharacterRight;
                image.SetNativeSize();
                if (flipCharacterRight)
                {
                    image.transform.localScale = new Vector3(-5, 5, 5);
                }
            }
            else
            {
                CharacterRight.SetActive(false);
            }
        }

        public void OpenCardBuyOverlay(CardBase card)
        {
            cardBuyOverlay.GetComponent<CardBuyBanner>().Setup(card.GetCardData());
            cardBuyOverlay.SetActive(true);
        }

        public void CloseCardBuyOverlay()
        {
            cardBuyOverlay.SetActive(false);
        }

    }
}