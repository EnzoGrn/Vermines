using UnityEngine.UI;
using UnityEngine;

namespace Vermines.UI.Effects {

    /// <summary>
    /// Photo style adjustments (temperature, tint, brightness, contrast, tones, brilliance, saturation) on a UI graphic.
    /// Every setting goes from -100 to 100, 0 leaves the image unchanged. Each graphic gets its own material.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("Vermines/UI/Image Adjustments")]
    public class ImageAdjustments : MonoBehaviour, IMaterialModifier {

        #region Constants

        /// <summary>
        /// Name of the shader that applies the adjustments.
        /// </summary>
        private const string ShaderName = "Vermines/UI/Image Adjustments";

        private static readonly int TemperatureId = Shader.PropertyToID("_Temperature");
        private static readonly int TintId        = Shader.PropertyToID("_Tint");
        private static readonly int BrightnessId  = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId    = Shader.PropertyToID("_Contrast");
        private static readonly int HighlightsId  = Shader.PropertyToID("_Highlights");
        private static readonly int ShadowsId     = Shader.PropertyToID("_Shadows");
        private static readonly int WhitesId      = Shader.PropertyToID("_Whites");
        private static readonly int BlacksId      = Shader.PropertyToID("_Blacks");
        private static readonly int BrillianceId  = Shader.PropertyToID("_Brilliance");
        private static readonly int SaturationId  = Shader.PropertyToID("_Saturation");

        #endregion

        #region Attributes

        /// <summary>
        /// Blue (negative) to orange (positive).
        /// </summary>
        [Header("White Balance")]
        [Range(-100f, 100f)]
        [Tooltip("Blue (negative) to orange (positive).")]
        public float Temperature;

        /// <summary>
        /// Green (negative) to magenta (positive).
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Green (negative) to magenta (positive).")]
        public float Tint;

        /// <summary>
        /// Darkens or brightens the midtones, pure black and pure white are kept.
        /// </summary>
        [Header("Light")]
        [Range(-100f, 100f)]
        [Tooltip("Darkens or brightens the midtones, pure black and pure white are kept.")]
        public float Brightness;

        /// <summary>
        /// Spreads or squeezes the tones around middle grey.
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Spreads or squeezes the tones around middle grey.")]
        public float Contrast;

        /// <summary>
        /// Light tones.
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Light tones.")]
        public float Highlights;

        /// <summary>
        /// Dark tones.
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Dark tones.")]
        public float Shadows;

        /// <summary>
        /// Brightest tones.
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Brightest tones.")]
        public float Whites;

        /// <summary>
        /// Darkest tones.
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Darkest tones.")]
        public float Blacks;

        /// <summary>
        /// Lifts the shadows and pulls the highlights back to reveal details.
        /// </summary>
        [Range(-100f, 100f)]
        [Tooltip("Lifts the shadows and pulls the highlights back to reveal details.")]
        public float Brilliance;

        /// <summary>
        /// Colour intensity, -100 gives black and white.
        /// </summary>
        [Header("Color")]
        [Range(-100f, 100f)]
        [Tooltip("Colour intensity, -100 gives black and white.")]
        public float Saturation;

        /// <summary>
        /// Shader that applies the adjustments.
        /// </summary>
        [SerializeField]
        private Shader _Shader;

        /// <summary>
        /// Graphic the adjustments are applied to.
        /// </summary>
        private Graphic _Graphic;

        /// <summary>
        /// Material instance of this graphic.
        /// </summary>
        private Material _Material;

        #endregion

        #region MonoBehaviour Methods

        private void Reset()
        {
            _Shader = Shader.Find(ShaderName);
        }

        private void OnEnable()
        {
            if (_Shader == null)
                _Shader = Shader.Find(ShaderName);
            SetDirty();
        }

        private void OnDisable()
        {
            SetDirty();
        }

        private void OnDestroy()
        {
            if (_Material == null)
                return;
            if (Application.isPlaying)
                Destroy(_Material);
            else
                DestroyImmediate(_Material);
            _Material = null;
        }

        private void OnValidate()
        {
            SetDirty();
        }

        private void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies the current values, call it after changing them by script.
        /// </summary>
        public void SetDirty()
        {
            if (_Graphic == null)
                _Graphic = GetComponent<Graphic>();
            if (_Graphic != null)
                _Graphic.SetMaterialDirty();
        }

        #endregion

        #region IMaterialModifier

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled || _Shader == null)
                return baseMaterial;
            if (_Material == null)
                _Material = new Material(_Shader) { name = "Image Adjustments (Instance)", hideFlags = HideFlags.HideAndDontSave };

            // Keeps the stencil settings of masks and the texture of the base material.
            _Material.CopyPropertiesFromMaterial(baseMaterial);
            _Material.SetFloat(TemperatureId, Temperature);
            _Material.SetFloat(TintId, Tint);
            _Material.SetFloat(BrightnessId, Brightness);
            _Material.SetFloat(ContrastId, Contrast);
            _Material.SetFloat(HighlightsId, Highlights);
            _Material.SetFloat(ShadowsId, Shadows);
            _Material.SetFloat(WhitesId, Whites);
            _Material.SetFloat(BlacksId, Blacks);
            _Material.SetFloat(BrillianceId, Brilliance);
            _Material.SetFloat(SaturationId, Saturation);

            return _Material;
        }

        #endregion
    }
}
