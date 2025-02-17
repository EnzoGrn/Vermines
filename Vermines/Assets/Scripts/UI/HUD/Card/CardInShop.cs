using UnityEngine;
using UnityEngine.UI;

namespace Vermines.HUD.Card
{
    public class CardInShop : MonoBehaviour
    {
        CardBase cardBase;

        public void OpenOverlay()
        {
            Debug.Log("Affichage de l'overlay d'achat");
            ShopManager.instance.GetShop().OpenCardBuyOverlay(cardBase);
        }

        public void SetCardBase(CardBase cardBase)
        {
            this.cardBase = cardBase;
        }
    }
}