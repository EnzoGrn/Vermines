using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vermines.Tutorial
{
    /// Voile noir plein écran percé de 1 à 4 "trous" éclairés autour des cibles.
    /// Remplace les 4 volets du prototype : bords adoucis, coins arrondis ou cercle,
    /// plusieurs cibles à la fois (carte + zone de dépôt), et transition animée d'une
    /// cible à l'autre.
    ///
    /// Les clics hors des trous sont bloqués (ICanvasRaycastFilter), les clics dans
    /// un trou passent à l'élément situé dessous.
    ///
    /// À placer sur un RectTransform étiré sur tout le canvas du tutoriel, avec le
    /// matériau utilisant le shader "Vermines/Tutorial/Spotlight".
    [RequireComponent(typeof(CanvasRenderer))]
    public class TutorialSpotlightOverlay : MaskableGraphic, ICanvasRaycastFilter, IPointerClickHandler
    {
        public const int MaxHoles = 4;
        public const string ShaderName = "Vermines/Tutorial/Spotlight";

        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.78f;
        [SerializeField] private float edgeSoftness = 10f;
        [SerializeField] private float transitionDuration = 0.3f;
        [Tooltip("Agrandissement de départ quand le voile apparaît (effet d'iris).")]
        [SerializeField] private float appearScale = 2.5f;

        private static readonly int HolesId = Shader.PropertyToID("_Holes");
        private static readonly int HoleRadiiId = Shader.PropertyToID("_HoleRadii");
        private static readonly int HoleCountId = Shader.PropertyToID("_HoleCount");
        private static readonly int DimId = Shader.PropertyToID("_Dim");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");

        private readonly Vector4[] _current = new Vector4[MaxHoles]; // centre xy, demi-taille zw (espace local)
        private readonly Vector4[] _from = new Vector4[MaxHoles];
        private readonly Vector4[] _to = new Vector4[MaxHoles];
        private readonly Vector4[] _radii = new Vector4[MaxHoles];
        private int _holeCount;
        private float _transitionT = 1f;
        private float _currentDim;
        private float _targetDim;
        private bool _blockClicks;
        private bool _visible;
        private Material _runtimeMaterial;

        /// Le joueur a cliqué hors des zones éclairées.
        public event Action OnBlockedClick;

        public bool IsVisible => _visible;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            if (!Application.isPlaying)
                return; // En édition, on ne touche pas au matériau (pas de modification de la scène).

            if (m_Material != null && m_Material.shader != null && m_Material.shader.name == ShaderName)
            {
                // Copie pour ne pas modifier l'asset du matériau en jeu.
                _runtimeMaterial = new Material(m_Material);
            }
            else
            {
                Shader shader = Shader.Find(ShaderName);
                if (shader != null)
                    _runtimeMaterial = new Material(shader);
                else
                    Debug.LogError($"[Tutoriel] Shader \"{ShaderName}\" introuvable : assignez le matériau TutorialSpotlight au TutorialSpotlightOverlay.", this);
            }
            if (_runtimeMaterial != null)
                material = _runtimeMaterial;
            ApplyToMaterial();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_runtimeMaterial != null)
                Destroy(_runtimeMaterial);
        }

        /// Affiche le voile autour des rectangles écran donnés (vide = voile complet).
        public void Show(IReadOnlyList<Rect> screenRects, SpotlightShape shape, float padding, bool dim, bool blockClicks)
        {
            bool wasVisible = _visible;
            _visible = true;
            _blockClicks = blockClicks;
            raycastTarget = blockClicks;
            _targetDim = dim ? dimAlpha : 0f;

            int count = Mathf.Min(screenRects?.Count ?? 0, MaxHoles);
            for (int i = 0; i < MaxHoles; i++)
            {
                _from[i] = _current[i];
                if (i < count)
                {
                    _to[i] = ToLocalHole(screenRects[i], shape, padding, out float radius);
                    _radii[i] = new Vector4(radius, 0, 0, 0);
                    bool isNewHole = !wasVisible || i >= _holeCount;
                    if (isNewHole)
                        _from[i] = new Vector4(_to[i].x, _to[i].y, _to[i].z * appearScale, _to[i].w * appearScale);
                }
                else
                {
                    _to[i] = Vector4.zero;
                }
            }
            _holeCount = count;
            _transitionT = transitionDuration > 0 ? 0f : 1f;
        }

        /// Met à jour la position des trous sans nouvelle animation (cibles qui bougent).
        public void Track(IReadOnlyList<Rect> screenRects, SpotlightShape shape, float padding)
        {
            if (!_visible)
                return;
            int count = Mathf.Min(screenRects?.Count ?? 0, MaxHoles);
            if (count > _holeCount)
            {
                for (int j = 0; j < _holeCount; j++)
                    _from[j] = _current[j];
                _transitionT = 0f;
            }
            for (int i = 0; i < count; i++)
            {
                _to[i] = ToLocalHole(screenRects[i], shape, padding, out float radius);
                _radii[i] = new Vector4(radius, 0, 0, 0);
                if (i >= _holeCount)
                {
                    // Cible apparue après le début de l'étape : le trou s'ouvre sur place.
                    _from[i] = _current[i] = new Vector4(_to[i].x, _to[i].y, 0f, 0f);
                }
            }
            _holeCount = count;
        }

        public void Hide()
        {
            _visible = false;
            _targetDim = 0f;
            raycastTarget = false;
            _blockClicks = false;
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;
            float dt = Time.unscaledDeltaTime;
            float fadeSpeed = transitionDuration > 0 ? 1f / transitionDuration : 1000f;
            _currentDim = Mathf.MoveTowards(_currentDim, _targetDim, dt * fadeSpeed);

            if (_transitionT < 1f)
                _transitionT = Mathf.Min(1f, _transitionT + dt / Mathf.Max(0.0001f, transitionDuration));
            float k = 1f - Mathf.Pow(1f - _transitionT, 3f); // ease-out cubique
            for (int i = 0; i < MaxHoles; i++)
                _current[i] = Vector4.Lerp(_from[i], _to[i], k);

            ApplyToMaterial();
        }

        private void ApplyToMaterial()
        {
            Material m = materialForRendering;
            if (m == null)
                return;
            m.SetVectorArray(HolesId, _current);
            m.SetVectorArray(HoleRadiiId, _radii);
            m.SetInt(HoleCountId, _holeCount);
            m.SetFloat(DimId, _currentDim);
            m.SetFloat(SoftnessId, edgeSoftness);
        }

        /// Le maillage porte la position locale de chaque sommet dans ses UV : le
        /// shader travaille ainsi dans l'espace de ce RectTransform, quel que soit le
        /// mode du canvas ou la plateforme.
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
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
            Vector2 half = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y)) * 0.5f + Vector2.one * padding;

            if (shape == SpotlightShape.Circle)
            {
                float r = half.magnitude;
                half = new Vector2(r, r);
                radius = r;
            }
            else
            {
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

        /// Vrai = ce point est bloqué par le voile.
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!_visible || !_blockClicks)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 p))
                return true;
            for (int i = 0; i < _holeCount; i++)
            {
                Vector4 h = _to[i];
                if (Mathf.Abs(p.x - h.x) <= h.z && Mathf.Abs(p.y - h.y) <= h.w)
                    return false;
            }
            return true;
        }

        public void OnPointerClick(PointerEventData eventData) => OnBlockedClick?.Invoke();
    }
}
