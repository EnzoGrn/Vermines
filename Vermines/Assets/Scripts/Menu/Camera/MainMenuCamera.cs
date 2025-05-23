using UnityEngine.Events;
using UnityEngine;
using Unity.Cinemachine;

namespace Vermines.Menu  {

    public class MainMenuCamera : MonoBehaviour {

        #region Fields

        [Header("Camera")]

        [SerializeField]
        private CinemachineSplineDolly _SplineDolly;

        [Header("Settings")]

        public float Speed = 1f;

        private bool _IsPlaying = false;

        #endregion

        #region Events

        public UnityEvent OnSplineStart = new();
        public UnityEvent OnSplineEnd   = new();
        public UnityEvent OnSplineReset = new();

        public void OnSplineStarted()
        {
            if (_IsPlaying == true)
                return;
            _IsPlaying = true;

            OnSplineStart?.Invoke();
        }

        private void OnSplineEnded()
        {
            if (_IsPlaying == false)
                return;
            _IsPlaying = false;

            OnSplineEnd?.Invoke();
        }

        public void OnSplineReseted()
        {
            _IsPlaying                  = false;
            _SplineDolly.CameraPosition = 0f;

            OnSplineReset?.Invoke();
        }

        #endregion

        #region Methods

        void Update()
        {
            if (_IsPlaying) {
                _SplineDolly.CameraPosition += Speed * Time.deltaTime;

                if (_SplineDolly.CameraPosition >= 1f) {
                    _SplineDolly.CameraPosition = 1f;
                    
                    OnSplineEnded();
                }
            }
        }

        #endregion
    }
}
