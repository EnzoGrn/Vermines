using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vermines.Player;
using Vermines.UI.Plugin;
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
        /// Cached 'Active' animation hash.
        /// </summary>
        protected static readonly int ActiveAnimHash = Animator.StringToHash("Active");

        /// <summary>
        /// Cached 'Idle' animation hash.
        /// </summary>
        protected static readonly int IdleAnimHash = Animator.StringToHash("Idle");

        /// <summary>
        /// The animator component.
        /// </summary>
        private Animator _animator;

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
        public PlayerRef PlayerRef => _playerRef;

        #endregion

        #region Methods

        public virtual void Awake()
        {
            TryGetComponent(out _animator);
            if (!_animator)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(PlayerBookTab), gameObject.name, "PlayerBookTab is not properly initialized. Animator component is missing.");

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

        public void UpdateTab(PlayerData player, bool force = false)
        {
            if (force || player.PlayerRef != _playerRef)
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
        /// Plays the 'Active' animation to visually indicate the tab is selected.
        /// </summary>
        public void PlayActiveAnimation(bool isActive)
        {
            if (_animator != null && _animator.gameObject.activeInHierarchy && _animator.HasState(0, ActiveAnimHash))
            {
                if (isActive)
                {
                    _animator.Play(ActiveAnimHash, 0, 0f);
                }
                else
                {
                    _animator.Play(IdleAnimHash, 0, 0f);
                }
            }
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