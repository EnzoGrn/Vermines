using OMGG.Menu.Screen;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {

    using Vermines.Player;

    using Button = UnityEngine.UI.Button;

    /// <summary>
    /// Vermines Gameplay Menu UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_Gameplay : MenuUIScreen {

        #region Fields

        /// <summary>
        /// Pause panel.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _PausePanelGO;

        /// <summary>
        /// Blocker.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _BlockerGO;

        /// <summary>
        /// The resume button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _ResumeButton;

        /// <summary>
        /// The settings button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _SettingsButton;

        /// <summary>
        /// The disconnect button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _DisconnectButton;

        #endregion

        #region Partial Methods

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #endregion

        #region Overrides Methods

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

            if (Controller.GetLastScreen(out MenuUIScreen screen) && screen is VMUI_Gameplay) {
                _BlockerGO.SetActive(true);
                _PausePanelGO.SetActive(true);
            }
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            _BlockerGO.SetActive(false);
            _PausePanelGO.SetActive(false);
        }

        #endregion

        #region Events

        public void TogglePauseMenu()
        {
            VMUI_Settings settings = Controller.Get<VMUI_Settings>();

            if (settings != null && settings.gameObject.activeSelf)
                return;
            if (_PausePanelGO.activeSelf) {
                OnResumeButtonPressed();
            } else {
                _BlockerGO.SetActive(true);
                _PausePanelGO.SetActive(true);
            }
        }

        /// <summary>
        /// Is called when the <see cref="_ResumeButton" /> is pressed using SendMessage() from the UI object. />
        /// </summary>
        protected virtual void OnResumeButtonPressed()
        {
            _BlockerGO.SetActive(false);
            _PausePanelGO.SetActive(false);
        }

        /// <summary>
        /// Is called when the <see cref="_SettingsButton" /> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnSettingsButtonPressed()
        {
            Controller.Show<VMUI_Settings>(this);
        }

        /// <summary>
        /// Is called when the <see cref="_disconnectButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual async void OnDisconnectButtonPressed()
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager)
                return;
            if (manager.HasStateAuthority)
                await manager.ReturnToMenu();
            else
                PlayerController.Local.ReturnToMenu();
        }

        #endregion
    }
}
