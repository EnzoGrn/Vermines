using Vermines.CardSystem.Elements;

namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Manage the display of the remove popup in the gameplay screen.
    /// </summary>
    public class CopyPopupPlugin : CardPopupBase
    {
        #region Methods

        /// <summary>
        /// Injects content and sets up the popup with card data and optional replace mode.
        /// </summary>
        protected override void SetupBase(ICard card)
        {
            base.SetupBase(card);

            SetupButtons(confirmButton, cancelButton);
        }

        protected override void OnConfirm()
        {
            GameEvents.OnEffectSelectCard.Invoke(_cardData);
            Hide();
        }

        protected override void OnCancel()
        {
            Hide();
        }

        #endregion
    }
}