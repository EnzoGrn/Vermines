using UnityEngine;

namespace Vermines.UI.Plugin {

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
            // if player is in a replace context, ignore the click
            if (UIContextManager.Instance.IsInContext<ReplaceEffectContext>())
            {
                Debug.Log("[NavButtonPlugin] Ignoring courtyard button click while in ReplaceEffectContext.");
                return;
            }
            GameplayUIController gameplayUIController = _ParentScreen.Controller;

            if (gameplayUIController != null)
                gameplayUIController.Hide();

            CamManager camera = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            if (camera != null)
                camera.GoOnMarketLocation();
            else
                Debug.LogWarning("CamManager not found, cannot navigate to market location.");
        }

        #endregion
    }
}