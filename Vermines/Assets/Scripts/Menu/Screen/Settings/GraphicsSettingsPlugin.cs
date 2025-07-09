using Fusion;
using OMGG.Menu.Configuration;
using OMGG.Menu.Screen;
using OMGG.Menu.Tools;
using UnityEngine.UI;
using UnityEngine;

namespace Vermines.Menu.Pluggin {

    using Dropdown = TMPro.TMP_Dropdown;

    public class GraphicsSettingsPlugin : MenuScreenPlugin {

        #region Settings Fields

        [Header("Settings Fields")]

        [Header(" - Resolution")]

        /// <summary>
        /// The resoltion GameObject to disable this option.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _GoResolution;

        /// <summary>
        /// The resolution dropdown.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Dropdown _UIResolution;

        /// <summary>
        /// The resolution dropdown option map.
        /// </summary>
        protected DropDownEntry<int> _EntryResolution;

        [Header(" - FullScreen")]

        /// <summary>
        /// The fullscreen GameObject to disable this option.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _GoFullscreen;

        /// <summary>
        /// The fullscreen toggle.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Toggle _UIFullscreen;

        [Header(" - Graphics Quality")]

        /// <summary>
        /// The graphics quality dropdown.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Dropdown _UIGraphicsQuality;

        /// <summary>
        /// The graphics quality dropdown option map.
        /// </summary>
        protected DropDownEntry<int> _EntryGraphicsQuality;

        [Header(" - Framerate")]

        /// <summary>
        /// The framerate dropdown.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Dropdown _UIFramerate;

        /// <summary>
        /// The framerate dropdown option map.
        /// </summary>
        protected DropDownEntry<int> _EntryFramerate;

        [Header(" - VSync")]

        /// <summary>
        /// The VSync toggle.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Toggle _UIVSyncCount;

        #endregion

        /// <summary>
        /// The graphics settings object.
        /// </summary>
        protected MenuGraphicsSettings _GraphicsSettings;

        #region Overrides Methods

        public override void Show(MenuUIScreen screen)
        {
            base.Show(screen);

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
        }

        public override void Init(MenuUIScreen screen)
        {
            base.Init(screen);

            _EntryFramerate       = new DropDownEntry<int>(_UIFramerate, SaveChanges);
            _EntryResolution      = new DropDownEntry<int>(_UIResolution, SaveChanges);
            _EntryGraphicsQuality = new DropDownEntry<int>(_UIGraphicsQuality, SaveChanges);

            _UIVSyncCount.onValueChanged.AddListener(_ => SaveChanges());
            _UIFullscreen.onValueChanged.AddListener(_ => SaveChanges());

            // TODO: Maybe create a Vermines Graphics Settings (that store data in a Vermines config and not an OMGG one.),
            // Than inherits MenuGraphicsSettings and override variables getters and setters.
            _GraphicsSettings = new MenuGraphicsSettings();

            #if UNITY_IOS || UNITY_ANDROID
                _GoResolution.SetActive(false);
                _GoFullscreenn.SetActive(false);
            #endif
        }

        #endregion

        #region Methods

        private void SaveChanges()
        {
            _GraphicsSettings.Fullscreen   = _UIFullscreen.isOn;
            _GraphicsSettings.Framerate    = _EntryFramerate.Value;
            _GraphicsSettings.Resolution   = _EntryResolution.Value;
            _GraphicsSettings.QualityLevel = _EntryGraphicsQuality.Value;
            _GraphicsSettings.VSync        = _UIVSyncCount.isOn;

            _GraphicsSettings.Apply();
        }

        #endregion
    }
}
