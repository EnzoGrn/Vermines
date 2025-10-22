using UnityEngine;

namespace Vermines.Core.Settings {

    using Vermines.Core.Settings;

    public class RuntimeSettings {

        #region Constants

        public const string KEY_MUSIC_VOLUME     = "MusicVolume";
        public const string KEY_EFFECTS_VOLUME   = "EffectsVolume";
        public const string KEY_WINDOWED         = "Windowed";
        public const string KEY_RESOLUTION       = "Resolution";
        public const string KEY_GRAPHICS_QUALITY = "GraphicsQuality";
        public const string KEY_LIMIT_FPS        = "LimitFPS";
        public const string KEY_TARGET_FPS       = "TargetFPS";
        public const string KEY_VSYNC            = "VSync";

        #endregion

        #region Attributes

        public Options Options => _Options;

        private Options _Options = new();

        #endregion

        #region Getters & Setters

        public float MusicVolume
        {
            get => _Options.GetFloat(KEY_MUSIC_VOLUME);
            set => _Options.Set(KEY_MUSIC_VOLUME, value, false);
        }

        public float EffectsVolume
        {
            get => _Options.GetFloat(KEY_EFFECTS_VOLUME);
            set => _Options.Set(KEY_EFFECTS_VOLUME, value, false);
        }

        public bool Windowed
        {
            get => _Options.GetBool(KEY_WINDOWED);
            set => _Options.Set(KEY_WINDOWED, value, false);
        }

        public int Resolution
        {
            get => _Options.GetInt(KEY_RESOLUTION);
            set => _Options.Set(KEY_RESOLUTION, value, false);
        }

        public int GraphicsQuality
        {
            get => _Options.GetInt(KEY_GRAPHICS_QUALITY);
            set => _Options.Set(KEY_GRAPHICS_QUALITY, value, false);
        }

        public bool VSync
        {
            get => _Options.GetBool(KEY_VSYNC);
            set => _Options.Set(KEY_VSYNC, value, false);
        }

        public bool LimitFPS
        {
            get => _Options.GetBool(KEY_LIMIT_FPS);
            set => _Options.Set(KEY_LIMIT_FPS, value, false);
        }

        public int TargetFPS
        {
            get => _Options.GetInt(KEY_TARGET_FPS);
            set => _Options.Set(KEY_TARGET_FPS, value, false);
        }

        #endregion

        #region Methods

        public void Initialize(GlobalSettings settings)
        {
            _Options.Initialize(settings.DefaultOptions, true, "Options.V3.");

            Windowed        = Screen.fullScreen == false;
            GraphicsQuality = QualitySettings.GetQualityLevel();
            Resolution      = GetCurrentResolutionIndex();

            QualitySettings.vSyncCount  = VSync    == true ? 1 : 0;
            Application.targetFrameRate = LimitFPS == true ? TargetFPS : -1;

            _Options.SaveChanges();
        }

        private int GetCurrentResolutionIndex()
        {
            Resolution[] resolutions = Screen.resolutions;

            if (resolutions == null || resolutions.Length == 0)
                return -1;
            int currentWidth       = Mathf.RoundToInt(Screen.width);
            int currentHeight      = Mathf.RoundToInt(Screen.height);
            int defaultRefreshRate = Mathf.RoundToInt((float)resolutions[^1].refreshRateRatio.value);

            for (int i = 0; i < resolutions.Length; i++) {
                var resolution = resolutions[i];

                if (resolution.width == currentWidth && resolution.height == currentHeight && Mathf.RoundToInt((float)resolution.refreshRateRatio.value) == defaultRefreshRate)
                    return i;
            }
            return -1;
        }

        #endregion
    }
}
