using Fusion;
using Fusion.Menu;

namespace Vermines {

    public class MenuUIController : FusionMenuUIController<FusionMenuConnectArgs> {

        /// <summary>
        /// The configuration of the menu.
        /// </summary>
        public FusionMenuConfig Config => _config;

        /// <summary>
        /// Selected game mode.
        /// The game mode is the enumeration state from Fusion, for define user connection mode.
        /// Default value is GameMode.AutoHostOrClient.
        /// </summary>
        public GameMode SelectedGameMode { get; protected set; } = GameMode.AutoHostOrClient;

        /// <summary>
        /// Function called when the game is started.
        /// </summary>
        public virtual void OnGameStarted() {}

        /// <summary>
        /// Function called when the game is stopped.
        /// </summary>
        public virtual void OnGameStopped() {}

    }
}
