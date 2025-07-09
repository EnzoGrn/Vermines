using UnityEngine;

namespace Vermines.UI.Plugin {

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
                gameplayUIController.Hide();
            CamManager camera = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            if (camera != null)
                camera.GoOnCourtyardLocation();
            else
                Debug.LogWarning("CamManager not found, cannot navigate to courtyard location.");
        }

        #endregion
    }
}