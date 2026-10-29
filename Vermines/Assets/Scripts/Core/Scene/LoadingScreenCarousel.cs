using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Vermines.Core.Scene {

    /// <summary>
    /// Shows loading screen prefabs one after the other and cross fades to a new one every few seconds.
    /// The screens come from the serialized list, or from a Resources folder when the list is empty.
    /// </summary>
    public class LoadingScreenCarousel : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Loading screen prefabs. When empty, every prefab of <see cref="_ResourcesFolder"/> is used.
        /// </summary>
        [SerializeField]
        [Tooltip("Loading screen prefabs. Leave empty to use every prefab of the Resources folder below.")]
        private List<GameObject> _Screens = new();

        /// <summary>
        /// Resources folder that holds the loading screen prefabs, used when <see cref="_Screens"/> is empty.
        /// </summary>
        [SerializeField]
        [Tooltip("Resources folder of the loading screen prefabs, used when the list above is empty.")]
        private string _ResourcesFolder = "Prefabs/UI/LoadingScreen";

        /// <summary>
        /// Time a screen stays on display before the next one comes in, in seconds.
        /// </summary>
        [SerializeField]
        [Min(0.5f)]
        [Tooltip("Time a screen stays on display before the next one comes in, in seconds.")]
        private float _DisplayDuration = 6f;

        /// <summary>
        /// Duration of the cross fade between two screens, in seconds.
        /// </summary>
        [SerializeField]
        [Min(0f)]
        [Tooltip("Duration of the cross fade between two screens, in seconds.")]
        private float _TransitionDuration = 1.2f;

        /// <summary>
        /// Scale of the incoming screen when its fade starts, it settles to 1 during the fade.
        /// </summary>
        [SerializeField]
        [Range(1f, 1.2f)]
        [Tooltip("Scale of the incoming screen when its fade starts. It settles to 1 during the fade, 1 disables the zoom.")]
        private float _IncomingScale = 1.04f;

        /// <summary>
        /// Minimum time a screen that came in by a transition stays fully visible before the loading screen may close.
        /// </summary>
        [SerializeField]
        [Min(0f)]
        [Tooltip("Minimum time a new screen stays fully visible before the loading screen may close, so it never flashes. 0 disables it.")]
        private float _MinimumScreenTime = 2f;

        /// <summary>
        /// Picks the screens in a random order, without showing the same one twice in a row.
        /// </summary>
        [SerializeField]
        [Tooltip("Random order without showing the same screen twice in a row. Otherwise the list order is used.")]
        private bool _Shuffle = true;

        /// <summary>
        /// Prefabs left to show before the order is shuffled again.
        /// </summary>
        private readonly List<GameObject> _Bag = new();

        /// <summary>
        /// Prefab of the screen shown last.
        /// </summary>
        private GameObject _LastPrefab;

        /// <summary>
        /// Index of the next prefab when the order is not shuffled.
        /// </summary>
        private int _NextIndex;

        /// <summary>
        /// Screen currently on display.
        /// </summary>
        private CanvasGroup _Current;

        /// <summary>
        /// Screen fading in above the current one, null outside transitions.
        /// </summary>
        private CanvasGroup _Incoming;

        /// <summary>
        /// Running rotation, null when stopped.
        /// </summary>
        private Coroutine _Rotation;

        /// <summary>
        /// False to keep the current screen, for instance while the loading screen fades out.
        /// </summary>
        private bool _IsRotating = true;

        /// <summary>
        /// Real time when the current screen became fully visible.
        /// </summary>
        private float _CurrentShownTime;

        /// <summary>
        /// True when the current screen came in by a transition, false for the first screen.
        /// </summary>
        private bool _IsCurrentFromTransition;

        /// <summary>
        /// Estimated time left before the loading ends, negative when unknown. A screen only changes when enough time is left.
        /// </summary>
        public System.Func<float> RemainingTimeProvider;

        #endregion

        #region Getters & Setters

        /// <summary>
        /// True when no transition is running and the current screen has been seen long enough to close the loading screen.
        /// </summary>
        public bool HasSettled => _Incoming == null && (!_IsCurrentFromTransition || Time.realtimeSinceStartup - _CurrentShownTime >= _MinimumScreenTime);

        #endregion

        #region MonoBehaviour Methods

        protected void Awake()
        {
            if (_Screens.Count == 0 && !string.IsNullOrEmpty(_ResourcesFolder))
                _Screens.AddRange(Resources.LoadAll<GameObject>(_ResourcesFolder));
            _Screens.RemoveAll(screen => screen == null);

            if (_Screens.Count == 0)
                Debug.LogWarning($"[LoadingScreenCarousel] No loading screen prefab found (Resources/{_ResourcesFolder}).", this);
        }

        protected void OnEnable()
        {
            if (_Current == null) {
                _Current                 = Spawn(NextPrefab());
                _CurrentShownTime        = Time.realtimeSinceStartup;
                _IsCurrentFromTransition = false;
            }

            if (_IsRotating)
                StartRotation();
        }

        protected void OnDisable()
        {
            StopRotation();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts or stops the change of screen. When stopped, a running transition still ends smoothly.
        /// </summary>
        /// <param name="isRotating">True to change the screen every few seconds.</param>
        public void SetRotating(bool isRotating)
        {
            _IsRotating = isRotating;

            if (isRotating && isActiveAndEnabled)
                StartRotation();
        }

        private void StartRotation()
        {
            if (_Rotation == null && _Screens.Count > 1)
                _Rotation = StartCoroutine(Rotate());
        }

        private void StopRotation()
        {
            if (_Rotation != null) {
                StopCoroutine(_Rotation);

                _Rotation = null;
            }
            CompleteTransition();
        }

        private IEnumerator Rotate()
        {
            while (_IsRotating) {
                for (float time = 0f; time < _DisplayDuration; time += Time.unscaledDeltaTime)
                    yield return null;

                // Wait while the loading is expected to end before the next screen could be seen properly.
                while (_IsRotating && !HasEnoughTimeLeft())
                    yield return null;
                if (!_IsRotating)
                    break;
                _Incoming = Spawn(NextPrefab());

                if (_Incoming == null)
                    continue;
                SetVisual(_Incoming, 0f, _IncomingScale);

                for (float time = 0f; time < _TransitionDuration; time += Time.unscaledDeltaTime) {
                    float progress = Mathf.SmoothStep(0f, 1f, time / _TransitionDuration);

                    SetVisual(_Incoming, progress, Mathf.Lerp(_IncomingScale, 1f, progress));

                    yield return null;
                }
                CompleteTransition();
            }
            _Rotation = null;
        }

        private bool HasEnoughTimeLeft()
        {
            float remaining = RemainingTimeProvider != null ? RemainingTimeProvider() : -1f;

            return remaining < 0f || remaining >= _TransitionDuration + _MinimumScreenTime;
        }

        /// <summary>
        /// Shows the incoming screen fully and removes the previous one.
        /// </summary>
        private void CompleteTransition()
        {
            if (_Incoming == null)
                return;
            SetVisual(_Incoming, 1f, 1f);

            if (_Current != null)
                Destroy(_Current.gameObject);
            _Current                 = _Incoming;
            _Incoming                = null;
            _CurrentShownTime        = Time.realtimeSinceStartup;
            _IsCurrentFromTransition = true;
        }

        private CanvasGroup Spawn(GameObject prefab)
        {
            if (prefab == null)
                return null;
            GameObject screen = Instantiate(prefab, transform, false);

            screen.transform.SetAsLastSibling();

            if (!screen.TryGetComponent(out CanvasGroup group))
                group = screen.AddComponent<CanvasGroup>();
            group.interactable   = false;
            group.blocksRaycasts = false;

            SetVisual(group, 1f, 1f);

            return group;
        }

        private static void SetVisual(CanvasGroup screen, float alpha, float scale)
        {
            screen.alpha = alpha;
            screen.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private GameObject NextPrefab()
        {
            if (_Screens.Count == 0)
                return null;
            GameObject prefab;

            if (_Shuffle) {
                if (_Bag.Count == 0) {
                    _Bag.AddRange(_Screens);

                    for (int i = _Bag.Count - 1; i > 0; i--) {
                        int j = Random.Range(0, i + 1);

                        (_Bag[i], _Bag[j]) = (_Bag[j], _Bag[i]);
                    }

                    // Never show the same screen twice in a row when a new round starts.
                    if (_Bag.Count > 1 && _Bag[_Bag.Count - 1] == _LastPrefab)
                        (_Bag[0], _Bag[_Bag.Count - 1]) = (_Bag[_Bag.Count - 1], _Bag[0]);
                }
                prefab = _Bag[_Bag.Count - 1];

                _Bag.RemoveAt(_Bag.Count - 1);
            } else {
                prefab = _Screens[_NextIndex % _Screens.Count];

                _NextIndex++;
            }
            _LastPrefab = prefab;

            return prefab;
        }

        #endregion
    }
}
