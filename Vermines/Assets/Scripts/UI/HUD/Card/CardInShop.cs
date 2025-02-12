using UnityEngine;
using UnityEngine.UI;

namespace Vermines.HUD
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