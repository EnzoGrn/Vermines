using UnityEngine;
using UnityEngine.UI;

namespace Vermines.HUD.Card
{
    public class CardInShop : MonoBehaviour
    {
        public CardBase CardBase;

        public void Initialize(CardBase cardBase)
        {
            CardBase = cardBase;
        }

        public void OpenOverlay()
        {
            ShopManager.instance.GetShop().OpenCardBuyOverlay(CardBase);
        }
    }
}