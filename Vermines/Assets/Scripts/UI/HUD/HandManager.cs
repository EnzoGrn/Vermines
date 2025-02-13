using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using DG.Tweening;

namespace Vermines.HUD
{
    public class HandManager : MonoBehaviour
    {
        [SerializeField] private int maxHandSize = 5; // TODO: Link this with game configuration
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform handContainer;

        private List<GameObject> handCards = new List<GameObject>();

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

        public void DrawCard()
        {
            if (handCards.Count >= maxHandSize) return;
            GameObject card = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation, handContainer.transform);
            card.transform.localScale = 1.5f * Vector3.one;
            handCards.Add(card);
            UpdateCardPosition();
        }

        private void UpdateCardPosition()
        {
            if (handCards.Count == 0) return;

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

                // 🔹 Met à jour la position initiale après l'animation
                int index = i;
                DOVirtual.DelayedCall(0.25f, () => handCards[index].GetComponent<CardHover>().SetInitialPosition());
            }
        }

    }
}
