using UnityEngine;
namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Manages the navigation button plugin.
    /// </summary>
    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
        #region Events

        /// <summary>
        /// Is called when the market button is clicked.
        /// </summary>
        protected virtual void OnOpenMarket()
        {
            Debug.Log("[UI]: Open Market button clicked.");
        }

        #endregion
    }
}