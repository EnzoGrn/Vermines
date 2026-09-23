using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Enumerations;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Data.Effect;
using Vermines.UI.Utils;

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

        [Header("Selection Visual")]
        [SerializeField] private float liftHeight = 30f;

        private ICardClickHandler _clickHandler = null;
        private Vector3 _originalPosition;
        private bool _isSelected = false;

        public ICard Card { get; private set; }

        private void Awake()
        {
            _originalPosition = transform.localPosition;
        }

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
            string cardType = data.Type switch
            {
                CardType.Partisan => "Partisan",
                CardType.Equipment => "Item",
                CardType.Tools => "Item",
                _ => "Unknown"
            };

            LoadVisuals(data, cardType);
        }

        private void LoadVisuals(CardData data, string cardType)
        {
            // Character/background lookups go through the shared
            // UISpriteLoader utility (same path convention already used by
            // PlayerBannerUI/CardPopupBase/BookPagePlugin/etc:
            // Sprites/Card/{family-or-type}/{name}) instead of rebuilding
            // the path string here.
            _characterImage.sprite = data.Sprite != null
                ? data.Sprite
                : UISpriteLoader.GetDefaultSprite(data.Type, data.Family, data.SpriteName);

            _backgroundImage.sprite = UISpriteLoader.GetDefaultSprite(data.Type, data.Family, "Background");

            if (!_characterImage.sprite)
                Debug.LogError($"[CardDisplay] Character sprite not found for {data.Name} ({data.Type}/{data.Family}).");

            if (!_backgroundImage.sprite)
                Debug.LogError($"[CardDisplay] Background sprite not found for {data.Type}/{data.Family}.");

            // Description image uses a different folder/naming convention
            // (Sprites/UI/Card/, not Sprites/Card/) that UISpriteLoader has
            // no method for - left as its own lookup.
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

            // FIX: the following were never reset, leaving a previous card's
            // level, effect text, description image and lifted/selected
            // position visually stuck on an otherwise "empty" slot.
            _levelText.text = string.Empty;
            _levelText.gameObject.SetActive(false);
            _effectDescription.text = string.Empty;
            _descriptionImage.sprite = null;

            // NOTE: deliberately NOT nulling Card here. At least one known
            // caller (GameplayUITable.OnCardSacrified) reads
            // slot.CardDisplay.Card.ID after only checking that CardDisplay
            // itself is truthy, not that .Card is non-null - nulling it here
            // would introduce a new NullReferenceException on any table scan
            // that reaches a reset/empty slot. Card stays stale (points to
            // the last card shown) until Display() is called again - a
            // narrower, pre-existing staleness that's out of scope to fix
            // here without auditing every consumer first.
            _clickHandler = null;
            SetSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _clickHandler?.OnCardClicked(Card);
        }

        public void SetClickHandler(ICardClickHandler clickHandler)
        {
            _clickHandler = clickHandler;
        }

        public ICardClickHandler GetClickHandler()
        {
            return _clickHandler;
        }

        public void SetSelected(bool selected)
        {
            if (_isSelected == selected) return;

            _isSelected = selected;

            if (selected)
            {
                transform.localPosition = _originalPosition + Vector3.up * liftHeight;
            }
            else
            {
                transform.localPosition = _originalPosition;
            }
        }
    }
}
