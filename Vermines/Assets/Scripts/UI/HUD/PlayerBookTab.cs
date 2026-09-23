using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vermines.Core.Player;
using Vermines.Player;
using Vermines.UI.Utils;

namespace Vermines.UI
{
    public class PlayerBookTab : MonoBehaviour
    {
        #region Attributes

        protected static readonly int HideAnimHash = Animator.StringToHash("Hide");
        protected static readonly int ShowAnimHash = Animator.StringToHash("Show");
        protected static readonly int ActiveAnimHash = Animator.StringToHash("Active");
        protected static readonly int IdleAnimHash = Animator.StringToHash("Idle");

        private Animator _animator;
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

        public void UpdateTab(PlayerStatistics stat, bool force = false)
        {
            if (force || stat.PlayerRef != _playerRef)
            {
                _playerRef = stat.PlayerRef;

                UpdateVisuals(stat);
            }
        }

        private void UpdateVisuals(PlayerStatistics stat)
        {
            if (_cultistImage != null)
                _cultistImage.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, stat.Family, "Cultist");
            if (_cultistBgImage != null)
                _cultistBgImage.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, stat.Family, "Background");
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
