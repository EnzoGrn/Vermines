using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using DG.Tweening;

namespace Vermines.HUD
{
    using Vermines.CardSystem.Elements;
    using Vermines.HUD.Card;

    public class HandManager : MonoBehaviour
    {
        [SerializeField] private int maxHandSize = 5; // TODO: Link this with game configuration
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform handContainer;

        private List<GameObject> handCards = new List<GameObject>(); // TODO: Link this with player's hand

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private void Update()
        {
            if (debugMode)
            {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    DrawCard();
                }
            }
        }

        /// <summary>
        /// This method is used to draw a card from the deck.
        /// It will instantiate a new card and add it to the hand.
        /// </summary>
        public void DrawCard()
        {
            if (handCards.Count >= maxHandSize) return;
            GameObject card = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation, handContainer.transform);
            card.name = GenerateCardName(handCards);
            card.AddComponent<CardHover>();
            card.AddComponent<CardDraggable>();
            CardDraggable cardScript = card.GetComponent<CardDraggable>();
            if (cardScript != null)
            {
                cardScript.SetHandManager(this);
            }
            handCards.Add(card);
            UpdateCardPosition();
        }

        public void DrawCard(ICard cardObject)
        {
            // TODO: Generate card from card data
        }

        public void DrawCards(List<ICard> cards)
        {
            foreach (var card in cards)
            {
                DrawCard(card);
            }
        }

        /// <summary>
        /// This method is used to remove a card from the hand.
        /// </summary>
        public void RemoveCard(GameObject card)
        {
            handCards.Remove(card);
            UpdateCardPosition();
        }

        /// <summary>
        /// This method is used to add a card to the hand.
        /// Called only if you want to add a card to the hand without drawing it.
        /// </summary>
        public void AddCard(GameObject card)
        {
            handCards.Add(card);
            UpdateCardPosition();
        }

        public void UpdateCardPosition()
        {
            if (handCards.Count == 0) return;

            LockAllCards(true);

            float cardSpacing = 1f / maxHandSize;
            float firstCardPosition = 0.5f - (handCards.Count - 1) * cardSpacing / 2;
            Spline spline = splineContainer.Spline;

            for (int i = 0; i < handCards.Count; i++)
            {
                float t = firstCardPosition + i * cardSpacing;
                Vector3 handPosition = handContainer.position;
                Vector3 splinePosition = spline.EvaluatePosition(t);
                splinePosition += handPosition;
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                handCards[i].transform.DOMove(splinePosition, 0.25f);
                handCards[i].transform.DORotateQuaternion(rotation, 0.25f);

                int index = i;
                DOVirtual.DelayedCall(0.25f, () => handCards[index].GetComponent<CardHover>().SetInitialPosition());

            }
            DOVirtual.DelayedCall(0.3f, () => LockAllCards(false));
        }

        public void LockAllCards(bool state)
        {
            foreach (var card in handCards)
            {
                var hover = card.GetComponent<CardHover>();
                if (hover != null)
                {
                    hover.SetLocked(state);
                }
            }
        }

        private string GenerateCardName(List<GameObject> cards)
        {
            return $"Card_{cards.Count + 1}";
        }
    }
}
