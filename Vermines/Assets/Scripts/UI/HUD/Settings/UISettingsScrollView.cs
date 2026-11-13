using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Vermines.UI.View {

    using Vermines.Core.Settings;
    using Vermines.Extension;
    using Vermines.Menu.View;
    using Vermines.UI.Core;

    /// <summary>
    /// Settings scroll, the same in game and in the menus (it replaces the book, UISettingsView).
    /// In game it has no parchment of its own: its content rides on the pause parchment, which rises a bit more
    /// while the settings are open. In the menus, the prefab variant shows its own parchment and dim.
    /// Reads and writes RuntimeSettings exactly like the book: values are pending until Save, discarded on Back.
    /// Back is the UICloseView close button; openers listen to HasClosed to come back.
    /// </summary>
    public class UISettingsScrollView : UICloseView {

        #region Constants

        // Same key LocalizationSelector writes.
        private const string LOCALE_PREFS_KEY = "LocaleID";

        #endregion

        #region Attributes

        [Header("Animation")]
        [SerializeField] private PausePanelAnimator _Anim;

        [Tooltip("On the list content: rows cascade in once the scroll is up.")]
        [SerializeField] private StaggerReveal _Reveal;

        [Tooltip("Optional, scrolled back to the top on open.")]
        [SerializeField] private ScrollRect _ScrollRect;

        [Header("Sections")]
        [SerializeField] private SettingsSection[] _Sections;

        [Tooltip("Opening a section folds the other ones.")]
        [SerializeField] private bool _Accordion = true;

        [Header("Audio")]
        [SerializeField] private UISlider _MusicSlider;
        [SerializeField] private UISlider _EffectsSlider;

        [Header("Graphics")]
        [SerializeField] private UIToggle _FullScreen;
        [SerializeField] private UIToggle _VSync;
        [SerializeField] private UISlider _Quality;
        [SerializeField] private TextMeshProUGUI _QualityLabel;
        [SerializeField] private string _QualityFormat = "QUALITY PRESET : {0}";
        [SerializeField] private UISlider _Display;
        [SerializeField] private TextMeshProUGUI _DisplayLabel;
        [SerializeField] private string _DisplayFormat = "DISPLAY : {0} X {1}";

        [Header("Language (pending until Save like the rest, not part of Reset)")]
        [SerializeField] private UISlider _Language;
        [SerializeField] private TextMeshProUGUI _LanguageLabel;
        [SerializeField] private string _LanguageFormat = "LANGUAGE : {0}";

        [Header("Ribbons (Back is the close button)")]
        [SerializeField] private UIButton _SaveButton;
        [SerializeField] private UIButton _ResetButton;

        // Screen.resolutions indices at the default refresh rate, smallest first.
        private readonly List<int> _ValidResolutions = new(32);

        private bool _Closing;

        // Locale picked on the slider, applied on Save. -1 = no pending change.
        private int _PendingLocale = -1;

        // Main menu whose buttons were switched off while the scroll is open (like the book did). Null in game.
        private UIMainMenuView _MainMenu;

        #endregion

        #region Getters & Setters

        private bool HasPendingLanguage
        {
            get
            {
                if (_PendingLocale < 0 || !LocalizationSettings.InitializationOperation.IsDone)
                    return false;
                List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;

                return _PendingLocale < locales.Count && locales[_PendingLocale] != LocalizationSettings.SelectedLocale;
            }
        }

        #endregion

        #region Methods

        private void PrepareResolutions()
        {
            _ValidResolutions.Clear();

            Resolution[] resolutions = UnityEngine.Screen.resolutions;

            if (resolutions.Length > 0) {
                int defaultRefreshRate = Mathf.RoundToInt((float)resolutions[^1].refreshRateRatio.value);

                for (int i = 0; i < resolutions.Length; i++) {
                    if (Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value) == defaultRefreshRate)
                        _ValidResolutions.Add(i);
                }
            }

            SetRange(_Display, _ValidResolutions.Count - 1);
            SetRange(_Quality, QualitySettings.names.Length - 1);

            _Display.interactable = _ValidResolutions.Count > 1;
        }

        private void LoadValues()
        {
            RuntimeSettings settings = Context.RuntimeSettings;

            _MusicSlider.SetOptionsValueFloat(settings.Options.GetValue(RuntimeSettings.KEY_MUSIC_VOLUME));
            _EffectsSlider.SetOptionsValueFloat(settings.Options.GetValue(RuntimeSettings.KEY_EFFECTS_VOLUME));

            _FullScreen.SetIsOnWithoutNotify(!settings.Windowed);
            _VSync.SetIsOnWithoutNotify(settings.VSync);

            _Quality.SetValueWithoutNotify(settings.GraphicsQuality);

            int display = _ValidResolutions.IndexOf(settings.Resolution);

            _Display.SetValueWithoutNotify(display >= 0 ? display : _ValidResolutions.Count - 1);

            LoadLanguage();
            RefreshLabels();
        }

        private void LoadLanguage()
        {
            if (_Language == null)
                return;
            bool ready = LocalizationSettings.InitializationOperation.IsDone;

            _Language.interactable = ready;

            if (!ready)
                return;
            List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;

            SetRange(_Language, locales.Count - 1);

            int shown = _PendingLocale >= 0 ? _PendingLocale : locales.IndexOf(LocalizationSettings.SelectedLocale);

            _Language.SetValueWithoutNotify(Mathf.Max(0, shown));
        }

        private void RefreshLabels()
        {
            string[] qualities = QualitySettings.names;
            int quality        = Mathf.RoundToInt(_Quality.value);

            if (quality >= 0 && quality < qualities.Length)
                _QualityLabel.SetTextSafe(string.Format(_QualityFormat, qualities[quality].ToUpperInvariant()));
            int display = Mathf.RoundToInt(_Display.value);

            if (display >= 0 && display < _ValidResolutions.Count) {
                Resolution resolution = UnityEngine.Screen.resolutions[_ValidResolutions[display]];

                _DisplayLabel.SetTextSafe(string.Format(_DisplayFormat, resolution.width, resolution.height));
            }

            if (_Language != null && LocalizationSettings.InitializationOperation.IsDone) {
                List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
                int language         = Mathf.RoundToInt(_Language.value);

                if (language >= 0 && language < locales.Count)
                    _LanguageLabel.SetTextSafe(string.Format(_LanguageFormat, LocaleDisplayName(locales[language])));
            }
        }

        private static string LocaleDisplayName(Locale locale)
        {
            System.Globalization.CultureInfo culture = locale.Identifier.CultureInfo;

            string name = culture != null ? culture.NativeName : locale.LocaleName;

            return name.ToUpperInvariant();
        }

        // Changing min / max fires onValueChanged, so the listeners are moved aside meanwhile.
        private static void SetRange(Slider slider, int max)
        {
            Slider.SliderEvent onValueChanged = slider.onValueChanged;

            slider.onValueChanged = new Slider.SliderEvent();
            slider.wholeNumbers   = true;
            slider.minValue       = 0;
            slider.maxValue       = Mathf.Max(0, max);
            slider.onValueChanged = onValueChanged;
        }

        private void FoldSections(bool instant, SettingsSection except = null)
        {
            if (_Sections == null)
                return;
            foreach (SettingsSection section in _Sections) {
                if (section != null && section != except)
                    section.SetExpanded(false, instant);
            }
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_Anim == null && !TryGetComponent(out _Anim))
                Debug.LogError("[UISettingsScrollView] No PausePanelAnimator assigned.", this);

            _MusicSlider.onValueChanged.AddListener(OnVolumeChanged);
            _EffectsSlider.onValueChanged.AddListener(OnVolumeChanged);

            _FullScreen.onValueChanged.AddListener(OnFullScreenChanged);
            _VSync.onValueChanged.AddListener(OnVSyncChanged);
            _Quality.onValueChanged.AddListener(OnQualityChanged);
            _Display.onValueChanged.AddListener(OnDisplayChanged);

            if (_Language != null)
                _Language.onValueChanged.AddListener(OnLanguageChanged);
            _SaveButton.onClick.AddListener(OnSaveButton);
            _ResetButton.onClick.AddListener(OnResetButton);

            if (_Sections != null) {
                foreach (SettingsSection section in _Sections) {
                    if (section != null)
                        section.Expanding += OnSectionExpanding;
                }
            }
        }

        protected override void OnDeinitialize()
        {
            _MusicSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            _EffectsSlider.onValueChanged.RemoveListener(OnVolumeChanged);

            _FullScreen.onValueChanged.RemoveListener(OnFullScreenChanged);
            _VSync.onValueChanged.RemoveListener(OnVSyncChanged);
            _Quality.onValueChanged.RemoveListener(OnQualityChanged);
            _Display.onValueChanged.RemoveListener(OnDisplayChanged);

            if (_Language != null)
                _Language.onValueChanged.RemoveListener(OnLanguageChanged);
            _SaveButton.onClick.RemoveListener(OnSaveButton);
            _ResetButton.onClick.RemoveListener(OnResetButton);

            if (_Sections != null) {
                foreach (SettingsSection section in _Sections) {
                    if (section != null)
                        section.Expanding -= OnSectionExpanding;
                }
            }

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            _Closing       = false;
            _PendingLocale = -1;
            IsInteractable = true;

            // The main menu buttons live in the 3D scene, under the scroll: no clicks on them meanwhile.
            // Switched off once and back on once, so their listeners are never added twice.
            if (_MainMenu == null) {
                _MainMenu = SceneUI.Get<UIMainMenuView>();

                if (_MainMenu != null)
                    _MainMenu.DeactiveButton();
            }

            PrepareResolutions();
            LoadValues();
            FoldSections(true);

            if (_ScrollRect != null)
                _ScrollRect.verticalNormalizedPosition = 1f;
            if (_Anim != null) {
                _Anim.Snap(false);
                _Anim.Show();
            }

            if (_Reveal != null)
                _Reveal.Play(_Anim != null ? _Anim.ShowDuration * 0.5f : 0f);
        }

        protected override void OnClose()
        {
            _Closing       = false;
            _PendingLocale = -1;

            if (_MainMenu != null) {
                _MainMenu.ActiveButton();

                _MainMenu = null;
            }

            // Back without Save drops the pending values, like the book.
            Context.RuntimeSettings.Options.DiscardChanges();
            Context.Audio.UpdateVolume();

            base.OnClose();
        }

        protected override void OnTick()
        {
            base.OnTick();

            _SaveButton.interactable = Context.RuntimeSettings.Options.HasUnsavedChanges || HasPendingLanguage;
        }

        // Back ribbon and back action: let the scroll leave, then really close (HasClosed brings the pause back).
        protected override void OnCloseButton()
        {
            if (_Closing)
                return;
            _Closing       = true;
            IsInteractable = false;

            if (_Anim == null) {
                base.OnCloseButton();

                return;
            }

            // Not keepDim: in the menus this view owns its dim, which must fade out with it (in game there is none).
            _Anim.Hide().OnComplete(() => base.OnCloseButton());
        }

        private void OnSectionExpanding(SettingsSection section)
        {
            if (_Accordion)
                FoldSections(false, section);
        }

        private void OnSaveButton()
        {
            RuntimeSettings settings = Context.RuntimeSettings;

            settings.Options.SaveChanges();

            if (!Application.isMobilePlatform || Application.isEditor) {
                Resolution[] resolutions = UnityEngine.Screen.resolutions;

                if (resolutions.Length > 0) {
                    int index = settings.Resolution < 0 || settings.Resolution >= resolutions.Length ? resolutions.Length - 1 : settings.Resolution;

                    UnityEngine.Screen.SetResolution(resolutions[index].width, resolutions[index].height, !settings.Windowed);
                }
            }

            QualitySettings.SetQualityLevel(settings.GraphicsQuality);
            QualitySettings.vSyncCount = settings.VSync ? 1 : 0;

            if (HasPendingLanguage) {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_PendingLocale];

                PlayerPrefs.SetInt(LOCALE_PREFS_KEY, _PendingLocale);
            }

            _PendingLocale = -1;
        }

        private void OnResetButton()
        {
            Options options = Context.RuntimeSettings.Options;

            options.ResetValueToDefault(RuntimeSettings.KEY_MUSIC_VOLUME    , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_EFFECTS_VOLUME  , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_GRAPHICS_QUALITY, false);
            options.ResetValueToDefault(RuntimeSettings.KEY_RESOLUTION      , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_WINDOWED        , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_VSYNC           , false);

            LoadValues();

            Context.Audio.UpdateVolume();
        }

        private void OnVolumeChanged(float value)
        {
            Context.RuntimeSettings.MusicVolume   = _MusicSlider.value;
            Context.RuntimeSettings.EffectsVolume = _EffectsSlider.value;

            Context.Audio.UpdateVolume();
        }

        private void OnFullScreenChanged(bool value)
        {
            Context.RuntimeSettings.Windowed = !value;
        }

        private void OnVSyncChanged(bool value)
        {
            Context.RuntimeSettings.VSync = value;
        }

        private void OnQualityChanged(float value)
        {
            Context.RuntimeSettings.GraphicsQuality = Mathf.RoundToInt(value);

            RefreshLabels();
        }

        private void OnDisplayChanged(float value)
        {
            int index = Mathf.RoundToInt(value);

            if (index >= 0 && index < _ValidResolutions.Count)
                Context.RuntimeSettings.Resolution = _ValidResolutions[index];
            RefreshLabels();
        }

        private void OnLanguageChanged(float value)
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
                return;
            List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
            int index            = Mathf.RoundToInt(value);

            if (index < 0 || index >= locales.Count)
                return;
            _PendingLocale = index;

            RefreshLabels();
        }

        #endregion
    }
}
