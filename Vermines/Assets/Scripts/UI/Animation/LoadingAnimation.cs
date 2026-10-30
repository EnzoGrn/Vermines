using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Animations {

    /// <summary>
    /// Makes the loading dots jump one after the other.
    /// The pose of every dot is computed from the real time each frame, so after a frame hitch
    /// the animation is back at the right place at once instead of restarting its coroutines.
    /// </summary>
    public class LoadingAnimationPlugin : MonoBehaviour {

        #region Attributes

        [SerializeField]
        private List<RectTransform> _Dots = new();

        [SerializeField]
        private float _JumpHeight = 20f;

        [SerializeField]
        private float _JumpDuration = 0.3f;

        [SerializeField]
        private float _DelayBetweenJumps = 0.2f;

        private Vector2[] _OriginalPositions;

        #endregion

        #region MonoBehaviour Methods

        protected void Awake()
        {
            _OriginalPositions = new Vector2[_Dots.Count];

            for (int i = 0; i < _Dots.Count; i++)
                _OriginalPositions[i] = _Dots[i].anchoredPosition;
        }

        protected void Update()
        {
            if (_OriginalPositions == null || _JumpDuration <= 0f)
                return;
            float cycle = Mathf.Max(_Dots.Count * _DelayBetweenJumps, _JumpDuration);
            float time  = Time.unscaledTime;

            for (int i = 0; i < _Dots.Count; i++) {
                if (_Dots[i] == null)
                    continue;
                float phase  = Mathf.Repeat(time - i * _DelayBetweenJumps, cycle);
                float height = phase < _JumpDuration ? Mathf.Sin(phase / _JumpDuration * Mathf.PI) * _JumpHeight : 0f;

                _Dots[i].anchoredPosition = _OriginalPositions[i] + new Vector2(0f, height);
            }
        }

        protected void OnDisable()
        {
            if (_OriginalPositions == null)
                return;
            for (int i = 0; i < _Dots.Count; i++) {
                if (_Dots[i] != null)
                    _Dots[i].anchoredPosition = _OriginalPositions[i];
            }
        }

        #endregion
    }
}
