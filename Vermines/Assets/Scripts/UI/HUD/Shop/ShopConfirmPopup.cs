using Fusion;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Shop;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// A popup for confirming the purchase of a card in the shop.
    /// </summary>
    public class ShopPopupPlugin : CardPopupBase
    {
        #region Attributes

        /// <summary>
        /// The text component for displaying the cost of the card.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text costText;

        /// <summary>
        /// The text component for displaying the question to the player (e.g., "Buy this card?").
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text questionText;

        /// <summary>
        /// The action to be invoked when the card is bought.
        /// </summary>
        protected System.Action<ICard> _onBuy;

        protected ShopType _shopType = ShopType.Market;

        protected bool _isReplace = false;

        #endregion

        #region Override Methods

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        public void Setup(System.Action<ICard> onBuy, bool isReplace, ShopType shopType)
        {
            _onBuy = onBuy;
            _isReplace = isReplace;
            _shopType = shopType;
        }

        /// <summary>
        /// Injects content and sets up the popup with card data and optional replace mode.
        /// </summary>
        protected override void SetupBase(ICard card)
        {
            base.SetupBase(card);

            SetupButtons(confirmButton, cancelButton);

            // TODO: This needs to be changed with Localization later, using SmartString

            // If we have a free context, we write "Free" instead of the cost
            costText.text = UIContextManager.Instance.IsInContext<FreeCardContext>() && UIContextManager.Instance.GetContext<FreeCardContext>().ShopType == _shopType
                ? "Free"
                : $"Cost: {card.Data.CurrentEloquence} eloquences";

            questionText.text = _isReplace
                ? $"Replace {card.Data.Name} ?"
                : $"Buy {card.Data.Name} ?";

            var activeShop = GameObject.FindAnyObjectByType<ShopUIController>();

            if (activeShop != null)
            {
                activeShop.SetDialogueVisible(false);
            }
        }

        protected override void OnConfirm()
        {
            _onBuy?.Invoke(_cardData);

            Hide();

            if (UIContextManager.Instance.IsInContext<FreeCardContext>())
            {
                UIContextManager.Instance.PopContextOfType<FreeCardContext>();
            }

            var activeShop = GameObject.FindAnyObjectByType<ShopUIController>();

            if (activeShop is ShopUIController controller)
            {
                controller.SetDialogueVisible(true);
            }
        }

        protected override void OnCancel()
        {
            Hide();

            var activeShop = GameObject.FindAnyObjectByType<ShopUIController>();

            if (activeShop is ShopUIController controller)
            {
                controller.SetDialogueVisible(true);
            }
        }

        #endregion
    }
}