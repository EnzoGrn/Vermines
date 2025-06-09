using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vermines.Player;
using Vermines.UI.Utils;

namespace Vermines.UI
{
    public class PlayerBookTab : MonoBehaviour
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
        private Animator _Animator;

        /// <summary>
        /// The hide animation coroutine.
        /// </summary>
        private Coroutine _HideCoroutine;

        [SerializeField]
        private GameObject _playerCultist;

        [SerializeField]
        private Image _cultistImage;

        [SerializeField]
        private Image _cultistBgImage;

        private PlayerRef _playerRef;

        #endregion

        #region Methods

        public virtual void Awake()
        {
            TryGetComponent(out _Animator);

            _playerCultist.SetActive(false);
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public void Show()
        {
            if (_HideCoroutine != null)
            {
                StopCoroutine(_HideCoroutine);

                if (_Animator.gameObject.activeInHierarchy && _Animator.HasState(0, ShowAnimHash))
                    _Animator.Play(ShowAnimHash, 0, 0);
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public void Hide()
        {
            if (_Animator != null && _Animator.gameObject.activeInHierarchy && _Animator.HasState(0, HideAnimHash))
            {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(HideAnimCoroutine());

                return;
            }

            gameObject.SetActive(false);
        }

        public void OnShow()
        {
            if (_playerCultist != null)
            {
                _playerCultist.SetActive(true);
            }
        }

        public void OnHide()
        {
            if (_playerCultist != null)
            {
                _playerCultist.SetActive(false);
            }
        }

        public void UpdateTab(PlayerData player)
        {
            if (player.PlayerRef != _playerRef)
            {
                _playerRef = player.PlayerRef;
                UpdateVisuals(player);
            }
        }

        private void UpdateVisuals(PlayerData player)
        {
            if (_cultistImage != null)
            {
                _cultistImage.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, player.Family, "Cultist");
            }
            if (_cultistBgImage != null)
            {
                _cultistBgImage.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, player.Family, "Background");
            }
        }

        /// <summary>
        /// Play the hide animation wrapped in a coroutine.
        /// Forces the target framerate to 60 during the transition animations.
        /// </summary>
        /// <returns>When done</returns>
        private IEnumerator HideAnimCoroutine()
        {
#if UNITY_IOS || UNITY_ANDROID
                var changedFramerate = false;
                
                if (Config.AdaptFramerateForMobilePlatform) {
                    if (Application.targetFrameRate < 60) {
                        Application.targetFrameRate = 60;
                        changedFramerate            = true;
                    }
                }
#endif

            _Animator.Play(HideAnimHash);

            yield return null;

            while (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;

#if UNITY_IOS || UNITY_ANDROID
                  if (changedFramerate)
                    new FusionMenuGraphicsSettings().Apply();
#endif

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

            if (_Animator != null && _Animator.gameObject.activeInHierarchy && _Animator.HasState(0, ShowAnimHash))
            {
                _Animator.Play(ShowAnimHash, 0, 0f);

                yield return null;

                while (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                {
                    yield return null;
                }
            }

#if UNITY_IOS || UNITY_ANDROID
    if (changedFramerate)
        new FusionMenuGraphicsSettings().Apply();
#endif
        }

        public IEnumerator PlayHideAnimCoroutine()
        {
            if (_Animator != null && _Animator.gameObject.activeInHierarchy && _Animator.HasState(0, HideAnimHash))
            {
                _Animator.Play(HideAnimHash, 0, 0f);

                yield return null;

                while (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                {
                    yield return null;
                }
            }

            gameObject.SetActive(false);
        }

        #endregion
    }
}