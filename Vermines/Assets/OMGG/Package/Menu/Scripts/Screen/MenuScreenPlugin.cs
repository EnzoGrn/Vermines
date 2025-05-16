using Fusion;

namespace OMGG.Menu.Screen {

    /// <summary>
    /// Screen plugin are usually a UI features that is shared between multiple screens.
    /// The plugin must be registered at <see cref="MenuUIScreen.Plugins"/> and receieve Show and Hide callbacks.
    /// </summary>
    public class MenuScreenPlugin : FusionMonoBehaviour {

        /// <summary>
        /// The parent screen to init.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public virtual void Init(MenuUIScreen screen) { }

        /// <summary>
        /// The parent screen is shown.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public virtual void Show(MenuUIScreen screen) { }

        /// <summary>
        /// The parent screen is hidden.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public virtual void Hide(MenuUIScreen screen) { }
    }
}
