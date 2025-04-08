using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Vermines.CardSystem.Elements;

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

        public void DrawCard(ICard cardData)
        {
            if (handCards.Count >= maxHandSize) return;

            GameObject card = CreateCard();
            if (card.TryGetComponent<CardDisplay>(out var display))
                display.Display(cardData, null);
            else
                Debug.LogWarning("CardDisplay component missing on card prefab.");

            handCards.Add(card);

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
            foreach (var card in handCards)
            {
                CardDisplay cardBase = card.GetComponent<CardDisplay>();
                GameEvents.InvokeOnDiscard(cardBase.Card.ID);
            }

            handCards.Clear();
            UpdateCardPositions();
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
                float t = startT + i * spacing;
                Vector3 worldOffset = handContainer.position;
                Vector3 position = (Vector3)spline.EvaluatePosition(t) + worldOffset;
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                var card = handCards[i];
                card.transform.DOMove(position, 0.25f);
                card.transform.DORotateQuaternion(rotation, 0.25f);

                int index = i;
                DOVirtual.DelayedCall(0.25f, () =>
                {
                    if (card.TryGetComponent<CardHover>(out var hover))
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
            card.AddComponent<CardHover>();
            card.AddComponent<CardDraggable>();
            return card;
        }

        #endregion
    }
}
