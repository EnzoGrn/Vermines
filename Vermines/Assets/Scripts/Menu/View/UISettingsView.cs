    using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Vermines.UI {

    using Vermines.Core.Settings;
    using Vermines.UI.Core;
    using Vermines.Extension;
    using Vermines.Utils;
    using Vermines.Core.UI;
    using Vermines.Menu.View;
    using System.Collections;

    public class UISettingsView : UICloseView {

        #region Private Class

        private struct ResolutionData {

            public int Index;

            public Resolution Resolution;

            public ResolutionData(int index, Resolution resolution)
            {
                Index      = index;
                Resolution = resolution;
            }
        };

        #endregion

        #region Attributes

        [SerializeField]
        private List<BookCategories> _Categories;

        private BookCategories _CurrentSelectedCategory;

        [SerializeField]
        private GameObject _Content;

        private List<ResolutionData> _ValidResolutions = new(32);

        [SerializeField]
        private SliderModule _MusicVolumeSlider;

        [SerializeField]
        private SliderModule _EffectsVolumeSlider;

        [SerializeField]
        private TMP_Dropdown _GraphicsQuality;

        [SerializeField]
        private TMP_Dropdown _Resolution;

        [SerializeField]
        private UIToggle _VSync;

        [SerializeField]
        private UIToggle _LimitFPS;

        [SerializeField]
        private SliderModule _TargetFPS;

        [SerializeField]
        private int[] _TargetFPSValues;

        [SerializeField]
        private UIToggle _Windowed;

        [SerializeField]
        private UIButton _ConfirmButton;

        [SerializeField]
        private UIButton _ResetButton;

        private Coroutine _ShowCoroutine;
        private Coroutine _HideCoroutine;

        protected static readonly int ShowAnimHash = Animator.StringToHash("Show");
        protected static readonly int HideAnimHash = Animator.StringToHash("Hide");

        #endregion

        #region Methods

        private void LoadValues()
        {
            RuntimeSettings runtimeSettings = Context.RuntimeSettings;

            _MusicVolumeSlider.SetOptionsValueFloat(runtimeSettings.Options.GetValue(RuntimeSettings.KEY_MUSIC_VOLUME));
            _EffectsVolumeSlider.SetOptionsValueFloat(runtimeSettings.Options.GetValue(RuntimeSettings.KEY_EFFECTS_VOLUME));

            _Windowed.SetIsOnWithoutNotify(runtimeSettings.Windowed);
            _GraphicsQuality.SetValueWithoutNotify(runtimeSettings.GraphicsQuality);
            _Resolution.SetValueWithoutNotify(_ValidResolutions.FindIndex(t => t.Index == runtimeSettings.Resolution));
            _TargetFPS.SetOptionsValueInt(runtimeSettings.Options.GetValue(RuntimeSettings.KEY_TARGET_FPS));
            _LimitFPS.SetIsOnWithoutNotify(runtimeSettings.LimitFPS);
            _VSync.SetIsOnWithoutNotify(runtimeSettings.VSync);
        }

        private void PrepareResolutionDropdown()
        {
            _ValidResolutions.Clear();

            Resolution[] resolutions = UnityEngine.Screen.resolutions;

            int defaultRefreshRate = Mathf.RoundToInt((float)resolutions[^1].refreshRateRatio.value);

            // Add resolutions in reversed order
            for (int i = resolutions.Length - 1; i >= 0; i--) {
                Resolution resolution = resolutions[i];

                if (Mathf.RoundToInt((float)resolution.refreshRateRatio.value) != defaultRefreshRate)
                    continue;
                _ValidResolutions.Add(new ResolutionData(i, resolution));
            }

            var options = ListPool.Get<TMP_Dropdown.OptionData>(16);

            for (int i = 0; i < _ValidResolutions.Count; i++) {
                Resolution resolution = _ValidResolutions[i].Resolution;

                options.Add(new TMP_Dropdown.OptionData($"{resolution.width} x {resolution.height}"));
            }

            _Resolution.ClearOptions();
            _Resolution.AddOptions(options);

            ListPool.Return(options);
        }

        public void SelectCategory(BookCategories category)
        {
            if (_CurrentSelectedCategory != null)
                _CurrentSelectedCategory.SetActiveCategorie(false);
            _CurrentSelectedCategory = category;

            _CurrentSelectedCategory.SetActiveCategorie(true);
        }

        #endregion

        #region Event

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _Content?.SetActive(gameObject.activeSelf);

            foreach (BookCategories category in _Categories)
                category.SetActiveCategorie(false);
            _ConfirmButton.onClick.AddListener(OnConfirmButton);
            _ResetButton.onClick.AddListener(OnResetButton);

            _MusicVolumeSlider.OnValueChanged.AddListener(OnVolumeChanged);
            _EffectsVolumeSlider.OnValueChanged.AddListener(OnVolumeChanged);

            _GraphicsQuality.onValueChanged.AddListener(OnGraphicsChanged);
            _Resolution.onValueChanged.AddListener(OnGraphicsChanged);
            _TargetFPS.OnValueChanged.AddListener(OnTargetFPSChanged);
            _LimitFPS.onValueChanged.AddListener(OnLimitFPSChanged);
            _VSync.onValueChanged.AddListener(OnLimitFPSChanged);

            _Windowed.onValueChanged.AddListener(OnWindowedChanged);
        }

        protected override void OnDeinitialize()
        {
            _ConfirmButton.onClick.RemoveListener(OnConfirmButton);
            _ResetButton.onClick.RemoveListener(OnResetButton);

            _MusicVolumeSlider.OnValueChanged.RemoveListener(OnVolumeChanged);
            _EffectsVolumeSlider.OnValueChanged.RemoveListener(OnVolumeChanged);

            _GraphicsQuality.onValueChanged.RemoveListener(OnGraphicsChanged);
            _Resolution.onValueChanged.RemoveListener(OnGraphicsChanged);
            _TargetFPS.OnValueChanged.RemoveListener(OnTargetFPSChanged);
            _LimitFPS.onValueChanged.RemoveListener(OnLimitFPSChanged);
            _VSync.onValueChanged.RemoveListener(OnLimitFPSChanged);

            _Windowed.onValueChanged.RemoveListener(OnWindowedChanged);

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            UIMainMenuView menu = SceneUI.Get<UIMainMenuView>();

            menu?.DeactiveButton();

            StartCoroutine(ShowCoroutine());
        }

        private IEnumerator ShowCoroutine()
        {
            if (Animator && gameObject.activeSelf) {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);

                if (_ShowCoroutine != null)
                    StopCoroutine(_ShowCoroutine);

                _ShowCoroutine = StartCoroutine(ShowAnimCoroutine());

                yield return _ShowCoroutine;

                _ShowCoroutine = null;
            }

            yield break;
        }

        private IEnumerator ShowAnimCoroutine()
        {
            Animator.Play(ShowAnimHash);

            yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == ShowAnimHash);

            while (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
            base.OnOpen();

            PrepareResolutionDropdown();
            LoadValues();

            _Content?.SetActive(true);

            SelectCategory(_Categories[0]);
            foreach (BookCategories categories in _Categories)
                categories.Open();
        }

        protected override void OnClose()
        {
            UIMainMenuView menu = SceneUI.Get<UIMainMenuView>();

            menu?.ActiveButton();

            StartCoroutine(CloseCoroutine());
        }

        private IEnumerator CloseCoroutine()
        {
            if (Animator && gameObject.activeSelf) {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(HideAnimCoroutine());

                yield return _HideCoroutine;

                _HideCoroutine = null;
            }

            yield break;
        }

        private IEnumerator HideAnimCoroutine()
        {
            Animator.Play(HideAnimHash);

            yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HideAnimHash);

            while (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
            foreach (BookCategories categories in _Categories)
                categories.Close();
            Context.RuntimeSettings.Options.DiscardChanges();
            Context.Audio.UpdateVolume();

            base.OnCloseButton();
        }

        protected override void OnTick()
        {
            base.OnTick();

            _ConfirmButton.interactable = Context.RuntimeSettings.Options.HasUnsavedChanges;

            _LimitFPS.SetActive(!_VSync.isOn);
            _TargetFPS.SetActive(!_VSync.isOn && _LimitFPS.isOn);
        }

        private void OnConfirmButton()
        {
            RuntimeSettings runtimeSettings = Context.RuntimeSettings;

            runtimeSettings.Options.SaveChanges();

            if (!Application.isMobilePlatform || Application.isEditor) {
                Resolution resolution = UnityEngine.Screen.resolutions[runtimeSettings.Resolution < 0 ? UnityEngine.Screen.resolutions.Length - 1 : runtimeSettings.Resolution];

                UnityEngine.Screen.SetResolution(resolution.width, resolution.height, !_Windowed.isOn);
            }

            QualitySettings.SetQualityLevel(runtimeSettings.GraphicsQuality);

            Application.targetFrameRate = runtimeSettings.LimitFPS ? runtimeSettings.TargetFPS : -1;
            QualitySettings.vSyncCount  = runtimeSettings.VSync    ? 1 : 0;

            OnCloseButton();
        }

        private void OnResetButton()
        {
            Options options = Context.RuntimeSettings.Options;

            options.ResetValueToDefault(RuntimeSettings.KEY_EFFECTS_VOLUME  , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_MUSIC_VOLUME    , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_GRAPHICS_QUALITY, false);
            options.ResetValueToDefault(RuntimeSettings.KEY_RESOLUTION      , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_WINDOWED        , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_LIMIT_FPS       , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_TARGET_FPS      , false);
            options.ResetValueToDefault(RuntimeSettings.KEY_VSYNC           , false);

            LoadValues();

            Context.Audio.UpdateVolume();
        }

        protected override void OnCloseButton()
        {
            _Content?.SetActive(false);

            StartCoroutine(CloseCoroutine());
        }

        private void OnVolumeChanged(float value)
        {
            Context.RuntimeSettings.MusicVolume   = _MusicVolumeSlider.Slider.value;
            Context.RuntimeSettings.EffectsVolume = _EffectsVolumeSlider.Slider.value;

            Context.Audio.UpdateVolume();
        }

        private void OnLimitFPSChanged(bool value)
        {
            OnGraphicsChanged(-1);
        }

        private void OnTargetFPSChanged(float value)
        {
            OnGraphicsChanged(-1);
        }

        private void OnGraphicsChanged(int value)
        {
            RuntimeSettings runtimeSettings = Context.RuntimeSettings;

            runtimeSettings.GraphicsQuality = _GraphicsQuality.value;
            runtimeSettings.Resolution      = _ValidResolutions[_Resolution.value].Index;
            runtimeSettings.TargetFPS       = Mathf.RoundToInt(_TargetFPS.Slider.value);
            runtimeSettings.LimitFPS        = _LimitFPS.isOn;
            runtimeSettings.VSync           = _VSync.isOn;
        }

        private void OnWindowedChanged(bool value)
        {
            Context.RuntimeSettings.Windowed = value;
        }

        #endregion
    }
}
