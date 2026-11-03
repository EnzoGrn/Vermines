using Fusion;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;
using Vermines.CardSystem.Elements;
using Vermines.Core.Scene;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public partial class GameplayUIMain : GameplayUIScreen
    {
        #region Attributes

        [Header("Navigation Buttons")]

        /// <summary>
        /// The turn button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _TurnButton;

        /// <summary>
        /// The table button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _TableButton;

        /// <summary>
        /// The recycle button.
        /// Can't be null, but can be disabled.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _RecycleButton;

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
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();

            if (_TurnButton == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUIMain Critical Error: Missing 'Button' reference on GameObject '{0}'. This component is required to render the turn button. Please assign a valid Button in the Inspector.",
                    gameObject.name
                );
                return;
            }
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

            GameEvents.OnCardClicked.AddListener(OnCardButtonPressed);
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            GameEvents.OnCardClicked.RemoveListener(OnCardButtonPressed);
        }

        #endregion

        #region Methods
        #endregion

        #region Events


        // NOTE: GameEvents.OnCardClicked is shared between this screen and
        // GameplayUITable - both subscribe to the same event independently.
        // slotId acts as a dispatch sentinel: >= 0 means a real table slot
        // click, handled by GameplayUITable's own listener; -1 means "no
        // slot" (e.g. a hand-card click), handled here instead to show card
        // info. Not enforced by any shared contract, just a convention - if a
        // future click source ever emits a real card+slot combo without
        // meaning "table interaction", it would incorrectly also trigger this
        // handler. Documented here since it wasn't obvious from this file
        // alone; not changed.
        protected virtual void OnCardButtonPressed(ICard card, int slotId)
        {
            if (card == null || slotId > -1)
                return;

            Controller.GetActiveScreen(out GameplayUIScreen lastScreen);

            if (lastScreen != null)
            {
                // If we are already on the card, do nothing.
                if (lastScreen is GameplayUICardInfo)
                    return;
            }
            Controller.ShowWithParams<GameplayUICardInfo, ICard>(card, this);
        }

        #endregion
    }
}
