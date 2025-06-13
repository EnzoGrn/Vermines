using UnityEngine;
using Vermines.CardSystem.Elements;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;
    using Image = UnityEngine.UI.Image;

    public class EquipmentBookSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text nameText;

        public void Setup(ICard card)
        {
            icon.sprite = card.Data.Sprite;
            nameText.text = card.Data.Name;
        }
    }
}
