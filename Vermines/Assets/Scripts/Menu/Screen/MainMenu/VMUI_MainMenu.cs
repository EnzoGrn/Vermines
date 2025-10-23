using Fusion;
using OMGG.Menu.Screen;
using UnityEngine;
using OMGG.Menu.Configuration;

namespace Vermines.Menu.Screen {

    using Vermines.Environment.Interaction.Button;

    using Text       = TMPro.TMP_Text;
    using InputField = TMPro.TMP_InputField;

    /// <summary>
    /// Vermines Main Menu UI partial class.
    /// Extends of the <see cref="MenuUIMain" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_MainMenu : MenuUIMain {

        #region Profile

        [Header("Profile")]

        /// <summary>
        /// The username label.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _UsernameLabel;

        /// <summary>
        /// The username input UI part.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _UsernameView;

        /// <summary>
        /// The actual username input field.
        /// </summary>
        [InlineHelp, SerializeField]
        protected InputField _UsernameInput;

        /// <summary>
        /// The username confirmation button (background).
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _UsernameConfirmButton;

        /// <summary>
        /// The username change button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _UsernameButton;

        #endregion

        #region Navigation Buttons

        [Header("Navigation Buttons")]

        /// <summary>
        /// The open setting button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected ButtonSignInteraction _SettingsButton;

        /// <summary>
        /// The quick play button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected ButtonSignInteraction _PlayButton;

        /// <summary>
        /// The quit button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected ButtonSignInteraction _QuitButton;

        [Header("Game Object")]

        /// <summary>
        /// The sign object that contains the buttons.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _SignObject;

        #endregion

        #region Partial Methods

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #endregion

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// Applies the current selected graphics settings (loaded from PlayerPrefs)
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            new MenuGraphicsSettings().Apply();

            #if UNITY_STANDALONE
                _QuitButton.gameObject.SetActive(true);
            #else
                _QuitButton.gameObject.SetActive(false);
            #endif

            AwakeUser();
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// Initialized the default arguments.
        /// </summary>
        public override void Init()
        {
            base.Init();

            ConnectionArgs.SetDefaults(Config);

            InitUser();
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (_UsernameView) {
                _UsernameView.SetActive(false);

                if (_UsernameLabel)
                    _UsernameLabel.text = ConnectionArgs.Username;
            }

            ActiveButton();
            ShowUser();

            if (_SignObject)
                _SignObject.SetActive(true);
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            DeactiveButton();
            HideUser();

            if (_SignObject)
                _SignObject.SetActive(true);
        }

        #endregion

        #region Methods

        public void ActiveButton()
        {
            _PlayButton.onClick.AddListener(OnPlayButtonPressed);
            _SettingsButton.onClick.AddListener(OnSettingsButtonPressed);
            _QuitButton.onClick.AddListener(OnQuitButtonPressed);
        }

        public void DeactiveButton()
        {
            _PlayButton.onClick.RemoveListener(OnPlayButtonPressed);
            _SettingsButton.onClick.RemoveListener(OnSettingsButtonPressed);
            _QuitButton.onClick.RemoveListener(OnQuitButtonPressed);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the sceen background is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnFinishUsernameEdit()
        {
            OnFinishUsernameEdit(_UsernameInput.text);
        }

        /// <summary>
        /// Is called when the <see cref="_UsernameInput"/> has finished editing using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnFinishUsernameEdit(string username)
        {
            if (_UsernameView) {
                _UsernameView.SetActive(false);

                if (string.IsNullOrEmpty(username) == false) {
                    _UsernameLabel.text     = username;
                    ConnectionArgs.Username = username;
                }
            } else
                Debug.LogError("Username view is not assigned in the inspector.");
        }

        /// <summary>
        /// Is called when the <see cref="_UsernameButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnUsernameButtonPressed()
        {
            if (_UsernameView) {
                _UsernameView.SetActive(true);

                _UsernameInput.text = _UsernameLabel.text;
            }
            else
                Debug.LogError("Username view is not assigned in the inspector.");
        }

        /// <summary>
        /// Is called when the <see cref="_PlayButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual void OnPlayButtonPressed()
        {
            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            if (camera) {
                camera.OnSplineEnd.AddListener(() => InTavern());
                camera.OnKnotPassed.AddListener((knotIndex) => OnKnotPassed(knotIndex));

                camera.OnSplineStarted();
            } else {
                Debug.LogWarning($"[VMUI_MainMenu]: No CinemachineCamera found in the scene. The spline will not be played. Automatically open the 'VMUI_Tavern'.");

                InTavern();
            }
        }

        private void OnKnotPassed(int knotIndex)
        {
            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            if (camera) {
                if (camera.GetKnotsCount() / 2 == knotIndex)
                    camera.SetEndLookAt();
            }
        }

        private void InTavern()
        {
            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            if (camera) {
                camera.OnSplineEnd.RemoveListener(() => InTavern());
                camera.OnKnotPassed.RemoveListener((knotIndex) => OnKnotPassed(knotIndex));
            }

            Controller.Show<VMUI_Tavern>(this);
        }

        /// <summary>
        /// Is called when the <see cref="_SettingsButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnSettingsButtonPressed()
        {
            Controller.Show<VMUI_Settings>(this);
        }

        /// <summary>
        /// Is called when the <see cref="_QuitButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnQuitButtonPressed()
        {
            Application.Quit();
        }

        #endregion
    }
}
