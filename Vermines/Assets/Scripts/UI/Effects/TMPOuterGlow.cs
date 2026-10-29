using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.UI.Effects {

    /// <summary>
    /// Reproduces the Photoshop "Outer Glow" layer style on a TextMeshPro UGUI text.
    /// The text shape is baked into a small render texture and blurred,
    /// then a hidden sibling graphic draws it right behind the text with the "Vermines/UI/TMP Outer Glow" shader.
    /// Sizes are in local units of the text, which are canvas pixels when the text is not scaled.
    /// </summary>
    [ExecuteAlways][DisallowMultipleComponent][RequireComponent(typeof(TextMeshProUGUI))][AddComponentMenu("Vermines/UI/TMP Outer Glow")]
    public class TMPOuterGlow : MonoBehaviour {

        #region Constants

        /// <summary>
        /// Width of the gradient and contour lookup textures.
        /// </summary>
        private const int RampResolution = 256;

        /// <summary>
        /// Largest side of the baked mask, in texels.
        /// </summary>
        private const int MaxMaskSize = 2048;

        /// <summary>
        /// Pass of the bake shader that draws the text coverage.
        /// </summary>
        private const int MaskPass = 0;

        /// <summary>
        /// Pass of the bake shader that blurs horizontally.
        /// </summary>
        private const int BlurHorizontalPass = 1;

        /// <summary>
        /// Pass of the bake shader that blurs vertically.
        /// </summary>
        private const int BlurVerticalPass = 2;

        /// <summary>
        /// Name of the shader that draws the glow.
        /// </summary>
        private const string GlowShaderName = "Vermines/UI/TMP Outer Glow";

        /// <summary>
        /// Name of the shader that bakes and blurs the text mask.
        /// </summary>
        private const string BakeShaderName = "Hidden/Vermines/TMP Outer Glow Bake";

        private static readonly int MainTexId            = Shader.PropertyToID("_MainTex");
        private static readonly int BakeRectId           = Shader.PropertyToID("_BakeRect");
        private static readonly int BlurSigmaId          = Shader.PropertyToID("_BlurSigma");
        private static readonly int GlowGradientId       = Shader.PropertyToID("_GlowGradient");
        private static readonly int GlowContourId        = Shader.PropertyToID("_GlowContour");
        private static readonly int GlowOpacityId        = Shader.PropertyToID("_GlowOpacity");
        private static readonly int GlowNoiseId          = Shader.PropertyToID("_GlowNoise");
        private static readonly int GlowSpreadId         = Shader.PropertyToID("_GlowSpread");
        private static readonly int GlowRangeId          = Shader.PropertyToID("_GlowRange");
        private static readonly int GlowJitterId         = Shader.PropertyToID("_GlowJitter");
        private static readonly int GlowIntensityId      = Shader.PropertyToID("_GlowIntensity");
        private static readonly int GradientSmoothnessId = Shader.PropertyToID("_GradientSmoothness");

        #endregion

        #region Attributes

        [Header("Structure")]

        [SerializeField][Range(0f, 1f)]
        private float _Opacity = 0.64f;

        [SerializeField][Range(0f, 1f)]
        private float _Noise = 0f;

        /// <summary>
        /// Gradient: left is the outer edge of the glow, right is against the text (as in Photoshop).
        /// </summary>
        [SerializeField][Tooltip("Gradient: left = outer edge of the glow, right = against the text. Alpha keys are the opacity stops.")]
        private Gradient _Gradient = CreateDefaultGradient();

        [SerializeField][Range(0f, 1f)]
        private float _GradientSmoothness = 1f;

        [Header("Elements")]

        [SerializeField][Range(0f, 0.99f)]
        private float _Spread = 0f;

        /// <summary>
        /// "Size", in local units of the text (canvas pixels).
        /// </summary>
        [SerializeField][Min(0f)][Tooltip("Size, in canvas pixels.")]
        private float _Size = 51f;

        [Header("Quality")]

        /// <summary>
        /// "Contour": x is the glow strength before the contour, y after it.
        /// </summary>
        [SerializeField][Tooltip("Contour: the default straight line is the 'Linear' contour.")]
        private AnimationCurve _Contour = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField][Range(0.01f, 1f)]
        private float _Range = 0.61f;

        [SerializeField][Range(0f, 1f)]
        private float _Jitter = 0.51f;

        [Header("Advanced")]

        /// <summary>
        /// Gain applied to the blurred text before the contour, 2 makes the glow fully opaque along a straight edge.
        /// </summary>
        [SerializeField][Range(0f, 8f)][Tooltip("Strength of the glow before the contour. 2 matches Photoshop on thick shapes, raise it for thin texts.")]
        private float _Intensity = 2f;

        /// <summary>
        /// Texels of the baked mask per blur standard deviation.
        /// </summary>
        [SerializeField][Range(2f, 16f)][Tooltip("Resolution of the baked glow. Higher is smoother but costs more memory.")]
        private float _Resolution = 6f;

        /// <summary>
        /// Rebakes the glow every frame, for texts whose vertices are animated by script.
        /// </summary>
        [SerializeField][Tooltip("Rebake every frame. Only needed when the text vertices are animated by script.")]
        private bool _UpdateEveryFrame = false;

        /// <summary>
        /// Shader that draws the glow.
        /// </summary>
        [SerializeField]
        private Shader _GlowShader;

        /// <summary>
        /// Shader that bakes and blurs the text mask.
        /// </summary>
        [SerializeField]
        private Shader _BakeShader;

        /// <summary>
        /// Text the glow is applied to.
        /// </summary>
        private TextMeshProUGUI _Text;

        /// <summary>
        /// Hidden graphic that draws the glow behind the text.
        /// </summary>
        private TMPOuterGlowGraphic _Graphic;

        /// <summary>
        /// Instance of the glow shader used by the graphic.
        /// </summary>
        private Material _GlowMaterial;

        /// <summary>
        /// Instance of the bake shader.
        /// </summary>
        private Material _BakeMaterial;

        /// <summary>
        /// Lookup texture of the gradient.
        /// </summary>
        private Texture2D _GradientTexture;

        /// <summary>
        /// Lookup texture of the contour.
        /// </summary>
        private Texture2D _ContourTexture;

        /// <summary>
        /// Text mask, holds the final blurred result.
        /// </summary>
        private RenderTexture _MaskTexture;

        /// <summary>
        /// Intermediate texture of the separable blur.
        /// </summary>
        private RenderTexture _BlurTexture;

        /// <summary>
        /// Commands that bake and blur the mask.
        /// </summary>
        private CommandBuffer _CommandBuffer;

        /// <summary>
        /// Per mesh properties (font atlas) of the mask pass.
        /// </summary>
        private MaterialPropertyBlock _PropertyBlock;

        /// <summary>
        /// True when the mask must be baked again.
        /// </summary>
        private bool _IsMaskDirty;

        /// <summary>
        /// True when the material and the lookup textures must be updated.
        /// </summary>
        private bool _IsMaterialDirty;

        #endregion

        #region MonoBehaviour Methods

        private void Reset()
        {
            _GlowShader = Shader.Find(GlowShaderName);
            _BakeShader = Shader.Find(BakeShaderName);
        }

        private void OnEnable()
        {
            _Text = GetComponent<TextMeshProUGUI>();

            if (_GlowShader == null)
                _GlowShader = Shader.Find(GlowShaderName);
            if (_BakeShader == null)
                _BakeShader = Shader.Find(BakeShaderName);
            if (_GlowShader == null || _BakeShader == null) {
                Debug.LogWarning($"[TMPOuterGlow] Missing shaders on '{name}', the glow is disabled.", this);

                return;
            }

            CreateResources();

            _IsMaskDirty = true;
            _IsMaterialDirty = true;

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);

            Canvas.willRenderCanvases += OnWillRenderCanvases;
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

            Canvas.willRenderCanvases -= OnWillRenderCanvases;

            ReleaseResources();
        }

        private void OnValidate()
        {
            _IsMaskDirty     = true;
            _IsMaterialDirty = true;

            if (_Graphic != null)
                _Graphic.SetAllDirty();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rebakes the glow on the next frame, call it after editing the text vertices by script.
        /// </summary>
        public void Refresh()
        {
            _IsMaskDirty = true;
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new();

            gradient.SetKeys(new GradientColorKey[] {
                new(Color.black, 0f), new(Color.white, 1f)
            }, new GradientAlphaKey[] {
                new(1f, 0f), new(1f, 1f)
            });

            return gradient;
        }

        private void CreateResources()
        {
            _GlowMaterial = new Material(_GlowShader) {
                hideFlags = HideFlags.HideAndDontSave
            };

            _BakeMaterial = new Material(_BakeShader) {
                hideFlags = HideFlags.HideAndDontSave
            };

            _GradientTexture = CreateRampTexture("TMP Outer Glow Gradient", false);
            _ContourTexture  = CreateRampTexture("TMP Outer Glow Contour" , true);

            _CommandBuffer = new CommandBuffer {
                name = "TMP Outer Glow"
            };

            _PropertyBlock = new MaterialPropertyBlock();

            GameObject glowObject = new("TMP Outer Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(LayoutElement), typeof(TMPOuterGlowGraphic));

            glowObject.hideFlags = HideFlags.HideAndDontSave;
            glowObject.layer     = gameObject.layer;
            glowObject.GetComponent<LayoutElement>().ignoreLayout = true;

            _Graphic               = glowObject.GetComponent<TMPOuterGlowGraphic>();
            _Graphic.raycastTarget = false;
            _Graphic.material      = _GlowMaterial;

            SyncGraphicTransform();
        }

        private void ReleaseResources()
        {
            if (_Graphic != null)
                DestroyObj(_Graphic.gameObject);
            DestroyObj(_GlowMaterial);
            DestroyObj(_BakeMaterial);
            DestroyObj(_GradientTexture);
            DestroyObj(_ContourTexture);
            ReleaseRenderTexture(ref _MaskTexture);
            ReleaseRenderTexture(ref _BlurTexture);

            _CommandBuffer?.Release();

            _Graphic         = null;
            _GlowMaterial    = null;
            _BakeMaterial    = null;
            _GradientTexture = null;
            _ContourTexture  = null;
            _CommandBuffer   = null;
        }

        private static Texture2D CreateRampTexture(string textureName, bool linear)
        {
            return new Texture2D(RampResolution, 1, TextureFormat.RGBA32, false, linear) {
                name       = textureName,
                hideFlags  = HideFlags.HideAndDontSave,
                wrapMode   = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        private static void DestroyObj(Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying) {
                Destroy(target);

                return;
            }

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () => {
                    if (target != null)
                        DestroyImmediate(target);
                };
            #else
                DestroyImmediate(target);
            #endif
        }

        private static void ReleaseRenderTexture(ref RenderTexture texture)
        {
            if (texture == null)
                return;
            texture.Release();

            DestroyObj(texture);

            texture = null;
        }

        /// <summary>
        /// Keeps the glow graphic right before the text in the hierarchy, with the same transform.
        /// </summary>
        private void SyncGraphicTransform()
        {
            RectTransform textTransform = _Text.rectTransform;
            RectTransform glowTransform = _Graphic.rectTransform;

            if (textTransform.parent == null)
                return;
            if (glowTransform.parent != textTransform.parent)
                glowTransform.SetParent(textTransform.parent, false);
            int textIndex = textTransform.GetSiblingIndex();
            int glowIndex = glowTransform.GetSiblingIndex();

            if (glowIndex != textIndex - 1)
                glowTransform.SetSiblingIndex(glowIndex < textIndex ? textIndex - 1 : textIndex);
            if (glowTransform.anchorMin != textTransform.anchorMin)
                glowTransform.anchorMin = textTransform.anchorMin;
            if (glowTransform.anchorMax != textTransform.anchorMax)
                glowTransform.anchorMax = textTransform.anchorMax;
            if (glowTransform.pivot != textTransform.pivot)
                glowTransform.pivot = textTransform.pivot;
            if (glowTransform.sizeDelta != textTransform.sizeDelta)
                glowTransform.sizeDelta = textTransform.sizeDelta;
            if (glowTransform.localPosition != textTransform.localPosition)
                glowTransform.localPosition = textTransform.localPosition;
            if (glowTransform.localRotation != textTransform.localRotation)
                glowTransform.localRotation = textTransform.localRotation;
            if (glowTransform.localScale != textTransform.localScale)
                glowTransform.localScale = textTransform.localScale;
            if (_Graphic.enabled != _Text.enabled)
                _Graphic.enabled = _Text.enabled;
        }

        private void UpdateMaterial()
        {
            Color[] gradientPixels = new Color[RampResolution];
            Color[] contourPixels  = new Color[RampResolution];

            for (int i = 0; i < RampResolution; i++) {
                float position = i / (float)(RampResolution - 1);
                float contour  = Mathf.Clamp01(_Contour != null ? _Contour.Evaluate(position) : position);

                gradientPixels[i] = _Gradient != null ? _Gradient.Evaluate(position) : Color.white;
                contourPixels[i]  = new Color(contour, contour, contour, 1f);
            }

            _GradientTexture.SetPixels(gradientPixels);
            _GradientTexture.Apply(false);
            _ContourTexture.SetPixels(contourPixels);
            _ContourTexture.Apply(false);

            _GlowMaterial.SetTexture(GlowGradientId    , _GradientTexture);
            _GlowMaterial.SetTexture(GlowContourId     , _ContourTexture);
            _GlowMaterial.SetFloat(GlowOpacityId       , _Opacity);
            _GlowMaterial.SetFloat(GlowNoiseId         , _Noise);
            _GlowMaterial.SetFloat(GlowSpreadId        , _Spread);
            _GlowMaterial.SetFloat(GlowRangeId         , _Range);
            _GlowMaterial.SetFloat(GlowJitterId        , _Jitter);
            _GlowMaterial.SetFloat(GlowIntensityId     , _Intensity);
            _GlowMaterial.SetFloat(GradientSmoothnessId, _GradientSmoothness);
        }

        /// <summary>
        /// Draws the coverage of every text mesh into the mask, then blurs it.
        /// </summary>
        private void BakeMask()
        {
            TMP_TextInfo textInfo = _Text.textInfo;

            if (textInfo == null || textInfo.meshInfo == null || textInfo.characterCount == 0) {
                _Graphic.SetGlow(null, Rect.zero);

                return;
            }

            int meshCount = Mathf.Min(textInfo.materialCount, textInfo.meshInfo.Length);

            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);

            for (int i = 0; i < meshCount; i++) {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

                if (meshInfo.vertices == null)
                    continue;
                int vertexCount = Mathf.Min(meshInfo.vertexCount, meshInfo.vertices.Length);

                for (int v = 0; v < vertexCount; v++) {
                    min = Vector2.Min(min, meshInfo.vertices[v]);
                    max = Vector2.Max(max, meshInfo.vertices[v]);
                }
            }

            if (min.x > max.x || min.y > max.y) {
                _Graphic.SetGlow(null, Rect.zero);

                return;
            }

            float        sigma  = Mathf.Max(_Size * (1f - _Spread), 0.5f) / 2f;
            float        margin = _Size * 1.5f + 2f;
            Rect       glowRect = Rect.MinMaxRect(min.x - margin, min.y - margin, max.x + margin, max.y + margin);
            float texelsPerUnit = Mathf.Min(_Resolution / sigma, 2f, MaxMaskSize / Mathf.Max(glowRect.width, glowRect.height));
            int           width = Mathf.Clamp(Mathf.CeilToInt(glowRect.width * texelsPerUnit), 4, MaxMaskSize);
            int          height = Mathf.Clamp(Mathf.CeilToInt(glowRect.height * texelsPerUnit), 4, MaxMaskSize);

            EnsureRenderTexture(ref _MaskTexture, width, height);
            EnsureRenderTexture(ref _BlurTexture, width, height);

            _BakeMaterial.SetVector(BakeRectId, new Vector4(glowRect.xMin, glowRect.yMin, glowRect.width, glowRect.height));
            _BakeMaterial.SetFloat(BlurSigmaId, sigma * texelsPerUnit);

            _CommandBuffer.Clear();
            _CommandBuffer.SetRenderTarget(_MaskTexture);
            _CommandBuffer.ClearRenderTarget(false, true, Color.clear);

            for (int i = 0; i < meshCount; i++) {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                Texture         atlas = meshInfo.material != null ? meshInfo.material.mainTexture : null;

                if (meshInfo.mesh == null || atlas == null)
                    continue;
                _PropertyBlock.SetTexture(MainTexId, atlas);
                _CommandBuffer.DrawMesh(meshInfo.mesh, Matrix4x4.identity, _BakeMaterial, 0, MaskPass, _PropertyBlock);
            }

            _CommandBuffer.Blit(_MaskTexture, _BlurTexture, _BakeMaterial, BlurHorizontalPass);
            _CommandBuffer.Blit(_BlurTexture, _MaskTexture, _BakeMaterial, BlurVerticalPass);

            Graphics.ExecuteCommandBuffer(_CommandBuffer);

            _Graphic.SetGlow(_MaskTexture, glowRect);
        }

        private static void EnsureRenderTexture(ref RenderTexture texture, int width, int height)
        {
            if (texture != null && texture.width == width && texture.height == height)
                return;
            ReleaseRenderTexture(ref texture);

            RenderTextureFormat format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32;

            texture = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear) {
                name       = "TMP Outer Glow Mask",
                hideFlags  = HideFlags.HideAndDontSave,
                wrapMode   = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            texture.Create();
        }

#endregion

        #region Events

        private void OnTextChanged(Object changedObject)
        {
            if (changedObject == _Text)
                _IsMaskDirty = true;
        }

        private void OnWillRenderCanvases()
        {
            if (_Text == null || _Graphic == null)
                return;
            SyncGraphicTransform();

            if (_IsMaterialDirty) {
                _IsMaterialDirty = false;

                UpdateMaterial();
            }

            if (_UpdateEveryFrame || _MaskTexture == null || !_MaskTexture.IsCreated())
                _IsMaskDirty = true;
            if (_IsMaskDirty) {
                _IsMaskDirty = false;

                BakeMask();
            }
        }

        #endregion
    }
}
