using UnityEngine;
using TMPro;

namespace Vermines.UI.Effects {

    /// <summary>
    /// Bends a TextMeshPro text along a curve: each letter is raised by the curve and, optionally, turned to follow its slope
    /// (text written on a banner). The wave can also scroll to animate the text.
    /// </summary>
    /// <remarks>
    /// The curve is read from left (time 0) to right (time 1) of the text, its value is multiplied by <see cref="_Amplitude"/>.
    /// Only the vertices are moved: the text, its font and its material are untouched, and everything comes back when the component is disabled.
    /// </remarks>
    [ExecuteAlways][DisallowMultipleComponent][RequireComponent(typeof(TMP_Text))][AddComponentMenu("Vermines/UI/TMP Curve Wave")]
    public class TMPCurveWave : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Shape of the wave: time 0 = left edge of the text, time 1 = right edge. The value is multiplied by the amplitude.
        /// </summary>
        [Header("Shape")]
        [SerializeField]
        private AnimationCurve _Curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(1f, 0f, -2f, 0f));

        /// <summary>
        /// Height of the wave, in local units of the text (canvas pixels when the text is not scaled). Negative values bend it downwards.
        /// </summary>
        [SerializeField]
        private float _Amplitude = 20f;

        /// <summary>
        /// Number of times the curve is repeated across the text.
        /// </summary>
        [SerializeField, Min(0.01f)]
        private float _Frequency = 1f;

        /// <summary>
        /// Shift of the curve along the text (0-1).
        /// </summary>
        [SerializeField]
        private float _Phase = 0f;

        /// <summary>
        /// Turns each letter to follow the slope of the curve.
        /// </summary>
        [Header("Letters")]
        [SerializeField]
        private bool _FollowSlope = true;

        /// <summary>
        /// Strength of the rotation (1 = exactly the slope of the curve).
        /// </summary>
        [SerializeField, Range(0f, 2f)]
        private float _SlopeFactor = 1f;

        /// <summary>
        /// Scrolling speed of the wave, in curves per second (0 = static text).
        /// </summary>
        [Header("Animation")]
        [SerializeField]
        private float _Speed = 0f;

        /// <summary>
        /// The animation ignores Time.timeScale (menus, pause...).
        /// </summary>
        [SerializeField]
        private bool _UnscaledTime = true;

        /// <summary>
        /// Repeats the curve when it scrolls or when <see cref="_Frequency"/> is above 1 (otherwise the curve is clamped).
        /// </summary>
        [SerializeField]
        private bool _Loop = true;

        private TMP_Text _Text;

        private bool _Dirty = true;

        private bool _Applying;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            _Text  = GetComponent<TMP_Text>();
            _Dirty = true;

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

            // Gives the straight text back.
            if (_Text != null)
                _Text.ForceMeshUpdate(true, true);
        }

        private void OnValidate()
        {
            _Dirty = true;
        }

        private void LateUpdate()
        {
            if (_Text == null)
                return;
            if (!_Dirty && Mathf.Approximately(_Speed, 0f))
                return;
            _Dirty = false;

            Apply();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shape of the wave (time 0 = left of the text, 1 = right).
        /// </summary>
        public AnimationCurve Curve
        {
            get => _Curve;
            set
            {
                _Curve = value;
                _Dirty = true;
            }
        }

        /// <summary>
        /// Height of the wave.
        /// </summary>
        public float Amplitude
        {
            get => _Amplitude;
            set
            {
                _Amplitude = value;
                _Dirty     = true;
            }
        }

        /// <summary>
        /// Bends the text again (call it after changing the curve keys by code).
        /// </summary>
        public void Refresh()
        {
            _Dirty = true;
        }

        #endregion

        #region Deformation

        private void OnTextChanged(Object changed)
        {
            if (!_Applying && changed == _Text)
                _Dirty = true;
        }

        private void Apply()
        {
            _Applying = true;

            try {
                _Text.ForceMeshUpdate(true);
                Bend(_Text.textInfo);
                _Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            } finally {
                _Applying = false;
            }
        }

        private void Bend(TMP_TextInfo info)
        {
            if (info == null || info.characterCount == 0 || _Curve == null || _Curve.length == 0)
                return;
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            for (int i = 0; i < info.characterCount; i++) {
                TMP_CharacterInfo character = info.characterInfo[i];

                if (!character.isVisible)
                    continue;
                minX = Mathf.Min(minX, character.bottomLeft.x);
                maxX = Mathf.Max(maxX, character.topRight.x);
            }
            float width = maxX - minX;

            if (width <= 0f)
                return;
            float time  = _UnscaledTime ? Time.unscaledTime : Time.time;
            float phase = _Phase + (Application.isPlaying ? time * _Speed : 0f);

            for (int i = 0; i < info.characterCount; i++) {
                TMP_CharacterInfo character = info.characterInfo[i];

                if (!character.isVisible)
                    continue;
                Vector3[] vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                int index          = character.vertexIndex;
                float middle       = (character.bottomLeft.x + character.topRight.x) * 0.5f;
                float t            = (middle - minX) / width * _Frequency + phase;
                float offset       = Evaluate(t) * _Amplitude;
                Vector3 pivot      = new Vector3(middle, character.baseLine, 0f);
                Quaternion turn    = Quaternion.identity;

                if (_FollowSlope && _SlopeFactor > 0f) {
                    const float step = 0.001f;

                    // dy/dx of the curve in text units: (dCurve/dt) * amplitude * frequency / width.
                    float slope = (Evaluate(t + step) - Evaluate(t - step)) / (2f * step) * _Amplitude * _Frequency / width;

                    turn = Quaternion.Euler(0f, 0f, Mathf.Atan(slope) * Mathf.Rad2Deg * _SlopeFactor);
                }

                for (int v = 0; v < 4; v++)
                    vertices[index + v] = turn * (vertices[index + v] - pivot) + pivot + new Vector3(0f, offset, 0f);
            }
        }

        private float Evaluate(float t)
        {
            return _Curve.Evaluate(_Loop ? Mathf.Repeat(t, 1f) : Mathf.Clamp01(t));
        }

        #endregion
    }
}
