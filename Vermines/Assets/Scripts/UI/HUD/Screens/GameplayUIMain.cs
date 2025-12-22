using Fusion;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.UI.Card;
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
        /// The book button.
        /// Can't be null, but can be disabled.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _BookButton;

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

            GameEvents.OnPhaseChanged.AddListener(UpdateTurnButton);
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

            GameEvents.OnPhaseChanged.RemoveListener(UpdateTurnButton);
            GameEvents.OnCardClicked.RemoveListener(OnCardButtonPressed);
        }

        #endregion

        #region Methods

        protected void UpdateTurnButton(PhaseType phase)
        {
            if (_TurnButton == null || !PlayerController.Local)
                return;
            SceneContext context = PlayerController.Local.Context;

            bool isMyTurn = context.GameplayMode.IsMyTurn;

            _TurnButton.interactable = isMyTurn;

            var labelKey = isMyTurn ? GetButtonTranslationKey(context.GameplayMode.PhaseManager.CurrentPhase) : "ui.button.wait_your_turn";

            _TurnButton.GetComponentInChildren<Text>().text = Translate(labelKey);
        }

        protected string GetButtonTranslationKey(PhaseType phase)
        {
            return phase switch
            {
                PhaseType.Sacrifice => "ui.button.next_gain",
                PhaseType.Gain => "ui.button.next_action",
                PhaseType.Action => "ui.button.next_resolution",
                PhaseType.Resolution => "ui.button.next_player",
                _ => "ui.button.unknown_phase"
            };
        }

        protected string Translate(string key)
        {
            LocalizedString localized = new LocalizedString("UIUtils", key);
            return localized.GetLocalizedString();
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_TurnButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnAttemptToNextPhase()
        {
            //UIContextManager.Instance.ClearContext();

            SceneContext context      = PlayerController.Local.Context;
            PhaseManager phaseManager = context.GameplayMode.PhaseManager;

            if (phaseManager.CurrentPhase == PhaseType.Sacrifice) {
                Controller.ShowDualPopup(new SacrificeSkipStrategy());

                return;
            }

            if (context.HandManager.HasCards() && phaseManager.CurrentPhase == PhaseType.Action) {
                Controller.ShowDualPopup(new DefaultDiscardStrategy());

                return;
            }

            phaseManager.Phases[phaseManager.CurrentPhase].OnPhaseEnding(context.Runner.LocalPlayer, false);
        }

        /// <summary>
        /// Is called when the <see cref="_TableButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual void OnTableButtonPressed()
        {
            Controller.GetActiveScreen(out GameplayUIScreen lastScreen);
            if (lastScreen != null)
            {
                // If we are already on the table, do nothing.
                if (lastScreen is GameplayUITable)
                    return;
            }
            Controller.Show<GameplayUITable>(lastScreen);
        }

        /// <summary>
        /// Is called when the <see cref="_BookButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual void OnBookButtonPressed()
        {
            Controller.GetActiveScreen(out GameplayUIScreen lastScreen);
            if (lastScreen != null)
            {
                // If we are already on the book, do nothing.
                if (lastScreen is GameplayUIBook)
                    return;
            }
            Controller.Show<GameplayUIBook>(lastScreen);
        }

        /// <summary>
        /// Is called when the <see cref="_RecycleButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual void OnRecycleButtonPressed()
        {
            Controller.GetActiveScreen(out GameplayUIScreen lastScreen);
            if (lastScreen != null)
            {
                // If we are already on the recycle, do nothing.
                if (lastScreen is GameplayUIRecycle)
                    return;
            }
            Controller.Show<GameplayUIRecycle>(lastScreen);
        }

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