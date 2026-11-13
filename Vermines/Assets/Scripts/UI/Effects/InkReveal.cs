using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI.Effects {

    /// <summary>
    /// Draws a UI graphic like an ink blot spreading on the parchment (shader "Vermines/UI/Ink Reveal").
    /// Reveal() and Fade() return tweens so an owner (e.g. PausePanelAnimator) can place them in its sequence.
    /// Each graphic gets its own material, masks keep working (IMaterialModifier).
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("Vermines/UI/Ink Reveal")]
    public class InkReveal : MonoBehaviour, IMaterialModifier {

        #region Constants

        private const string ShaderName = "Vermines/UI/Ink Reveal";

        private static readonly int ProgressId      = Shader.PropertyToID("_Progress");
        private static readonly int OriginId        = Shader.PropertyToID("_Origin");
        private static readonly int SoftnessId      = Shader.PropertyToID("_Softness");
        private static readonly int NoiseScaleId    = Shader.PropertyToID("_NoiseScale");
        private static readonly int NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        private static readonly int WetColorId      = Shader.PropertyToID("_WetColor");
        private static readonly int WetWidthId      = Shader.PropertyToID("_WetWidth");
        private static readonly int SeedId          = Shader.PropertyToID("_Seed");
        private static readonly int WarpId          = Shader.PropertyToID("_Warp");
        private static readonly int InkColorId      = Shader.PropertyToID("_InkColor");
        private static readonly int DensityId       = Shader.PropertyToID("_Density");
        private static readonly int DensityScaleId  = Shader.PropertyToID("_DensityScale");
        private static readonly int GrainId         = Shader.PropertyToID("_Grain");
        private static readonly int GrainScaleId    = Shader.PropertyToID("_GrainScale");
        private static readonly int PoolingId       = Shader.PropertyToID("_Pooling");
        private static readonly int PoolRadiusId    = Shader.PropertyToID("_PoolRadius");

        #endregion

        #region Attributes

        [Header("State")]
        [Range(0f, 1f)]
        [Tooltip("0 = nothing drawn, 1 = fully drawn. Editable in the inspector to preview the blot.")]
        [SerializeField] private float _Progress = 1f;

        [Header("Blot")]
        [Tooltip("Where the drop lands, in UV (0.5, 0.5 = centre).")]
        [SerializeField] private Vector2 _Origin = new(0.5f, 0.55f);
        [Range(0.005f, 0.3f)]
        [SerializeField] private float _Softness = 0.06f;
        [SerializeField] private float _NoiseScale = 5f;
        [Range(0f, 0.6f)]
        [SerializeField] private float _NoiseStrength = 0.3f;
        [Tooltip("Colour of the wet ink at the front, alpha = how strong.")]
        [SerializeField] private Color _WetColor = new(0.12f, 0.08f, 0.06f, 0.85f);
        [Range(0f, 0.4f)]
        [SerializeField] private float _WetWidth = 0.14f;
        [Tooltip("Bends the front into lobes and fingers. 0 = round blot.")]
        [Range(0f, 0.6f)]
        [SerializeField] private float _Warp = 0.25f;

        [Header("Ink body (stays after the reveal)")]
        [Tooltip("Colour the ink darkens toward, alpha = how strong.")]
        [SerializeField] private Color _InkColor = new(0.16f, 0.11f, 0.08f, 0.9f);
        [Tooltip("Large blotches where the ink is heavier or thinner.")]
        [Range(0f, 1f)]
        [SerializeField] private float _Density = 0.5f;
        [SerializeField] private float _DensityScale = 3f;
        [Tooltip("Fine granulation of the paper.")]
        [Range(0f, 1f)]
        [SerializeField] private float _Grain = 0.35f;
        [SerializeField] private float _GrainScale = 120f;
        [Tooltip("Darker ink gathered along the edges of the drawing.")]
        [Range(0f, 1f)]
        [SerializeField] private float _Pooling = 0.6f;
        [Tooltip("How far from the edges the pooling reaches, in UV (0.02 = 2% of the sprite).")]
        [Range(0.002f, 0.08f)]
        [SerializeField] private float _PoolRadius = 0.02f;

        [Tooltip("New blot shape on every reveal.")]
        [SerializeField] private bool _RandomSeed = true;
        [SerializeField] private float _Seed;

        [Header("Timing")]
        [SerializeField] private float _RevealDuration = 0.9f;
        [SerializeField] private Ease _RevealEase = Ease.OutCubic;
        [SerializeField] private float _FadeDuration = 0.2f;

        [SerializeField] private Shader _Shader;

        private Graphic _Graphic;
        private Material _Material;

        #endregion

        #region Getters & Setters

        public float Progress
        {
            get => _Progress;
            set
            {
                _Progress = Mathf.Clamp01(value);

                SetDirty();
            }
        }

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

        #endregion

        #region Methods

        /// <summary>
        /// Instant state, no animation.
        /// </summary>
        public void Snap(bool visible)
        {
            Progress = visible ? 1f : 0f;
        }

        /// <summary>
        /// Hides the graphic right away and returns the tween that draws it. Not started by a target:
        /// insert it in a sequence, or call Play() on it.
        /// </summary>
        public Tween Reveal()
        {
            if (_RandomSeed)
                _Seed = Random.Range(0f, 100f);
            Progress = 0f;

            return DOTween.To(() => _Progress, v => Progress = v, 1f, _RevealDuration).SetEase(_RevealEase);
        }

        /// <summary>
        /// Quick retreat of the blot, for closing.
        /// </summary>
        public Tween Fade()
        {
            return DOTween.To(() => _Progress, v => Progress = v, 0f, _FadeDuration).SetEase(Ease.InQuad);
        }

        private void SetDirty()
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
                _Material = new Material(_Shader) { name = "Ink Reveal (Instance)", hideFlags = HideFlags.HideAndDontSave };

            // Keeps the stencil settings of masks and the texture of the base material.
            _Material.CopyPropertiesFromMaterial(baseMaterial);
            _Material.SetFloat(ProgressId, _Progress);
            _Material.SetVector(OriginId, _Origin);
            _Material.SetFloat(SoftnessId, _Softness);
            _Material.SetFloat(NoiseScaleId, _NoiseScale);
            _Material.SetFloat(NoiseStrengthId, _NoiseStrength);
            _Material.SetColor(WetColorId, _WetColor);
            _Material.SetFloat(WetWidthId, _WetWidth);
            _Material.SetFloat(SeedId, _Seed);
            _Material.SetFloat(WarpId, _Warp);
            _Material.SetColor(InkColorId, _InkColor);
            _Material.SetFloat(DensityId, _Density);
            _Material.SetFloat(DensityScaleId, _DensityScale);
            _Material.SetFloat(GrainId, _Grain);
            _Material.SetFloat(GrainScaleId, _GrainScale);
            _Material.SetFloat(PoolingId, _Pooling);
            _Material.SetFloat(PoolRadiusId, _PoolRadius);

            return _Material;
        }

        #endregion
    }
}
