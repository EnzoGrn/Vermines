namespace Vermines.UI.Plugin
{
    using Vermines.UI.Screen;
    using Vermines.ShopSystem.Enumerations;

    /// <summary>
    /// Manages the navigation button plugin.
    /// </summary>
    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
        #region Events

        /// <summary>
        /// Is called when the courtyard button is clicked.
        /// </summary>
        protected virtual void OnOpenCourtyard()
        {
            _ParentScreen.Controller.ShowWithParams<GameplayUIShop, ShopType>(ShopType.Courtyard);
        }

        #endregion
    }
}