using UnityEngine.Audio;
using UnityEngine;
using OMGG.Menu.Screen;
using System.Collections;
using Vermines.Sound;

namespace Vermines.Plugin.Sounds {

    public class UserMixerPlugin : MenuScreenPlugin {

        [SerializeField]
        private AudioMixer _AudioMixer;

        #region Main Sound

        public bool SoundEnabled
        {
            get => PlayerPrefs.GetInt("Vermines.Sound.Enabled", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("Vermines.Sound.Enabled", value ? 1 : 0);

                SetMainVolume();
                SetFXVolume();
                SetAmbianceVolume();
                SetMusicVolume();
            }
        }

        public float MainVolume
        {
            get => PlayerPrefs.GetFloat("Vermines.Sound.MainVolume", 0.5f);
            set
            {
                PlayerPrefs.SetFloat("Vermines.Sound.MainVolume", value);

                SetMainVolume();
            }
        }

        #endregion

        #region FX Sound

        public bool FXEnabled
        {
            get => PlayerPrefs.GetInt("Vermines.Sound.FXEnabled", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("Vermines.Sound.FXEnabled", value ? 1 : 0);

                SetFXVolume();
                SetAmbianceVolume();
            }
        }

        public float FXVolume
        {
            get => PlayerPrefs.GetFloat("Vermines.Sound.FXVolume", 0.5f);
            set
            {
                PlayerPrefs.SetFloat("Vermines.Sound.FXVolume", value);

                SetFXVolume();
                SetAmbianceVolume();
            }
        }

        public bool AmbianceEnabled
        {
            get => PlayerPrefs.GetInt("Vermines.Sound.AmbianceEnabled", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("Vermines.Sound.AmbianceEnabled", value ? 1 : 0);

                SetAmbianceVolume();
            }
        }

        public float AmbianceVolume
        {
            get => FXVolume;
            private set => FXVolume = value;
        }

        #endregion

        #region Music Sound

        public bool MusicEnabled
        {
            get => PlayerPrefs.GetInt("Vermines.Sound.MusicEnabled", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("Vermines.Sound.MusicEnabled", value ? 1 : 0);

                SetMusicVolume();
            }
        }

        public float MusicVolume
        {
            get => PlayerPrefs.GetFloat("Vermines.Sound.MusicVolume", 0.5f);
            set
            {
                PlayerPrefs.SetFloat("Vermines.Sound.MusicVolume", value);

                SetMusicVolume();
            }
        }

        #endregion

        #region Override Methods

        public void Start()
        {
            LoadSettings();
        }

        public override void Init(MenuUIScreen screen)
        {
            base.Init(screen);

            if (_AudioMixer == null) {
                Debug.LogError("[UserMixerPlugin]: Cannot set volume due to missing '_AudioMixer'.");

                return;
            }

            LoadSettings();
        }

        #endregion

        #region Methods

        public void LoadSettings()
        {
            SoundEnabled = SoundEnabled;
            MainVolume   = MainVolume;

            FXEnabled = FXEnabled;
            FXVolume  = FXVolume;

            AmbianceEnabled = AmbianceEnabled;

            MusicEnabled = MusicEnabled;
            MusicVolume  = MusicVolume;
        }

        public void SetMainVolume()
        {
            const string label = "MasterVolume";

            if (SoundEnabled)
                SetValue(label, MainVolume);
            else
                SetValue(label, 0.0f);
        }

        public void SetFXVolume()
        {
            const string label = "SoundFXVolume";

            if (SoundEnabled && FXEnabled)
                SetValue(label, FXVolume);
            else
                SetValue(label, 0.0f);
        }

        public void SetAmbianceVolume()
        {
            const string label = "AtmosphereVolume";

            if (SoundEnabled && FXEnabled && AmbianceEnabled)
                SetValue(label, AmbianceVolume);
            else
                SetValue(label, 0.0f);
        }

        public void SetMusicVolume()
        {
            const string label = "MusicVolume";

            Debug.Log($"Setting Music Volume label: {label} SoundEnable {SoundEnabled} && MusicEnable {MusicEnabled}");

            if (SoundEnabled && MusicEnabled)
                SetValue(label, MusicVolume);
            else
                SetValue(label, 0.0f);
        }

        private void SetValue(string label, float value)
        {
            Debug.Log($"[UserMixerPlugin]: Set '{label}' to {value}");
            if (value <= 0f)
                value = 0.0001f; // Avoid Mathf.Log10(0) error
            _AudioMixer.SetFloat(label, Mathf.Log10(value) * 20f);
        }

        #endregion
    }
}
