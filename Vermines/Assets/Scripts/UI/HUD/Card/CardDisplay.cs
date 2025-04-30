using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Enumerations;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;

namespace Vermines.UI.Card
{
    public class CardDisplay : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _cardNameText;
        [SerializeField] private TextMeshProUGUI _eloquenceText;
        [SerializeField] private TextMeshProUGUI _soulsText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _backgroundImage;

        private ICardClickHandler _clickHandler;

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

            _cardNameText.text = data.Name;
            _eloquenceText.text = data.Eloquence.ToString();
            _soulsText.text = data.Souls.ToString();
            // _effectText.text = data.Effects?.FirstOrDefault()?.Description ?? "";

            string family = data.Type switch
            {
                CardType.Partisan => data.Family.ToString(),
                CardType.Equipment => "Equipment",
                CardType.Tools => "Tools",
                _ => "Unknown"
            };

            LoadVisuals(data.Name, family);
        }

        private void LoadVisuals(string characterName, string family)
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
        }

        public void Clear()
        {
            _cardNameText.text = string.Empty;
            _eloquenceText.text = string.Empty;
            _soulsText.text = string.Empty;
            //_effectText.text = string.Empty;
            _characterImage.sprite = null;
            _backgroundImage.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[ShopCardSlot] Card {gameObject.name} clicked.");
            _clickHandler?.OnCardClicked(Card);
        }
    }
}
