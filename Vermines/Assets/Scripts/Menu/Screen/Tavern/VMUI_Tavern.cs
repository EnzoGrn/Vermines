using System.Threading.Tasks;
using OMGG.Menu.Screen;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {

    using Button = UnityEngine.UI.Button;

    using Vermines.Menu.Tavern;
    using Vermines.Characters;

    /// <summary>
    /// Vermines Tarvern UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_Tavern : MenuUIScreen {

        [Tooltip("The scene refence that will be load in network, when the game will be found.")]
        public SceneRef SceneRef;

        #region User

        public Cultist SelectedCultist;

        #endregion

        #region Navigation Buttons

        [Header("Navigation Buttons")]

        /// <summary>
        /// The quick play button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _PlayButton;

        /// <summary>
        /// The open party screen button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _PartyButton;

        /// <summary>
        /// Return to the main menu button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _MainMenuButton;

        /// <summary>
        /// The open settings screen button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _SettingsButton;

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

            InitUser();
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();

            CultistSelectDisplay cultistSelectDisplay = FindFirstObjectByType<CultistSelectDisplay>();

            //if (cultistSelectDisplay)
            //    SelectedCultist = cultistSelectDisplay.GetSelectedCultist();
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

        #region Events

        /// <summary>
        /// Is called when the <see cref="_PlayButton"/> is pressed using SendMessage() from the UI object.
        /// Intitiates the connection and expects the connection object to set further screen states.
        /// </summary>
        protected virtual async void OnPlayButtonPressed()
        {
            CultistSelectDisplay cultistSelectDisplay = FindFirstObjectByType<CultistSelectDisplay>();

            //if (cultistSelectDisplay)
            //    SelectedCultist = cultistSelectDisplay.GetSelectedCultist();
            ConnectionArgs.Session  = null;
            ConnectionArgs.Creating = false;
            ConnectionArgs.Region   = ConnectionArgs.PreferredRegion;

            Controller.Show<VMUI_Loading>(this);

            var result = await Connection.ConnectAsync(ConnectionArgs, SceneRef);

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
        /// Is called when the <see cref="_MainMenuButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual async void OnMainMenuButtonPressed()
        {
            SelectedCultist = null;

            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            Controller.Show<VMUI_MainMenu>(this);

            if (camera) {
                AnimatorStateInfo stateInfo = _Animator.GetCurrentAnimatorStateInfo(0);

                float duration = stateInfo.length;

                await Task.Delay((int)(duration * 1000));

                camera.OnSplineReseted();
            }
        }

        /// <summary>
        /// Is called when the <see cref="_SettingsButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnSettingsButtonPressed()
        {
            Controller.Show<VMUI_Settings>(this);
        }

        /// <summary>
        /// Is called when the <see cref="_PartyButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnPartyMenuButtonPressed()
        {
            SelectedCultist = null;

            Controller.Show<VMUI_PartyMenu>(this);
        }

        #endregion
    }
}
