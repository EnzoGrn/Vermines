using UnityEngine;
using System.Collections;

namespace Vermines.UI
{
    public class ContextBanner : MonoBehaviour
    {
        #region Attributes

        /// <summary>
        /// Cached 'Hide' animation hash.
        /// </summary>
        protected static readonly int HideAnimHash = Animator.StringToHash("Hide");

        /// <summary>
        /// Cached 'Show' animation hash.
        /// </summary>
        protected static readonly int ShowAnimHash = Animator.StringToHash("Show");

        /// <summary>
        /// The animator component.
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// The hide animation coroutine.
        /// </summary>
        private Coroutine _HideCoroutine;

        public IUIContext AssociatedContext { get; set; }

        #endregion

        #region Methods

        public virtual void Awake()
        {
            TryGetComponent(out _animator);
            if (!_animator)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(PlayerBookTab), gameObject.name, "PlayerBookTab is not properly initialized. Animator component is missing.");
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public void Show()
        {
            if (_HideCoroutine != null)
            {
                StopCoroutine(_HideCoroutine);

                if (_animator.gameObject.activeInHierarchy && _animator.HasState(0, ShowAnimHash))
                    _animator.Play(ShowAnimHash, 0, 0);
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public void Hide()
        {
            if (_animator != null && _animator.gameObject.activeInHierarchy && _animator.HasState(0, HideAnimHash))
            {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(PlayHideAnimation());

                return;
            }

            gameObject.SetActive(false);
        }

        public IEnumerator PlayShowAnimCoroutine()
        {
#if UNITY_IOS || UNITY_ANDROID
    var changedFramerate = false;

    if (Config.AdaptFramerateForMobilePlatform)
    {
        if (Application.targetFrameRate < 60)
        {
            Application.targetFrameRate = 60;
            changedFramerate = true;
        }
    }
#endif

            gameObject.SetActive(true);

            if (_animator != null && _animator.gameObject.activeInHierarchy && _animator.HasState(0, ShowAnimHash))
            {
                _animator.Play(ShowAnimHash, 0, 0f);

                yield return null;

                while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                {
                    yield return null;
                }
            }

#if UNITY_IOS || UNITY_ANDROID
    if (changedFramerate)
        new FusionMenuGraphicsSettings().Apply();
#endif
        }

        public IEnumerator PlayHideAnimation(bool adjustFramerate = true)
        {
#if UNITY_IOS || UNITY_ANDROID
    bool changedFramerate = false;

    if (adjustFramerate && Config.AdaptFramerateForMobilePlatform && Application.targetFrameRate < 60)
    {
        Application.targetFrameRate = 60;
        changedFramerate = true;
    }
#endif

            if (_animator != null && _animator.gameObject.activeInHierarchy && _animator.HasState(0, HideAnimHash))
            {
                _animator.Play(HideAnimHash, 0, 0f);

                yield return null; // Wait one frame for animation to start

                while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                {
                    yield return null;
                }
            }

#if UNITY_IOS || UNITY_ANDROID
    if (changedFramerate)
    {
        new FusionMenuGraphicsSettings().Apply();
    }
#endif

            gameObject.SetActive(false);
        }

        #endregion
    }
}