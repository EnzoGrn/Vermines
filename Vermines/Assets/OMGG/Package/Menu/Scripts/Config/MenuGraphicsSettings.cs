using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace OMGG.Menu.Configuration {

    public partial class MenuGraphicsSettings {

        /// <summary>
        /// Available framerates.
        /// -1 = platform default
        /// </summary>
        protected static int[] PossibleFramerates = new int[] {
             -1,
             30,
             60,
             75,
             90,
            120,
            144,
            165,
            240,
            360
        };

        /// <summary>
        /// Target framerate
        /// </summary>
        public virtual int Framerate
        {
            get
            {
                var f = PlayerPrefs.GetInt("OMGG.Config.Framerate", -1);

                if (PossibleFramerates.Contains(f) == false)
                    return PossibleFramerates[0];
                return f;
            }
            set => PlayerPrefs.SetInt("OMGG.Config.Framerate", value);
        }

        /// <summary>
        /// Fullscreen mode.
        /// Is not shown for mobile platforms.
        /// </summary>
        public virtual bool Fullscreen
        {
            get => PlayerPrefs.GetInt("OMGG.Config.Fullscreen", UnityEngine.Screen.fullScreen ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt("OMGG.Config.Fullscreen", value ? 1 : 0);
        }

        /// <summary>
        /// Selected resolution index based on <see cref="UnityEngine.Screen.resolutions"/>.
        /// Is not shown for mobile platforms.
        /// </summary>
        public virtual int Resolution
        {
            get => Math.Clamp(PlayerPrefs.GetInt("OMGG.Config.Resolution", GetCurrentResolutionIndex()), 0, Math.Max(0, UnityEngine.Screen.resolutions.Length - 1));
            set => PlayerPrefs.SetInt("OMGG.Config.Resolution", value);
        }

        /// <summary>
        /// Select VSync.
        /// </summary>
        public virtual bool VSync
        {
            get => PlayerPrefs.GetInt("OMGG.Config.VSync", Math.Clamp(QualitySettings.vSyncCount, 0, 1)) == 1;
            set => PlayerPrefs.SetInt("OMGG.Config.VSync", value ? 1 : 0);
        }

        /// <summary>
        /// Select Unity quality level index based on <see cref="QualitySettings.names"/>.
        /// </summary>
        public virtual int QualityLevel
        {
            get
            {
                var q = PlayerPrefs.GetInt("OMGG.Config.QualityLevel", QualitySettings.GetQualityLevel());

                q = Math.Clamp(q, 0, QualitySettings.names.Length - 1);

                return q;
            }
            set => PlayerPrefs.SetInt("OMGG.Config.QualityLevel", value);
        }

        /// <summary>
        /// Return a list of possible framerates filtered by <see cref="UnityEngine.Screen.currentResolution.refreshRate"/>.
        /// </summary>
        public virtual List<int> CreateFramerateOptions => PossibleFramerates.Where(f => f <=
            #if UNITY_2022_2_OR_NEWER
                (int)Math.Round(UnityEngine.Screen.currentResolution.refreshRateRatio.value)
            #else
                UnityEngine.Screen.currentResolution.refreshRate
            #endif
        ).ToList();

        /// <summary>
        /// Returns a list of resolution option indices based on <see cref="UnityEngine.Screen.resolutions" />.
        /// </summary>
        public virtual List<int> CreateResolutionOptions => Enumerable.Range(0, UnityEngine.Screen.resolutions.Length).ToList();

        /// <summary>
        /// Returns a list of graphics quality indices based on <see cref="QualitySettings.names"/>.
        /// </summary>
        public virtual List<int> CreateGraphicsQualityOptions => Enumerable.Range(0, QualitySettings.names.Length).ToList();

        /// <summary>
        /// A partial method to be implemented on the SDK level.
        /// </summary>
        partial void ApplyUser();

        /// <summary>
        /// Applies all graphics settings.
        /// </summary>
        public virtual void Apply()
        {
            #if !UNITY_IOS && !UNITY_ANDROID
                if (UnityEngine.Screen.resolutions.Length > 0) {
                    var resolution = UnityEngine.Screen.resolutions[Resolution < 0 ? UnityEngine.Screen.resolutions.Length - 1 : Resolution];

                    if (UnityEngine.Screen.currentResolution.width != resolution.width || UnityEngine.Screen.currentResolution.height != resolution.height || UnityEngine.Screen.fullScreen != Fullscreen)
                        UnityEngine.Screen.SetResolution(resolution.width, resolution.height, Fullscreen);
                }
            #endif

            if (QualitySettings.GetQualityLevel() != QualityLevel)
                QualitySettings.SetQualityLevel(QualityLevel);
            if (QualitySettings.vSyncCount != (VSync ? 1 : 0))
                QualitySettings.vSyncCount = VSync ? 1 : 0;
            if (Application.targetFrameRate != Framerate)
                Application.targetFrameRate = Framerate;
            ApplyUser();
        }

        /// <summary>
        /// Return the current selected resolution index based on <see cref="UnityEngine.Screen.resolutions"/>.
        /// </summary>
        /// <returns>Index into <see cref="UnityEngine.Screen.resolutions"/></returns>
        private int GetCurrentResolutionIndex()
        {
            var resolutions = UnityEngine.Screen.resolutions;

            if (resolutions == null || resolutions.Length == 0)
                return -1;
            int currentWidth  = Mathf.RoundToInt(UnityEngine.Screen.width);
            int currentHeight = Mathf.RoundToInt(UnityEngine.Screen.height);

            #if UNITY_2022_2_OR_NEWER
                var defaultRefreshRate = resolutions[^1].refreshRateRatio;
            #else
                var defaultRefreshRate = resolutions[^1].refreshRate;
            #endif

            for (int i = 0; i < resolutions.Length; i++) {
                var resolution = resolutions[i];

                if (resolution.width == currentWidth && resolution.height == currentHeight
                    #if UNITY_2022_2_OR_NEWER
                        && resolution.refreshRateRatio.denominator == defaultRefreshRate.denominator
                        && resolution.refreshRateRatio.numerator == defaultRefreshRate.numerator)
                    #else
                        && resolution.refreshRate == defaultRefreshRate)
                    #endif
                        return i;
            }
            return -1;
        }
    }
}
