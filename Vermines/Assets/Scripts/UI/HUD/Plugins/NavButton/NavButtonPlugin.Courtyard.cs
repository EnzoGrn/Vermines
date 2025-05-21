namespace Vermines.UI.Plugin
{
    using Vermines.UI.Screen;
    using Vermines.ShopSystem.Enumerations;
    using System.Collections.Generic;
    using System;

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
            GameplayUIController gameplayUIController = _ParentScreen.Controller;
            if (gameplayUIController != null)
            {
                gameplayUIController.Hide();
            }

            CamManager.Instance.GoOnCourtyardLocation();
        }

        #endregion
    }
}