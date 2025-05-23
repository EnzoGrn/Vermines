using System.Collections.Generic;
using UnityEngine.Splines;
using UnityEngine.Events;
using UnityEngine;
using Unity.Cinemachine;

namespace Vermines.Menu  {

    public class MainMenuCamera : MonoBehaviour {

        #region Fields

        [Header("Camera")]

        [SerializeField]
        private CinemachineCamera _CinemachineCamera;

        [SerializeField]
        private Camera _MainFixCamera;

        [SerializeField]
        private Camera _MainCamera;

        [SerializeField]
        private List<GameObject> _LookAt;

        [Header("Spline")]

        [SerializeField]
        private SplineContainer _SplineContainerRef;

        [SerializeField]
        private Animator _SplineCameraAnimator;

        [SerializeField]
        private CinemachineSplineDolly _CinemachineSplineDolly;

        private bool _IsAnimated = false;

        #endregion

        #region Events

        public UnityEvent OnCamLocationChanged = new();
        public UnityEvent OnCamLocationIsChanging = new();

        public void OnCameraSplineCompleted()
        {
            _SplineCameraAnimator.enabled = false;
            _IsAnimated                   = false;

            OnCamLocationChanged?.Invoke();
        }

        public void OnCameraSplineStart()
        {
            OnCamLocationIsChanging?.Invoke();
        }

        #endregion

        #region Methods

        public void StartSplineCameraAnimation(int splineID)
        {
            _IsAnimated = true;

            _CinemachineCamera.Follow = _LookAt[splineID].transform;
            _CinemachineSplineDolly.SplineSettings.Spline.Spline = _SplineContainerRef.Splines[splineID - 1];

            ResetAnimation();
        }

        private void ResetAnimation(bool reversed = false)
        {
            if (_SplineCameraAnimator != null) {
                _SplineCameraAnimator.gameObject.SetActive(true);

                _SplineCameraAnimator.enabled = true;
                _SplineCameraAnimator.SetBool("IsReversed", reversed);

                if (!reversed) {
                    _SplineCameraAnimator.Rebind();        // Resets the animator's state
                    _SplineCameraAnimator.Play(0, -1, 0f); // Restart animation from the beginning
                }
            } else
                Debug.LogWarning($"[MainMenuCamera]: SplineCameraAnimator is null");
        }

        public void GoOnTavern() // TODO:
        {
            if (_IsAnimated)
                return;
            StartSplineCameraAnimation(1);
        }

        public void GoOnNoneLocation() // TODO: 
        {
            if (_IsAnimated)
                return;
            StartSplineCameraAnimation(0);
        }

        #endregion
    }
}
