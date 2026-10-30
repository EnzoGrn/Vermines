using Fusion;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.UI.Card;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public partial class GameplayUICardInfo : GameplayUIScreen, IParamReceiver<ICard>
    {
        #region Attributes

        [Header("Navigation Buttons")]

        /// <summary>
        /// The close button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _CloseButton;

        [Header("Card Info")]

        /// <summary>
        /// The card display.
        /// </summary>
        [InlineHelp, SerializeField]
        protected CardDisplay _CardDisplay;

        /// <summary>
        /// The card name text.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _CardName;

        /// <summary>
        ///  The card family text.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _CardFamily;

        /// <summary>
        /// The card description text.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _CardDescription;

        /// <summary>
        /// The card souls text.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _CardSouls;

        /// <summary>
        /// The card eloquence text.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _CardEloquence;

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();

            #region Error Handling

            if (_CloseButton == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUITable Critical Error: Missing 'Button' reference on GameObject '{0}'. This component is required to render the close button. Please assign a valid Button in the Inspector.",
                    gameObject.name
                );
                return;
            }

            #endregion
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the callback to be called when the effect is done.
        /// </summary>
        /// <param name="onDone">The callback to be called when the effect is done.</param>
        public void SetParam(ICard cardContext)
        {
            if (cardContext == null)
            {
                Debug.LogError($"[{nameof(GameplayUICardInfo)}] SetParam called with null cardContext.");
                return;
            }
            Debug.Log($"[{nameof(GameplayUICardInfo)}] SetParam called with card: {cardContext.Data.Name}");
            if (_CardDisplay != null)
            {
                _CardDisplay.Display(cardContext);
            }
            if (_CardName != null)
            {
                _CardName.text = cardContext.Data.Name;
            }
            if (_CardFamily != null)
            {
                _CardFamily.text = cardContext.Data.Type switch
                {
                    CardType.Partisan => cardContext.Data.Family.ToString(),
                    CardType.Equipment => "Equipment",
                    CardType.Tools => "Tools",
                    _ => "Unknown"
                };
            }
            if (_CardDescription != null)
            {
                _CardDescription.text = string.Empty;
                foreach (var effect in cardContext.Data.Effects)
                {
                    _CardDescription.text += "- " + effect.Description + "\n";
                }
            }
            if (_CardSouls != null)
            {
                _CardSouls.text = cardContext.Data.Souls.ToString();
            }

            if (_CardEloquence != null)
            {
                _CardEloquence.text = cardContext.Data.Eloquence.ToString();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            Hide();
        }

        #endregion
    }
}