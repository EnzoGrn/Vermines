using Fusion;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using Vermines.UI.Shop;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;

    /// <summary>
    /// Manage the display of the remove popup in the gameplay screen.
    /// </summary>
    public class RemovePopupPlugin : GameplayScreenPlugin, IGameplayScreenPluginParam<ICard>
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
        protected Button removeButton;

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

            SetContent();
            CardDisplay display = cardDisplay.GetComponent<CardDisplay>();
            display.Display(_cardData, null);
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
        private void Awake()
        {
            ClearUI();
        }

        /// <summary>
        /// Unity callback used to validate references when edited in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (cardDisplay == null)
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Card display is not assigned.");

            if (nameText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Name text is not assigned.");

            if (typeIcon == null)
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Type icon is not assigned.");

            if (typeText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Type text is not assigned.");

            if (soulValueText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Soul value text is not assigned.");

            if (descriptionText == null)
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Description text is not assigned.");

            if (removeButton == null)
            {
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Remove button is not assigned.");
            }

            if (cancelButton == null)
            {
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(RemovePopupPlugin), "Cancel button is not assigned.");
            }
        }

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

            removeButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Injects content and sets up the popup with card data and optional replace mode.
        /// </summary>
        public void SetContent()
        {
            ClearUI();
            nameText.text = _cardData.Data.Name;

            //typeIcon.sprite = _cardData.Data.Type.;

            typeText.text = _cardData.Data.Type.ToString();

            // TODO: Localize this
            soulValueText.text = $"+{_cardData.Data.Souls} souls if sacrificed";

            if (cardDisplay.TryGetComponent(out CardDisplay display))
                display.Display(_cardData, null);

            foreach (var effect in _cardData.Data.Effects)
                descriptionText.text += effect.Description + "\n";

            removeButton.onClick.AddListener(() =>
            {
                GameEvents.OnCardSacrificedRequested.Invoke(_cardData);
                Hide();
            });

            cancelButton.onClick.AddListener(() =>
            {
                Hide();
            });
        }

        #endregion
    }
}