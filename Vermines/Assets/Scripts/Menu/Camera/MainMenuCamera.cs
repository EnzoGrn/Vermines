using System.Collections.Generic;
using UnityEngine.Splines;
using UnityEngine.Events;
using UnityEngine;
using Unity.Cinemachine;
using Unity.Mathematics;

namespace Vermines.Menu  {

    [System.Serializable]
    public class KnotPassedEvent : UnityEvent<int> {}

    public class MainMenuCamera : MonoBehaviour {

        #region Fields

        [Header("Camera")]

        [SerializeField]
        private CinemachineSplineDolly _SplineDolly;

        [Header("Spline")]

        [SerializeField]
        private SplineContainer _SplineContainer;

        [Header("Settings")]

        public float Speed = 1f;

        private bool _IsPlaying = false;

        [Header("Lookat")]

        [SerializeField]
        private Transform _InitialTarget;

        [SerializeField]
        private Transform _FinalTarget;

        [Header("Events")]

        [Tooltip("Event trigger when a Know is passed.")]
        public KnotPassedEvent OnKnotPassed;

        private readonly List<float> _KnotDistances = new();

        private readonly HashSet<int> _PassedKnots = new();

        #endregion

        #region Methods

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

            _SplineDolly.LookAtTarget.position = _InitialTarget.position;

            OnSplineStart?.Invoke();
        }

        private void OnSplineEnded()
        {
            if (_IsPlaying == false)
                return;
            _IsPlaying = false;

            _PassedKnots.Clear();
            OnSplineEnd?.Invoke();
        }

        public void OnSplineReseted()
        {
            _IsPlaying = false;

            if (_SplineDolly != null) {
                _SplineDolly.CameraPosition        = 0f;
                _SplineDolly.LookAtTarget.position = _InitialTarget.position;
            }

            _PassedKnots.Clear();
            OnSplineReset?.Invoke();
        }

        #endregion

        #region Methods

        private void Start()
        {
            for (int i = 0; i < _SplineContainer.Spline.Count; i++) {
                float distance = _SplineContainer.Spline.ConvertIndexUnit(i, PathIndexUnit.Knot, PathIndexUnit.Distance);

                _KnotDistances.Add(distance);
            }
        }

        void Update()
        {
            if (_IsPlaying) {
                // Convert the game object position into local spline position.
                Vector3 localPosition = _SplineContainer.transform.InverseTransformPoint(transform.position);

                // Calculate the nearest knot point from the spline.
                float3 nearestPoint;
                float t;

                SplineUtility.GetNearestPoint(_SplineContainer.Spline, localPosition, out nearestPoint, out t);

                // Convert the nearest knot point into world coordinates.
                Vector3 worldNearestPoint = _SplineContainer.transform.TransformPoint(nearestPoint);

                for (int i = 0; i < _SplineContainer.Spline.Count; i++) {
                    float knotT = SplineUtility.ConvertIndexUnit(_SplineContainer.Spline, i, PathIndexUnit.Knot, PathIndexUnit.Normalized);

                    if (t >= knotT && !_PassedKnots.Contains(i)) {
                        _PassedKnots.Add(i);

                        OnKnotPassed?.Invoke(i);
                    }
                }

                _SplineDolly.CameraPosition += Speed * Time.deltaTime;

                if (_SplineDolly.CameraPosition >= 1f) {
                    _SplineDolly.CameraPosition = 1f;
                    
                    OnSplineEnded();
                }
            }
        }

        public void SetEndLookAt()
        {
            _SplineDolly.LookAtTarget.position = _FinalTarget.position;
        }

        public int GetKnotsCount()
        {
            return _SplineContainer.Spline.Count;
        }

        #endregion
    }
}
