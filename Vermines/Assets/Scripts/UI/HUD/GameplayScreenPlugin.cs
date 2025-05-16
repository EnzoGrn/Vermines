using UnityEngine;

namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Screen plugin are usually a UI features that is shared between multiple screens.
    /// The plugin must be registered at <see cref="GameplayUIScreen.Plugins"/> and receieve Show and Hide callbacks.
    /// </summary>
    public class GameplayScreenPlugin : MonoBehaviour
    {
        /// <summary>
        /// The parent screen.
        /// </summary>
        protected GameplayUIScreen _ParentScreen;

        /// <summary>
        /// The parent screen is shown.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public virtual void Show(GameplayUIScreen screen)
        {
            _ParentScreen = screen;
        }

        /// <summary>
        /// The parent screen is hidden.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public virtual void Hide(GameplayUIScreen screen) { }
    }
}