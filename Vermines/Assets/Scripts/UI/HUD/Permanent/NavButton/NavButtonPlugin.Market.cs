namespace Vermines.UI.Plugin
{

    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
        #region Events

        /// <summary>
        /// Is called when the market button is clicked.
        /// </summary>
        protected virtual void OnOpenMarket()
        {
            TryNavigate(camManager => camManager.GoOnMarketLocation());
        }

        #endregion
    }
}
