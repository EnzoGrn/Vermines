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

            GameEvents.OnCardDiscarded.AddListener(OnCardDiscarded);
            GameEvents.OnCardSacrified.AddListener(OnCardSacrified);
        }

        private void Start()
        {
            GameEvents.OnCardDrawn.AddListener(DrawCard);
        }

        #region Public Methods

        public void DrawCard(ICard card)
        {
            if (handCards.Count >= maxHandSize) return;

            Debug.Log($"[HandManager] Drawing card: {card.Data.Name}");
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

        private void OnCardDiscarded(ICard card)
        {
            GameObject go = GetCardDisplayGO(card);
            if (go != null)
            {
                RemoveCard(go);
                go.transform.DOKill(true);
                Destroy(go);
            }
        }

        private void OnCardSacrified(ICard card)
        {
            GameObject go = GetCardDisplayGO(card);
            if (go != null)
            {
                RemoveCard(go);
                go.transform.DOKill(true);
                Destroy(go);
            }
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
