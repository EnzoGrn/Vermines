using UnityEngine;
using System.Collections;

namespace Vermines.UI
{
    public class ContextBanner : MonoBehaviour
    {
        #region Attributes

        protected static readonly int HideAnimHash = Animator.StringToHash("Hide");
        protected static readonly int ShowAnimHash = Animator.StringToHash("Show");

        private Animator _animator;
        private Coroutine _HideCoroutine;

        public IUIContext AssociatedContext { get; set; }

        #endregion

        #region Methods

        public virtual void Awake()
        {
            TryGetComponent(out _animator);
            if (!_animator)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(ContextBanner), gameObject.name, "ContextBanner is not properly initialized. Animator component is missing.");
        }

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
            gameObject.SetActive(true);

            yield return AnimatedTransition.PlayAndWait(_animator, ShowAnimHash);
        }

        public IEnumerator PlayHideAnimation(bool adjustFramerate = true)
        {
            yield return AnimatedTransition.PlayAndWait(_animator, HideAnimHash, adjustFramerate);

            gameObject.SetActive(false);
        }

        #endregion
    }
}
