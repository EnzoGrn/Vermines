using UnityEngine;
using UnityEngine.UI;

namespace Vermines.HUD.Card
{
    public class CardInShop : CardBase
    {
        public void OpenOverlay()
        {
            Debug.Log("Affichage de l'overlay d'achat");
            ShopManager.instance.GetShop().OpenCardBuyOverlay(this);
        }
    }
}