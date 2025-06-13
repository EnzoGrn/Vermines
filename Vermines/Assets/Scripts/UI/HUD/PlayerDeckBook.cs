using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Vermines.CardSystem.Elements;
using System.Collections;

namespace Vermines.UI
{
    public class PlayerDeckBook : MonoBehaviour
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

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Pagination")]
        [SerializeField] private int cardsPerPage = 10;
        [SerializeField] private GameObject nextPageButton;
        [SerializeField] private GameObject previousPageButton;

        private List<ICard> cards = new List<ICard>();
        private List<GameObject> slotPool = new List<GameObject>();

        private int currentPage = 0;

        #endregion

        public virtual void Awake()
        {
            TryGetComponent(out _animator);
            if (!_animator)
                Debug.LogErrorFormat(gameObject, "[{0}] {1} {2}", nameof(PlayerBookTab), gameObject.name, "PlayerBookTab is not properly initialized. Animator component is missing.");


            gameObject.SetActive(false);
        }

        #region Show/Hide

        /// <summary>
        /// Shows the book UI with animation.
        /// </summary>
        public void Show(List<ICard> cardList)
        {
            if (_HideCoroutine != null)
            {
                StopCoroutine(_HideCoroutine);

                if (_animator.gameObject.activeInHierarchy && _animator.HasState(0, ShowAnimHash))
                    _animator.Play(ShowAnimHash, 0, 0);
            }
            SetCards(cardList);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the book UI with animation.
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

        #endregion

        #region Setup

        /// <summary>
        /// Sets the title of the book section.
        /// </summary>
        /// <param name="title">The title to display.</param>
        public void SetTitle(string title)
        {
            titleText.text = title;
        }

        /// <summary>
        /// Loads the list of cards into the book.
        /// </summary>
        /// <param name="cardList">The list of cards to display.</param>
        public void SetCards(List<ICard> cardList)
        {
            Debug.Log($"[{GetType().Name}] Setting cards for PlayerDeckBook. Total cards: {cardList?.Count ?? 0}");
            cards = cardList ?? new List<ICard>();
            currentPage = 0;
            RefreshPage();
        }

        #endregion

        #region Pagination

        public void NextPage()
        {
            if ((currentPage + 1) * cardsPerPage < cards.Count)
            {
                currentPage++;
                RefreshPage();
            }
        }

        public void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                RefreshPage();
            }
        }

        private void RefreshPage()
        {
            EnsureSlotPoolInitialized(); // Make sure the pool has enough

            int startIndex = currentPage * cardsPerPage;
            int endIndex = Mathf.Min(startIndex + cardsPerPage, cards.Count);

            Debug.Log($"[{GetType().Name}] Refreshing page {currentPage + 1}. Cards from {startIndex} to {endIndex - 1}. Total cards: {cards.Count}");

            int cardIndex = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                GameObject slot = slotPool[cardIndex];
                slot.SetActive(true);

                if (slot.TryGetComponent(out DeckBookSlot cardSlot))
                {
                    cardSlot.Clear();
                    cardSlot.Configure(i);
                    cardSlot.SetCard(cards[i]);
                }

                cardIndex++;
            }

            // Deactivate remaining slots
            for (int i = cardIndex; i < cardsPerPage; i++)
            {
                slotPool[i].SetActive(false);
            }

            // Update buttons
            previousPageButton.SetActive(currentPage > 0);
            nextPageButton.SetActive((currentPage + 1) * cardsPerPage < cards.Count);
        }


        #endregion

        #region Pooling

        private void EnsureSlotPoolInitialized()
        {
            while (slotPool.Count < cardsPerPage)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotContainer);
                newSlot.SetActive(false);
                slotPool.Add(newSlot);
            }

            // Ensure all slots are cleared before reusing
            foreach (var slot in slotPool)
            {
                if (slot.TryGetComponent(out DeckBookSlot cardSlot))
                {
                    cardSlot.Clear();
                }
            }
        }

        #endregion

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

    }
}