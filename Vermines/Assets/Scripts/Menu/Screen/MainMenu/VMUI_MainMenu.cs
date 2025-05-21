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
        /// The open party screen button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _PartyButton;

        /// <summary>
        /// The quit button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected ButtonSignInteraction _QuitButton;

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

            _UsernameView.SetActive(false);

            if (_UsernameLabel)
                _UsernameLabel.text = ConnectionArgs.Username;
            _PlayButton.OnClicked     += OnPlayButtonPressed;
            _SettingsButton.OnClicked += OnSettingsButtonPressed;
            _QuitButton.OnClicked     += OnQuitButtonPressed;

            ShowUser();
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            _PlayButton.OnClicked     -= OnPlayButtonPressed;
            _SettingsButton.OnClicked -= OnSettingsButtonPressed;
            _QuitButton.OnClicked     -= OnQuitButtonPressed;

            HideUser();
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
            _UsernameView.SetActive(false);

            if (string.IsNullOrEmpty(username) == false) {
                _UsernameLabel.text     = username;
                ConnectionArgs.Username = username;
            }
        }

        /// <summary>
        /// Is called when the <see cref="_UsernameButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnUsernameButtonPressed()
        {
            _UsernameView.SetActive(true);

            _UsernameInput.text = _UsernameLabel.text;
        }

        /// <summary>
        /// Is called when the <see cref="_PlayButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual async void OnPlayButtonPressed()
        {
            ConnectionArgs.Session  = null;
            ConnectionArgs.Creating = false;
            ConnectionArgs.Region   = ConnectionArgs.PreferredRegion;

            Controller.Show<VMUI_Loading>(this);

            var result = await Connection.ConnectAsync(ConnectionArgs);

            await Controller.HandleConnectionResult(result, Controller);
        }

        /// <summary>
        /// Is called when the <see cref="_PartyButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnPartyButtonPressed()
        {
            Controller.Show<VMUI_PartyMenu>(this);
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
