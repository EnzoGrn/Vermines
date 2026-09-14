using UnityEngine;
using System.Collections;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
using Vermines.UI.Utils;
using UnityEngine.Events;
using Vermines.Core.Player;
using Fusion;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;
    using Image = UnityEngine.UI.Image;

    public class PlayerBannerUI : MonoBehaviour
    {
        protected static readonly int HideAnimHash = Animator.StringToHash("Hide");
        protected static readonly int ShowAnimHash = Animator.StringToHash("Show");
        protected static readonly int ActiveAnimHash = Animator.StringToHash("Active");
        protected static readonly int IdleAnimHash = Animator.StringToHash("Idle");

        private Animator _animator;
        private Coroutine _HideCoroutine;

        public UnityEvent onHideComplete;

        public void OnHideAnimationComplete()
        {
            onHideComplete?.Invoke();
        }

        private CanvasGroup _canvasGroup;

        [Header("UI References")]
        [SerializeField] private Text nicknameText;
        [SerializeField] private GameObject eloquenceText;
        [SerializeField] private GameObject soulsText;
        [SerializeField] private RectTransform root;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image avatarImage;

        [Header("Scale Settings")]
        [SerializeField] private float normalScale = 1.1f;
        [SerializeField] private float activeScale = 1.275f;

        private PlayerController _player;
        private int _playerId;

        private AnimatedCountingTextNative _eloquenceScript;
        private AnimatedCountingTextNative _soulsScript;

        private void Awake()
        {
            TryGetComponent(out _animator);

            if (!_animator)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(PlayerBannerUI), gameObject.name, "Animator component is missing.");
            TryGetComponent(out _canvasGroup);

            if (eloquenceText != null)
                _eloquenceScript = eloquenceText.GetComponent<AnimatedCountingTextNative>();
            if (soulsText != null)
                _soulsScript = soulsText.GetComponent<AnimatedCountingTextNative>();
        }

        public void OnDestroy()
        {
            GameEvents.OnPlayerUpdated.RemoveListener(UpdateBanner);
        }

        public void Initialize(PlayerController player)
        {
            _player = player;
            _playerId = player.Object.InputAuthority.PlayerId;
            nicknameText.text = player.NetworkedNickname.Value;

            GameEvents.OnPlayerUpdated.AddListener(UpdateBanner);

            UpdateBanner(player);
        }

        private void UpdateBanner(PlayerController player)
        {
            if (player.Object.InputAuthority.PlayerId != _playerId)
                return;
            if (player.Statistics.Family != CardFamily.None)
            {
                avatarImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, player.Statistics.Family, "Cultist");
                backgroundImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, player.Statistics.Family, "Background");
            }

            nicknameText.text = player.NetworkedNickname.Value;

            UpdateStats(player.Statistics);
        }

        public void UpdateStats(PlayerStatistics playerData)
        {
            if (_eloquenceScript != null)
                _eloquenceScript.SetValue(playerData.Eloquence);
            if (_soulsScript != null)
                _soulsScript.SetValue(playerData.Souls);
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

        public void Hide()
        {
            if (_animator != null && _animator.gameObject.activeInHierarchy && _animator.HasState(0, HideAnimHash))
            {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(PlayHideAnimation());

                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public int GetPlayerId() => _playerId;

        public PlayerRef GetPlayerRef() => _player.Object.InputAuthority;

        public PlayerController GetPlayer() => _player;

        #region Animation Coroutines

        public IEnumerator PlayHideAnimation(bool adjustFramerate = true)
        {
            yield return AnimatedTransition.PlayAndWait(_animator, HideAnimHash, adjustFramerate);

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        #endregion
    }
}
