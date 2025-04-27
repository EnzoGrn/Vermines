using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Vermines.CardSystem.Elements;
using Vermines.UI.GameTable;

namespace Vermines.UI.Card
{
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance;

        [Header("References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform handContainer;

        [Header("Settings")]
        [SerializeField] private int maxHandSize = 50;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private readonly List<GameObject> handCards = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            GameEvents.OnCardDrawn.AddListener(DrawCard);
        }

        private void Update()
        {
            if (debugMode && Input.GetKeyDown(KeyCode.Space))
                DrawCard();
        }

        #region Public Methods

        public void DrawCard()
        {
            if (handCards.Count >= maxHandSize) return;

            GameObject card = CreateCard();
            handCards.Add(card);

            UpdateCardPositions();
        }

        public void DrawCard(ICard card)
        {
            if (handCards.Count >= maxHandSize) return;

            Debug.Log($"[HandManager] Drawing card: {card.Data.name}");
            GameObject cardGO = CreateCard();
            if (cardGO.TryGetComponent<CardDisplay>(out var display))
                display.Display(card, null);
            else
                Debug.LogWarning("CardDisplay component missing on card prefab.");

            Debug.Log($"[HandManager] Card drawn: {card.Data.name}");
            handCards.Add(cardGO);

            UpdateCardPositions();
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
            UpdateCardPositions();
        }

        public void AddCard(GameObject card)
        {
            if (!handCards.Contains(card))
                handCards.Add(card);

            UpdateCardPositions();
        }

        public void LockAllCards(bool state)
        {
            foreach (var card in handCards)
            {
                if (card.TryGetComponent<CardHover>(out var hover))
                    hover.SetLocked(state);
            }
        }

        public void DiscardAllCards()
        {
            List<GameObject> cards = new List<GameObject>(handCards);
            foreach (var card in cards)
            {
                Debug.Log("[HandManager] Discard all cards");
                CardDisplay cardBase = card.GetComponent<CardDisplay>();
                GameEvents.OnCardDiscardRequestedNoEffect.Invoke(cardBase.Card);
            }
        }

        public bool HasRemainingCards() => handCards.Count > 0;

        #endregion

        #region Positioning

        public void UpdateCardPositions()
        {
            LockAllCards(true);

            float spacing = 1f / maxHandSize;
            float startT = 0.5f - (handCards.Count - 1) * spacing / 2;
            Spline spline = splineContainer.Spline;

            for (int i = 0; i < handCards.Count; i++)
            {
                Debug.Log("Update dotween");
                float t = startT + i * spacing;
                Vector3 worldOffset = handContainer.position;
                Vector3 position = (Vector3)spline.EvaluatePosition(t) + worldOffset;
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                var card = handCards[i];
                var localCard = card;

                card.transform.DOMove(position, 0.25f);
                card.transform.DORotateQuaternion(rotation, 0.25f);

                DOVirtual.DelayedCall(0.25f, () =>
                {
                    if (localCard != null && localCard.TryGetComponent<CardHover>(out var hover))
                        hover.SetInitialPosition();
                });
            }

            DOVirtual.DelayedCall(0.3f, () => LockAllCards(false));
        }

        #endregion

        #region Utils

        private GameObject CreateCard()
        {
            GameObject card = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation, handContainer);
            card.name = $"Card_{handCards.Count + 1}";
            //card.AddComponent<CardHover>();
            //card.AddComponent<DraggableCard>();
            return card;
        }

        public GameObject GetCardDisplayGO(ICard card)
        {
            foreach (var cardGO in handCards)
            {
                var display = cardGO.GetComponent<CardDisplay>();
                if (display != null && display.Card.ID == card.ID)
                {
                    return cardGO;
                }
            }
            return null;
        }

        #endregion
    }
}
