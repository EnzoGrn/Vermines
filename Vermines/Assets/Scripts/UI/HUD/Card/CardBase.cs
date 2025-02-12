using UnityEngine;
using TMPro;

namespace Vermines.HUD
{
    using Vermines.CardSystem.Data;

    public class CardBase : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI cost;
        [SerializeField] protected TextMeshProUGUI effectDescription;

        [SerializeField]
        [Tooltip("Fill this only in debug mode.")]
        protected CardData cardData;

        public virtual void Setup(CardData newCardData)
        {
            cardData = newCardData;
            //UpdateUI();
        }

        public CardData GetCardData()
        {
            return cardData;
        }

        //protected void UpdateUI()
        //{
        //    if (cardData != null)
        //    {
        //        title.text = cardData.Name;
        //        cost.text = cardData.Eloquence.ToString();
        //        effectDescription.text = cardData.Description;
        //    }
        //}
    }
}