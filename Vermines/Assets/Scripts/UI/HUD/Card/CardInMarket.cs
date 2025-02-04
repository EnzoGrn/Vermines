using UnityEngine;
using UnityEngine.UI;

namespace Vermines.HUD
{
    public class CardInMarket : CardBase
    {
        public void OpenOverlay()
        {
            Debug.Log("Affichage de l'overlay d'achat");
            HUDManager.Instance.OpenCardBuyOverlay(this);
        }
    }
}