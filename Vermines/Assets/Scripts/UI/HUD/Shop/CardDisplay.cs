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
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI eloquenceText;
        [SerializeField] private TextMeshProUGUI soulsText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image backgroundImage;

        private ICard _card;
        private ICardClickHandler _clickHandler;

        public void Display(ICard card, ICardClickHandler clickHandler = null)
        {
            if (card == null)
            {
                Debug.LogError("[CardDisplay] CardData is null.");
                return;
            }

            _card = card;
            _clickHandler = clickHandler;
            CardData data = card.Data;

            cardNameText.text = data.Name;
            eloquenceText.text = data.Eloquence.ToString();
            soulsText.text = data.Souls.ToString();
            // effectText.text = data.Effects?.FirstOrDefault()?.Description ?? "";

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

            characterImage.sprite = Resources.Load<Sprite>(characterPath);
            backgroundImage.sprite = Resources.Load<Sprite>(backgroundPath);

            if (!characterImage.sprite)
                Debug.LogError($"[CardDisplay] Character sprite not found: {characterPath}");

            if (!backgroundImage.sprite)
                Debug.LogError($"[CardDisplay] Background sprite not found: {backgroundPath}");
        }

        public void Clear()
        {
            cardNameText.text = string.Empty;
            eloquenceText.text = string.Empty;
            soulsText.text = string.Empty;
            //effectText.text = string.Empty;
            characterImage.sprite = null;
            backgroundImage.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[ShopCardSlot] Card {gameObject.name} clicked.");
            _clickHandler?.OnCardClicked(_card);
        }

        public ICard GetCardData()
        {
            return _card;
        }
    }
}
