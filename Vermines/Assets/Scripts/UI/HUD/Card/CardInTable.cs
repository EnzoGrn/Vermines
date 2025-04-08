using UnityEngine;

namespace Vermines.UI.Card
{
    public class CardInTable : MonoBehaviour
    {
        CardDisplay cardBase;

        public void OpenOverlay()
        {
            Debug.Log("Affichage de l'overlay d'achat");
            //TableManager.instance.OpenSacrificeOverlay(cardBase);
        }

        public void SetCardBase(CardDisplay cardBase)
        {
            this.cardBase = cardBase;
        }
    }
}