using UnityEngine;
using UnityEngine.EventSystems;

namespace Vermines.HUD
{
    public class CardClickable : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Carte {gameObject.name} cliquée !");
            HandleClick();
        }

        private void HandleClick()
        {
            if (TryGetComponent<CardInMarket>(out var marketCard))
            {
                marketCard.OpenOverlay();
            }
            //else if (TryGetComponent<CardOnTable>(out var tableCard))
            //{
            //    tableCard.OpenDetailsOverlay();
            //}
            // Ajoute d'autres conditions si nécessaire
        }
    }
}