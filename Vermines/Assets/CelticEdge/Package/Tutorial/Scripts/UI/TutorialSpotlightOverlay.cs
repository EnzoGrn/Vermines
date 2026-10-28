using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Full-screen black veil with 1 to 4 illuminated "holes" around the targets.
    /// 
    /// Replaces the 4 panels from the prototype: smoothed edges, rounded corners or circles,
    /// multiple targets at once (map + drop zone), and an animated transition from one target to another.
    ///
    /// Clicks outside the holes are blocked (ICanvasRaycastFilter).
    /// Clicks inside a hole pass through to the element located beneath it.
    ///
    /// Place this on a RectTransform stretched across the entire tutorial canvas, with the material using the "Tutorial/Spotlight" shader.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class TutorialSpotlightOverlay : MaskableGraphic, ITutorialHighlight, ICanvasRaycastFilter, IPointerClickHandler {

        public const int    MaxHoles   = 4;
        public const string ShaderName = "Tutorial/Spotlight";

        [SerializeField, Range(0f, 1f)]
        private float _DimAlpha = 0.78f;

        [SerializeField]
        private float _EdgeSoftness = 10f;

        [SerializeField]
        private float _TransitionDuration = 0.3f;

        [SerializeField][Tooltip("Initial magnification when the veil appears (iris effect).")]
        private float _AppearScale = 2.5f;

        private static readonly int HolesId     = Shader.PropertyToID("_Holes");
        private static readonly int HoleRadiiId = Shader.PropertyToID("_HoleRadii");
        private static readonly int HoleCountId = Shader.PropertyToID("_HoleCount");
        private static readonly int DimId       = Shader.PropertyToID("_Dim");
        private static readonly int SoftnessId  = Shader.PropertyToID("_Softness");

        private readonly Vector4[] _Current = new Vector4[MaxHoles]; // center xy, half-size zw (local space)
        private readonly Vector4[] _From    = new Vector4[MaxHoles];
        private readonly Vector4[] _To      = new Vector4[MaxHoles];
        private readonly Vector4[] _Radii   = new Vector4[MaxHoles];

        private int      _HoleCount;
        private float    _TransitionT = 1f;
        private float    _CurrentDim;
        private float    _TargetDim;
        private bool     _BlockClicks;
        private bool     _Visible;
        private Material _RuntimeMaterial;

        /// <summary>
        /// The player clicked outside the highlighted areas.
        /// </summary>
        public event Action OnBlockedClick;

        public bool IsVisible => _Visible;

        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;

            if (!Application.isPlaying)
                return; // During editing, the source material remains untouched (no changes are made to the scene).
            if (m_Material != null && m_Material.shader != null && m_Material.shader.name == ShaderName)
                _RuntimeMaterial = new Material(m_Material); // Copy to avoid modifying the in-game material asset.
            else {
                Shader shader = Shader.Find(ShaderName);

                if (shader != null)
                    _RuntimeMaterial = new Material(shader);
                else
                    Debug.LogError($"[Tutorial] Shader \"{ShaderName}\" not found: assign the TutorialSpotlight material to TutorialSpotlightOverlay.", this);
            }

            if (_RuntimeMaterial != null)
                material = _RuntimeMaterial;
            ApplyToMaterial();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_RuntimeMaterial != null)
                Destroy(_RuntimeMaterial);
        }

        /// <summary>
        /// Displays the blur around the specified screen rectangles (empty = full blur).
        /// </summary>
        public void Show(IReadOnlyList<Rect> screenRects, SpotlightShape shape, float padding, bool dim, bool blockClicks)
        {
            bool wasVisible = _Visible;

            _Visible      = true;
            _BlockClicks  = blockClicks;
            raycastTarget = blockClicks;
            _TargetDim    = dim ? _DimAlpha : 0f;

            int count = Mathf.Min(screenRects?.Count ?? 0, MaxHoles);

            for (int i = 0; i < MaxHoles; i++) {
                _From[i] = _Current[i];

                if (i < count) {
                    _To[i]    = ToLocalHole(screenRects[i], shape, padding, out float radius);
                    _Radii[i] = new Vector4(radius, 0, 0, 0);

                    bool isNewHole = !wasVisible || i >= _HoleCount;

                    if (isNewHole)
                        _From[i] = new Vector4(_To[i].x, _To[i].y, _To[i].z * _AppearScale, _To[i].w * _AppearScale);
                } else {
                    _To[i] = Vector4.zero;
                }
            }

            _HoleCount   = count;
            _TransitionT = _TransitionDuration > 0 ? 0f : 1f;
        }

        /// <summary>
        /// Updates the positions of the holes without creating a new animation (moving targets).
        /// </summary>
        public void Track(IReadOnlyList<Rect> screenRects, SpotlightShape shape, float padding)
        {
            if (!_Visible)
                return;
            int count = Mathf.Min(screenRects?.Count ?? 0, MaxHoles);

            if (count > _HoleCount) {
                for (int j = 0; j < _HoleCount; j++)
                    _From[j] = _Current[j];
                _TransitionT = 0f;
            }

            for (int i = 0; i < count; i++) {
                _To[i]    = ToLocalHole(screenRects[i], shape, padding, out float radius);
                _Radii[i] = new Vector4(radius, 0, 0, 0);

                if (i >= _HoleCount) {
                    // Target appeared after the stage began: the hole opens right there.
                    _From[i] = _Current[i] = new Vector4(_To[i].x, _To[i].y, 0f, 0f);
                }
            }

            _HoleCount = count;
        }

        public void Hide()
        {
            _Visible      = false;
            _TargetDim    = 0f;
            raycastTarget = false;
            _BlockClicks  = false;
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;
            float dt        = Time.unscaledDeltaTime;
            float fadeSpeed = _TransitionDuration > 0 ? 1f / _TransitionDuration : 1000f;

            _CurrentDim = Mathf.MoveTowards(_CurrentDim, _TargetDim, dt * fadeSpeed);

            if (_TransitionT < 1f)
                _TransitionT = Mathf.Min(1f, _TransitionT + dt / Mathf.Max(0.0001f, _TransitionDuration));
            float k = 1f - Mathf.Pow(1f - _TransitionT, 3f); // cubic ease-out

            for (int i = 0; i < MaxHoles; i++)
                _Current[i] = Vector4.Lerp(_From[i], _To[i], k);
            ApplyToMaterial();
        }

        private void ApplyToMaterial()
        {
            Material m = materialForRendering;

            if (m == null)
                return;
            m.SetVectorArray(HolesId    , _Current);
            m.SetVectorArray(HoleRadiiId, _Radii);
            m.SetInt(HoleCountId        , _HoleCount);
            m.SetFloat(DimId            , _CurrentDim);
            m.SetFloat(SoftnessId       , _EdgeSoftness);
        }

        /// <summary>
        /// The mesh stores the local position of each vertex in its UV coordinates:
        /// the shader thus operates within the space of this RectTransform, regardless of the canvas mode or platform.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect    r = rectTransform.rect;
            Color32 c = color;

            vh.AddVert(new Vector3(r.xMin, r.yMin), c, new Vector2(r.xMin, r.yMin));
            vh.AddVert(new Vector3(r.xMin, r.yMax), c, new Vector2(r.xMin, r.yMax));
            vh.AddVert(new Vector3(r.xMax, r.yMax), c, new Vector2(r.xMax, r.yMax));
            vh.AddVert(new Vector3(r.xMax, r.yMin), c, new Vector2(r.xMax, r.yMin));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        private Vector4 ToLocalHole(Rect screenRect, SpotlightShape shape, float padding, out float radius)
        {
            Camera cam = EventCamera();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenRect.min, cam, out Vector2 min);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenRect.max, cam, out Vector2 max);

            Vector2 center = (min + max) * 0.5f;
            Vector2 half   = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y)) * 0.5f + Vector2.one * padding;

            if (shape == SpotlightShape.Circle) {
                float r = half.magnitude;

                half   = new Vector2(r, r);
                radius = r;
            } else {
                radius = Mathf.Min(padding, Mathf.Min(half.x, half.y));
            }

            return new Vector4(center.x, center.y, half.x, half.y);
        }

        private Camera EventCamera()
        {
            Canvas c = canvas != null ? canvas.rootCanvas : null;

            if (c == null || c.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return c.worldCamera;
        }

        /// <summary>
        /// True = this point is blocked by the veil.
        /// </summary>
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!_Visible || !_BlockClicks)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 p))
                return true;
            for (int i = 0; i < _HoleCount; i++) {
                Vector4 h = _To[i];

                if (Mathf.Abs(p.x - h.x) <= h.z && Mathf.Abs(p.y - h.y) <= h.w)
                    return false;
            }

            return true;
        }

        public void OnPointerClick(PointerEventData eventData) => OnBlockedClick?.Invoke();
    }
}
