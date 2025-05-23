using UnityEngine;

namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Defines a contract for a gameplay screen plugin that can receive a parameter of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of parameter this plugin expects.</typeparam>
    public interface IGameplayScreenPluginParam<in T>
    {
        /// <summary>
        /// Sets the plugin's parameter with a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="param">The parameter value to configure the plugin.</param>
        void SetParam(T param);
    }

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
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">The parent screen that this plugin is attached to.</param>
        public virtual void Show(GameplayUIScreen screen)
        {
            _ParentScreen = screen;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public virtual void Hide() { }
    }
}