using Fusion;
using UnityEngine;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using Vermines.UI.Utils;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;

    public abstract class CardPopupBase : GameplayScreenPlugin, IGameplayScreenPluginParam<ICard>
    {
        #region Attributes

        /// <summary>
        /// The card data to be displayed in the popup.
        /// </summary>
        protected ICard _cardData;

        [Header("UI Elements")]

        /// <summary>
        /// The text component for displaying the name of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text nameText;

        /// <summary>
        /// The image component for displaying the type icon of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Image typeIcon;

        /// <summary>
        /// The text component for displaying the type of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text typeText;

        /// <summary>
        /// The text component for displaying the soul value of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text soulValueText;

        /// <summary>
        /// The text component for displaying the description of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text descriptionText;

        /// <summary>
        /// The button component for confirming the removal of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button confirmButton;

        /// <summary>
        /// The button component for canceling the removal of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button cancelButton;

        /// <summary>
        /// The card display object that shows the card's visual representation.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject cardDisplay;

        #endregion

        #region Override Methods

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
            SetupBase(_cardData);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }

        public void SetParam(ICard param)
        {
            _cardData = param;
        }

        #endregion

        #region Unity Methods

        /// <summary>
        /// Unity Awake method for initial setup.
        /// </summary>
        protected virtual void Awake()
        {
            ClearUI();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity callback used to validate references when edited in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (cardDisplay == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Card display is not assigned.");

            if (nameText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Name text is not assigned.");

            if (typeIcon == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Type icon is not assigned.");

            if (typeText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Type text is not assigned.");

            if (soulValueText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Soul value text is not assigned.");

            if (descriptionText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Description text is not assigned.");

            if (confirmButton == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Remove button is not assigned.");

            if (cancelButton == null)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(CardPopupBase), gameObject.name, "Cancel button is not assigned.");
        }
#endif

        #endregion

        #region Methods

        /// <summary>
        /// Clears all text fields in the popup.
        /// </summary>
        private void ClearUI()
        {
            nameText.text = string.Empty;
            typeIcon.sprite = null;
            typeText.text = string.Empty;
            soulValueText.text = string.Empty;
            descriptionText.text = string.Empty;
        }

        protected void DisplayCard(ICard card)
        {
            if (cardDisplay.TryGetComponent(out CardDisplay display))
                display.Display(card, null);
        }

        protected virtual void SetupBase(ICard card)
        {
            _cardData = card;
            ClearUI();

            // TODO: Localization support

            if (nameText) nameText.text = card.Data.Name;

            if (descriptionText)
            {
                foreach (var effect in card.Data.Effects)
                    descriptionText.text += effect.Description + "\n";
            }

            if (soulValueText && card.Data.Souls > 0)
                soulValueText.text = $"+{card.Data.CurrentSouls} souls if sacrificed";

            if (typeText)
                typeText.text = card.Data.Type == Vermines.CardSystem.Enumerations.CardType.Partisan
                    ? card.Data.Family.ToString()
                    : card.Data.Type.ToString();

            if (typeIcon)
                typeIcon.sprite = UISpriteLoader.GetDefaultSprite(card.Data.Type, card.Data.Family, "Icon");

            DisplayCard(card);

            Debug.Log($"[CardPopupBase] Setting up with card: {card.Data.Name}");
        }

        protected virtual void OnConfirm() { }
        protected virtual void OnCancel() { }

        protected void SetupButtons(Button confirm, Button cancel)
        {
            confirm.onClick.RemoveAllListeners();
            confirm.onClick.AddListener(OnConfirm);

            cancel.onClick.RemoveAllListeners();
            cancel.onClick.AddListener(OnCancel);

            confirmButton.interactable = GameManager.Instance.IsMyTurn();
        }

        #endregion
    }
}
