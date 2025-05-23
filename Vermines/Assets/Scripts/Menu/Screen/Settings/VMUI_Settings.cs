using OMGG.Menu.Configuration;
using OMGG.Menu.Screen;
using OMGG.Menu.Tools;
using UnityEngine.UI;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {

    using Dropdown = TMPro.TMP_Dropdown;

    /// <summary>
    /// Vermines Settings Menu UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_Settings : MenuUIScreen {

        #region Navigation

        [Header("Navigation")]

        /// <summary>
        /// The back button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _BackButton;

        #endregion

        #region Settings Fields

        [Header("Settings Fields")]

        /// <summary>
        /// The fullscreen toggle.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Toggle _UIFullscreen;

        /// <summary>
        /// The fullscreen GameObject to disable this option.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _GoFullscreen;

        /// <summary>
        /// The framerate dropdown.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Dropdown _UIFramerate;

        /// <summary>
        /// The graphics quality dropdown.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Dropdown _UIGraphicsQuality;

        /// <summary>
        /// The resolution dropdown.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Dropdown _UIResolution;

        /// <summary>
        /// The resoltion GameObject to disable this option.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _GoResolution;

        /// <summary>
        /// The VSync toggle.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Toggle _UIVSyncCount;

        /// <summary>
        /// The framerate dropdown option map.
        /// </summary>
        protected DropDownEntry<int> _EntryFramerate;

        /// <summary>
        /// The resolution dropdown option map.
        /// </summary>
        protected DropDownEntry<int> _EntryResolution;

        /// <summary>
        /// The graphics quality dropdown option map.
        /// </summary>
        protected DropDownEntry<int> _EntryGraphicsQuality;

        /// <summary>
        /// The graphics settings object.
        /// </summary>
        protected MenuGraphicsSettings _GraphicsSettings;

        #endregion

        #region Partial Methods

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();
        partial void SaveChangesUser();

        #endregion

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            _EntryFramerate       = new DropDownEntry<int>(_UIFramerate, SaveChanges);
            _EntryResolution      = new DropDownEntry<int>(_UIResolution, SaveChanges);
            _EntryGraphicsQuality = new DropDownEntry<int>(_UIGraphicsQuality, SaveChanges);

            _UIVSyncCount.onValueChanged.AddListener(_ => SaveChanges());
            _UIFullscreen.onValueChanged.AddListener(_ => SaveChanges());

            // TODO: Maybe create a Vermines Graphics Settings (that store data in a Vermines config and not an OMGG one.),
            // Than inherits MenuGraphicsSettings and override variables getters and setters.
            _GraphicsSettings = new MenuGraphicsSettings();

            #if UNITY_IOS || UNITY_ANDROID
                _goResolution.SetActive(false);
                _goFullscreenn.SetActive(false);
            #endif

            AwakeUser();
        }

        #endregion

        #region Methods

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
        /// </summary>
        public override void Show()
        {
            base.Show();

            _EntryFramerate.SetOptions(_GraphicsSettings.CreateFramerateOptions, _GraphicsSettings.Framerate, s => (s == -1 ? "Platform Default" : s.ToString()));

            {
                _EntryResolution.SetOptions(_GraphicsSettings.CreateResolutionOptions, _GraphicsSettings.Resolution, s =>

                #if UNITY_2022_2_OR_NEWER
                    $"{UnityEngine.Screen.resolutions[s].width} x {UnityEngine.Screen.resolutions[s].height} @ {Mathf.RoundToInt((float)UnityEngine.Screen.resolutions[s].refreshRateRatio.value)}");
                #else
                    UnityEngine.Screen.resolutions[s].ToString());
                #endif
            }

            _EntryGraphicsQuality.SetOptions(_GraphicsSettings.CreateGraphicsQualityOptions, _GraphicsSettings.QualityLevel, s => QualitySettings.names[s]);

            _UIFullscreen.isOn = _GraphicsSettings.Fullscreen;
            _UIVSyncCount.isOn = _GraphicsSettings.VSync;

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

        /// <summary>
        /// Saving changes callbacks are registered to all ui elements during <see cref="Show()"/>.
        /// If defined the partial SaveChangesUser() is also called in the end.
        /// </summary>
        protected virtual void SaveChanges()
        {
            if (IsShowing == false)
                return;
            _GraphicsSettings.Fullscreen = _UIFullscreen.isOn;
            _GraphicsSettings.Framerate = _EntryFramerate.Value;
            _GraphicsSettings.Resolution = _EntryResolution.Value;
            _GraphicsSettings.QualityLevel = _EntryGraphicsQuality.Value;
            _GraphicsSettings.VSync = _UIVSyncCount.isOn;
            _GraphicsSettings.Apply();

            SaveChangesUser();
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_BackButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            Controller.HideModal(this);
        }

        #endregion
    }
}
