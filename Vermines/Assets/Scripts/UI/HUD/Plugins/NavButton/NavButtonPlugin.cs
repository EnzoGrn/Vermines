using UnityEngine;

namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Manages the navigation button plugin.
    /// </summary>
    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
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

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            _ParentScreen.Controller.Hide();
            CamManager camManager = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);
            if (camManager != null)
            {
                camManager.GoOnNoneLocation();
            }
            else
            {
                Debug.LogWarning("CamManager not found, cannot navigate to courtyard location.");
            }
        }
    }
}