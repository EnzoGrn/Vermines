using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;

namespace Vermines.Menu.Pluggin {

    using Vermines.Plugin.Sounds;
    using Vermines.UI;

    public class AudioSettingsPlugin : MenuScreenPlugin {

        private UserMixerPlugin _MixerManager;

        #region Settings Fields

        [Header("Settings Fields")]

        [Header(" - Sound")]

        [SerializeField]
        protected Toggle _SoundEnabledToggle;

        [SerializeField]
        protected SliderModule _SoundSlider;

        [Header(" - FX")]

        [SerializeField]
        protected Toggle _FXEnabledToggle;

        [SerializeField]
        protected SliderModule _FXSlider;

        [Header(" - Music")]

        [SerializeField]
        protected Toggle _MusicEnabledToggle;

        [SerializeField]
        protected SliderModule _MusicSlider;

        #endregion

        #region Override Methods

        public override void Init(MenuUIScreen screen)
        {
            base.Init(screen);

            _MixerManager = FindFirstObjectByType<UserMixerPlugin>(FindObjectsInactive.Include);

            OnMusicEnabled(_MixerManager.MusicEnabled);
            OnMusicVolumeChanged(_MixerManager.MusicVolume);
            _MusicEnabledToggle.SetIsOnWithoutNotify(_MixerManager.MusicEnabled);
            _MusicSlider.SetValue(_MixerManager.MusicVolume);

            _MixerManager.MusicEnabled = _MixerManager.MusicEnabled;
            _MixerManager.MusicVolume  = _MixerManager.MusicVolume;

            OnFXEnabled(_MixerManager.FXEnabled);
            OnFXVolumeChanged(_MixerManager.FXVolume);
            _FXEnabledToggle.SetIsOnWithoutNotify(_MixerManager.FXEnabled);
            _FXSlider.SetValue(_MixerManager.FXVolume);

            _MixerManager.FXVolume  = _MixerManager.FXVolume;
            _MixerManager.FXEnabled = _MixerManager.FXEnabled;

            OnSoundEnabled(_MixerManager.SoundEnabled);
            OnSoundVolumeChanged(_MixerManager.MainVolume);
            _SoundEnabledToggle.SetIsOnWithoutNotify(_MixerManager.SoundEnabled);
            _SoundSlider.SetValue(_MixerManager.MainVolume);

            _MixerManager.MainVolume   = _MixerManager.MainVolume;
            _MixerManager.SoundEnabled = _MixerManager.SoundEnabled;
        }

        public override void Show(MenuUIScreen screen)
        {
            base.Show(screen);

            _SoundSlider.OnValueChanged.AddListener(OnSoundVolumeChanged);
            _FXSlider.OnValueChanged.AddListener(OnFXVolumeChanged);
            _MusicSlider.OnValueChanged.AddListener(OnMusicVolumeChanged);
        }

        public override void Hide(MenuUIScreen screen)
        {
            base.Hide(screen);

            _SoundSlider.OnValueChanged.RemoveListener(OnSoundVolumeChanged);
            _FXSlider.OnValueChanged.RemoveListener(OnFXVolumeChanged);
            _MusicSlider.OnValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        #endregion

        #region Events

        #region Toggle

        public void OnSoundEnabled(bool isEnabled)
        {
            _MixerManager.SoundEnabled = isEnabled;

            _SoundSlider.Interactable        = isEnabled;
            _FXEnabledToggle.interactable    = isEnabled;
            _FXSlider.Interactable           = isEnabled;
            _MusicEnabledToggle.interactable = isEnabled;
            _MusicSlider.Interactable        = isEnabled;
        }

        public void OnFXEnabled(bool isEnabled)
        {
            _MixerManager.FXEnabled = isEnabled;

            _FXSlider.Interactable = isEnabled;
        }

        public void OnMusicEnabled(bool isEnabled)
        {
            _MixerManager.MusicEnabled = isEnabled;

            _MusicSlider.Interactable = isEnabled;
        }

        #endregion

        #region Slider

        public void OnSoundVolumeChanged(float value)
        {
            _MixerManager.MainVolume = value;
        }

        public void OnFXVolumeChanged(float value)
        {
            _MixerManager.FXVolume = value;
        }

        public void OnMusicVolumeChanged(float value)
        {
            _MixerManager.MusicVolume = value;
        }

        #endregion

        #endregion
    }
}
