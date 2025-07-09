using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Vermines.UI.Card
{
    public class HandLayout : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 150f;
        [SerializeField] private float animationDuration = 0.25f;
        [SerializeField] private bool centerAlign = true;

        public void UpdateLayout(List<GameObject> cards)
        {
            if (cards == null || cards.Count == 0) return;

            float totalWidth = (cards.Count - 1) * cardSpacing;
            float startX = centerAlign ? -totalWidth / 2 : 0;

            for (int i = 0; i < cards.Count; i++)
            {
                GameObject card = cards[i];
                RectTransform rectTransform = card.GetComponent<RectTransform>();
                if (rectTransform == null) continue;

                Vector3 targetPosition = new Vector3(startX + i * cardSpacing, 0f, 0f);

                rectTransform.DOAnchorPos(targetPosition, animationDuration).SetEase(Ease.OutQuad);
                rectTransform.DORotateQuaternion(Quaternion.identity, animationDuration).SetEase(Ease.OutQuad);
            }
        }
    }
}
