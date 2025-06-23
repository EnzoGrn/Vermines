using UnityEngine;
using System.Collections;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
using Vermines.UI.Utils;
using UnityEngine.Events;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;
    using Image = UnityEngine.UI.Image;

    public class PlayerBannerUI : MonoBehaviour
    {
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

        public UnityEvent onHideComplete;

        public void OnHideAnimationComplete()
        {
            onHideComplete?.Invoke();
        }

        /// <summary>
        /// The canvas group component for managing visibility and interaction.
        /// </summary>
        private CanvasGroup _canvasGroup;

        [Header("UI References")]

        [SerializeField] private Text nicknameText;
        [SerializeField] private Text eloquenceText;
        [SerializeField] private Text soulsText;
        [SerializeField] private RectTransform root;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image avatarImage;

        [Header("Scale Settings")]
        [SerializeField] private float normalScale = 1.1f;
        [SerializeField] private float activeScale = 1.275f;

        private int _playerId;

        private void Awake()
        {
            TryGetComponent(out _animator);
            if (!_animator)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(PlayerBannerUI), gameObject.name, "Animator component is missing.");
            TryGetComponent(out _canvasGroup);
        }

        public void Initialize(PlayerData playerData, int playerId)
        {
            _playerId = playerId;
            nicknameText.text = playerData.Nickname;

            if (playerData.Family != CardFamily.None)
            {
                avatarImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, playerData.Family, "Cultist");
                backgroundImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, playerData.Family, "Background");
            }
            UpdateStats(playerData);
        }

        public void UpdateStats(PlayerData playerData)
        {
            eloquenceText.text = playerData.Eloquence.ToString();
            soulsText.text = playerData.Souls.ToString();
        }

        public void SetActive(bool isActive)
        {
            root.localScale = Vector3.one * (isActive ? activeScale : normalScale);
        }

        public void Show()
        {
            if (_HideCoroutine != null)
            {
                StopCoroutine(_HideCoroutine);

                if (_animator.gameObject.activeInHierarchy && _animator.HasState(0, ShowAnimHash))
                    _animator.Play(ShowAnimHash, 0, 0);
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
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

            _canvasGroup.alpha = 0f; // invisible
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public int GetPlayerId() => _playerId;

        #region Animation Coroutines

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

            _canvasGroup.alpha = 0f; // invisible
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

#endregion
    }
}