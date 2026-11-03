using UnityEngine.UI;
using UnityEngine;

namespace Vermines.UI {

    /// <summary>
    /// Flame of the game intro: plays its frames in a loop and flickers a little, on the unscaled time.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PUI_GameIntroFlame : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Frames of the flame, played in a loop.
        /// </summary>
        [SerializeField]
        private Sprite[] _Frames = new Sprite[0];

        /// <summary>
        /// Frames shown per second.
        /// </summary>
        [SerializeField]
        private float _FramesPerSecond = 10f;

        /// <summary>
        /// Strength of the flicker on the scale.
        /// </summary>
        [SerializeField]
        private float _Flicker = 0.06f;

        private Image _Image;

        private float _Offset;

        private Vector3 _BaseScale;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _Image     = GetComponent<Image>();
            _Offset    = Random.Range(0f, 10f);
            _BaseScale = transform.localScale;
        }

        private void Update()
        {
            float time = Time.unscaledTime + _Offset;

            if (_Frames != null && _Frames.Length > 0) {
                int frame = Mathf.FloorToInt(time * _FramesPerSecond) % _Frames.Length;

                if (_Frames[frame] != null && _Image.sprite != _Frames[frame])
                    _Image.sprite = _Frames[frame];
            }
            float flicker = 1f + (Mathf.PerlinNoise(time * 3f, _Offset) - 0.5f) * 2f * _Flicker;

            transform.localScale = new Vector3(_BaseScale.x * (2f - flicker), _BaseScale.y * flicker, _BaseScale.z);
        }

        #endregion

        /// <summary>
        /// Gives the frames of the flame (used by the setup tool).
        /// </summary>
        public void SetFrames(Sprite[] frames, float framesPerSecond)
        {
            _Frames          = frames;
            _FramesPerSecond = framesPerSecond;
        }
    }
}
