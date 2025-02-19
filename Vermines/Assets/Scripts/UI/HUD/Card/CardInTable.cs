using UnityEngine;

namespace Vermines.HUD.Card
{
    public class CardInTable : MonoBehaviour
    {
        CardBase cardBase;

        public void OpenOverlay()
        {
            Debug.Log("Affichage de l'overlay d'achat");
            TableManager.instance.OpenSacrificeOverlay(cardBase);
        }

        public void SetCardBase(CardBase cardBase)
        {
            this.cardBase = cardBase;
        }
    }
}