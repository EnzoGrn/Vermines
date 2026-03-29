using TMPro;
using UnityEngine;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;

namespace Vermines.UI.Card
{
    public class ShopCardSlot : CardSlotBase
    {
        [Header("Stack Count")]
        [SerializeField] private GameObject _stackCountContainer;
        [SerializeField] private TMP_Text _stackCountText;
        private void Awake()
        {
            _acceptedType = CardType.None;
            IsInteractable = false;
        }

        public void ShowStackCount(int count)
        {
            if (_stackCountContainer == null || _stackCountText == null) return;

            Debug.LogFormat(
                "[{0}] Showing stack count for card ID {1}: {2}",
                nameof(ShopCardSlot),
                CardDisplay.Card.ID,
                count
            );

            bool shouldShow = count > 1;
            _stackCountContainer.SetActive(shouldShow);

            if (shouldShow)
                _stackCountText.text = $"x{count}";
        }

        public void HideStackCount()
        {
            if (_stackCountContainer != null)
                _stackCountContainer.SetActive(false);
        }

        public override bool CanAcceptCard(ICard card)
        {
            return false;
        }

        public override void ResetSlot()
        {
            base.ResetSlot();
            HideStackCount();
        }
    }
}