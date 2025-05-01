using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Vermines.CardSystem.Elements;

namespace Vermines.UI.Card
{
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance;

        [Header("References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private HandLayout handLayout;

        [Header("Settings")]
        [SerializeField] private int maxHandSize = 50;

        private readonly List<GameObject> handCards = new();

        public bool HasCards() => handCards.Count > 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            GameEvents.OnCardDrawn.AddListener(DrawCard);
        }

        #region Public Methods

        public void DrawCard(ICard card)
        {
            if (handCards.Count >= maxHandSize) return;

            Debug.Log($"[HandManager] Drawing card: {card.Data.name}");
            GameObject cardGO = CreateCard();

            if (cardGO.TryGetComponent<CardDisplay>(out var display))
                display.Display(card, null);
            else
                Debug.LogWarning("CardDisplay component missing on card prefab.");

            handCards.Add(cardGO);

            RefreshHand();
        }

        public void DrawCards(List<ICard> cards, int count)
        {
            for (int i = 0; i < count && i < cards.Count; i++)
                DrawCard(cards[i]);
        }

        public void RemoveCard(GameObject card)
        {
            handCards.Remove(card);
            card.transform.DOKill(true);
            RefreshHand();
        }

        public void AddCard(GameObject card)
        {
            if (!handCards.Contains(card))
                handCards.Add(card);

            RefreshHand();
        }

        public void DiscardAllCards()
        {
            List<GameObject> cards = new List<GameObject>(handCards);
            foreach (var card in cards)
            {
                Debug.Log("[HandManager] Discarding card.");
                CardDisplay display = card.GetComponent<CardDisplay>();
                if (display != null)
                    GameEvents.OnCardDiscardedRequestedNoEffect.Invoke(display.Card);
            }
        }

        public void ExpandHand(bool expand)
        {
            if (expand)
            {
                handLayout.transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutExpo);
                handLayout.transform.DOLocalMoveY(200f, 0.3f).SetEase(Ease.OutExpo);
            }
            else
            {
                handLayout.transform.DOScale(1f, 0.3f).SetEase(Ease.InExpo);
                handLayout.transform.DOLocalMoveY(0f, 0.3f).SetEase(Ease.InExpo);
            }
        }

        public GameObject GetCardDisplayGO(ICard card)
        {
            foreach (var cardGO in handCards)
            {
                if (cardGO.TryGetComponent<CardDisplay>(out var display) && display.Card.ID == card.ID)
                    return cardGO;
            }
            return null;
        }

        #endregion

        #region Private Methods

        private GameObject CreateCard()
        {
            GameObject card = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation, handLayout.transform);
            card.name = $"Card_{handCards.Count + 1}";
            card.transform.localScale = Vector3.one * 2f;
            return card;
        }

        private void RefreshHand()
        {
            handLayout.UpdateLayout(handCards);
        }

        #endregion
    }
}
