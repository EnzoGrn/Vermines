using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Enumerations;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Data.Effect;

namespace Vermines.UI.Card
{
    public class CardDisplay : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _cardNameText;
        [SerializeField] private TextMeshProUGUI _eloquenceText;
        [SerializeField] private TextMeshProUGUI _soulsText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _effectDescription;
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _descriptionImage;

        private ICardClickHandler _clickHandler = null;

        // TODO: add the card type (partisan, object, etc.)

        public ICard Card { get; private set; }

        public void Display(ICard card, ICardClickHandler clickHandler = null)
        {
            if (card == null)
            {
                Debug.LogError("[CardDisplay] CardData is null.");
                return;
            }

            Card = card;
            _clickHandler = clickHandler;
            CardData data = card.Data;

            // --- Basic values
            _cardNameText.text = data.Name;
            _eloquenceText.text = data.Eloquence.ToString();
            _soulsText.text = data.Souls.ToString();
            _effectDescription.text = string.Empty;

            if (data.Type == CardType.Partisan)
            {
                _levelText.text = data.Level.ToString();
                _levelText.gameObject.SetActive(true);
            }
            else
            {
                _levelText.gameObject.SetActive(false);
            }

            foreach (AEffect effect in data.Effects)
            {
                _effectDescription.text += "- " + effect.Description + "\n";
            }

            // --- Load visuals
            string family = data.Type switch
            {
                CardType.Partisan => data.Family.ToString(),
                CardType.Equipment => "Equipment",
                CardType.Tools => "Tools",
                _ => "Unknown"
            };

            string type = data.Type switch
            {
                CardType.Partisan => "Partisan",
                CardType.Equipment => "Item",
                CardType.Tools => "Item",
                _ => "Unknown"
            };

            LoadVisuals(data.Name, family, type);

            // --- Effects
            //RefreshEffects(data.Draw());
        }

        private void LoadVisuals(string characterName, string family, string cardType)
        {
            string basePath = $"Sprites/Card/{family}";
            string characterPath = $"{basePath}/{characterName}";
            string backgroundPath = $"{basePath}/Background";

            _characterImage.sprite = Resources.Load<Sprite>(characterPath);
            _backgroundImage.sprite = Resources.Load<Sprite>(backgroundPath);

            if (!_characterImage.sprite)
                Debug.LogError($"[CardDisplay] Character sprite not found: {characterPath}");

            if (!_backgroundImage.sprite)
                Debug.LogError($"[CardDisplay] Background sprite not found: {backgroundPath}");

            // Set description image based on card type
            string descriptionPath = $"Sprites/UI/Card/{cardType}_Card_Descriptor";

            _descriptionImage.sprite = Resources.Load<Sprite>(descriptionPath);

            if (!_descriptionImage.sprite)
                Debug.LogError($"[CardDisplay] Description sprite not found: {descriptionPath}");
        }

        public void Clear()
        {
            _cardNameText.text = string.Empty;
            _eloquenceText.text = string.Empty;
            _soulsText.text = string.Empty;
            _characterImage.sprite = null;
            _backgroundImage.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[CardDisplay] Card {gameObject.name} clicked.");
            _clickHandler?.OnCardClicked(Card);
        }

        public void SetClickHandler(ICardClickHandler clickHandler)
        {
            _clickHandler = clickHandler;
        }
    }
}
