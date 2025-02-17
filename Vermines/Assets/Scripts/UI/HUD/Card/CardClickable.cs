using UnityEngine;
using UnityEngine.EventSystems;

namespace Vermines.HUD.Card
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
            if (CompareTag("ShopCard"))
            {
                if (TryGetComponent<CardInShop>(out var marketCard))
                {
                    marketCard.OpenOverlay();
                }
            }
            //else if (CompareTag("TableCard"))
            //{
            //    if (TryGetComponent<CardOnTable>(out var tableCard))
            //    {
            //        tableCard.OpenDetailsOverlay();
            //    }
            //}
            else if (CompareTag("HandCard"))
            {
                if (TryGetComponent<CardInHand>(out var inventoryCard))
                {
                    //inventoryCard.OpenInventoryDetails();
                }
            }
            // Ajoute d'autres conditions si nécessaire
        }
    }
}