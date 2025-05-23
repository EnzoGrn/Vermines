using Fusion;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.UI.Card;
using Vermines.UI.Popup;

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

        [Header("Discard All View")]

        /// <summary>
        /// The discard all view.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _DiscardAllView;

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
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            _DiscardAllView.SetActive(false);

            GameEvents.OnPhaseChanged.RemoveListener(UpdateTurnButton);
        }

        #endregion

        #region Methods

        protected void UpdateTurnButton(PhaseType phase)
        {
            if (_TurnButton == null) return;

            if (GameManager.Instance == null)
            {
                _TurnButton.interactable = false;
                _TurnButton.GetComponentInChildren<Text>().text = Translate("ui.button.wait_your_turn");
                return;
            }

            bool isMyTurn = GameManager.Instance.IsMyTurn();
            _TurnButton.interactable = isMyTurn;

            var labelKey = isMyTurn
                ? GetButtonTranslationKey(PhaseManager.Instance.CurrentPhase)
                : "ui.button.wait_your_turn";

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

        protected void ShowDiscardPopup(IDiscardPopupStrategy strategy)
        {
            if (_DiscardAllView == null)
            {
                Debug.LogError("Discard popup is not assigned.");
                return;
            }

            var popup = _DiscardAllView.GetComponent<PopupConfirm>();
            popup.Setup(
                strategy.GetTitle(),
                strategy.GetMessage(),
                strategy.OnConfirm,
                strategy.OnCancel
            );

            popup.ClearOnClosed();
            popup.OnClosed += () =>
            {
                _DiscardAllView.SetActive(false);
            };
            _DiscardAllView.SetActive(true);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_TurnButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnAttemptToNextPhase()
        {
            UIContextManager.Instance.ClearContext();

            if (PhaseManager.Instance.CurrentPhase == PhaseType.Sacrifice)
            {
                ShowDiscardPopup(new SacrificeSkipStrategy());
                return;
            }

            if (HandManager.Instance.HasCards() && PhaseManager.Instance.CurrentPhase == PhaseType.Action)
            {
                ShowDiscardPopup(new DefaultDiscardStrategy());
                return;
            }

            GameEvents.OnAttemptNextPhase.Invoke();
        }

        /// <summary>
        /// Is called when the <see cref="_DiscardAllView"/> is clicked outside the popup.
        /// </summary>
        protected virtual void OnCloseDiscardAllView()
        {
            _DiscardAllView.SetActive(false);
        }

        /// <summary>
        /// Is called when the <see cref="_TableButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual void OnTableButtonPressed()
        {
            Controller.GetActiveScreen(out GameplayUIScreen lastScreen);
            Debug.Log("Last screen: " + lastScreen);
            Controller.Show<GameplayUITable>(lastScreen);
        }

        #endregion
    }
}