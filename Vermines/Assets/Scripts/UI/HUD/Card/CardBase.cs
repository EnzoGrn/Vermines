using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Vermines.HUD.Card
{
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;

    public class CardBase : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI eloquence;
        [SerializeField] private TextMeshProUGUI souls;
        [SerializeField] private TextMeshProUGUI effectDescription;
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject background;

        // TODO: adapt card if it's a partisan or object
        // TODO: add the card type (partisan, object, etc.)
        // TODO: add the card level
        // TODO: add effect

        [SerializeField]
        [Tooltip("Fill this only in debug mode. Otherwise, it will be filled by the HandManager.")]
        public ICard Card;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private void Start()
        {
            //if (debugMode)
            //{
            //    Setup(cardData);
            //}
        }

        public void Setup(ICard newCard)
        {
            if (newCard == null)
            {
                Debug.LogError("Card data is null!");
                return;
            }
            Card = newCard;
            UpdateUI();
        }

        public ICard GetCard()
        {
            return Card;
        }

        /// <summary>
        /// Update the UI with the card data.
        /// </summary>
        private void UpdateUI()
        {
            cardName.text = Card.Data.Name;
            eloquence.text = Card.Data.Name.ToString();
            souls.text = Card.Data.Name.ToString();
            //effectDescription.text = Card.Effects[0].Description;
            switch (Card.Data.Type)
            {
                case CardType.Partisan:
                    LoadFromString(Card.Data.Name, Card.Data.Family.ToString());
                    break;
                case CardType.Equipment:
                    LoadFromString(Card.Data.Name, "Equipment");
                    break;
                case CardType.Tools:
                    LoadFromString(Card.Data.Name, "Tools");
                    break;
                default:
                break;
            }
        }

        /// <summary>
        /// Load the sprite from the string.
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="family"></param>
        private void LoadFromString(string characterName, string family)
        {
            if (characterName == null) return;

            string folderPath = $"Sprites/Card/{family}";
            string spritePath = $"{folderPath}/{characterName}";

            Sprite sprite = Resources.Load<Sprite>(spritePath);

            if (sprite == null)
            {
                Debug.LogError($"Sprite not found at {spritePath}");
                return;
            }

            character.GetComponent<Image>().sprite = sprite;

            string backgroundPath = $"{folderPath}/Background";

            Sprite backgroundSprite = Resources.Load<Sprite>(backgroundPath);

            if (backgroundSprite == null)
            {
                Debug.LogError($"Background sprite not found at {backgroundPath}");
                return;
            }

            background.GetComponent<Image>().sprite = backgroundSprite;
        }

        private void OnValidate()
        {
            if (debugMode)
            {
                Debug.Log("CardBase: Debug mode is enabled. Make sure to disable it before testing anything.");
            }
        }
    }
}