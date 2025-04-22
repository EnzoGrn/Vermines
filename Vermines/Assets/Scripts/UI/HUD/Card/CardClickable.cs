using UnityEngine;
using UnityEngine.EventSystems;

namespace Vermines.UI.Card
{
    public class CardClickable : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Carte {gameObject.name} cliquée !");
            HandleClick();
        }

        /// <summary>
        /// Handles the click on the card.
        /// It will open the overlay for a card in the shop and select the card for a card in the hand.
        /// </summary>
        private void HandleClick()
        {
            if (CompareTag("TableCard"))
            {
                if (TryGetComponent<CardInTable>(out var tableCard))
                {
                    tableCard.OpenOverlay();
                }
            }
            // Add more cases here
        }
    }
}