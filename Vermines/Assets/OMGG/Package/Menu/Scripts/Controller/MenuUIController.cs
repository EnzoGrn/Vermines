using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.Serialization;
using UnityEngine;
using Fusion;

namespace OMGG.Menu.Controller {

    using OMGG.Menu.Connection.Data;
    using OMGG.Menu.Connection.Element;
    using OMGG.Menu.Configuration;
    using OMGG.Menu.Connection;
    using OMGG.Menu.Screen;

    public class MenuUIController : FusionMonoBehaviour {

        #region Attributes

        /// <summary>
        /// The menu config.
        /// </summary>
        [InlineHelp, SerializeField]
        public ServerConfig Config;

        /// <summary>
        /// The connection wrapper.
        /// </summary>
        [FormerlySerializedAs("_connection"), InlineHelp, SerializeField]
        public MenuConnectionBehaviour Connection;

        /// <summary>
        /// The list of screens. The first one is the default screen shown on start.
        /// </summary>
        [InlineHelp, SerializeField]
        protected MenuUIScreen[] _Screens;

        /// <summary>
        /// A type to screen lookup to support <see cref="Get{S}()"/>
        /// </summary>
        protected Dictionary<Type, MenuUIScreen> _ScreenLookup;

        /// <summary>
        /// The last screen that was shown.
        /// </summary>
        protected MenuUIScreen _LastScreen;

        /// <summary>
        /// The screen to show at connection.
        /// </summary>
        [InlineHelp, SerializeField]
        protected MenuUIScreen _ScreenToShowOnConnect;

        /// <summary>
        /// The popup handler is automatically set if present based on the interface <see cref="Screen.Popup"/>.
        /// </summary>
        protected Popup _PopupHandler;

        /// <summary>
        /// The current active screen.
        /// </summary>
        protected MenuUIScreen _ActiveScreen;

        /// <summary>
        /// A factory to create SDK dependend derived connection args.
        /// </summary>
        protected virtual ConnectionArgs CreateConnectArgs() => throw (new NotImplementedException("CreateConnectArgs() - This method should be overridden in a derived class."));

        /// <summary>
        /// Current connection args. Shared by all screens.
        /// </summary>
        [NonSerialized]
        public ConnectionArgs ConnectionArgs;

        #endregion

        #region Override Methods

        protected virtual void Awake()
        {
            ConnectionArgs = CreateConnectArgs();
            _ScreenLookup  = new Dictionary<Type, MenuUIScreen>();

            foreach (var screen in _Screens) {
                var screenType = screen.GetType();

                screen.Config         = Config;
                screen.Connection     = Connection;
                screen.ConnectionArgs = ConnectionArgs;
                screen.Controller     = this;

                while (true) {
                    _ScreenLookup.Add(screenType, screen);

                    if (screenType.BaseType == null || typeof(MenuUIScreen).IsAssignableFrom(screenType) == false || screenType.BaseType == typeof(MenuUIScreen))
                        break;
                    screenType = screenType.BaseType;
                }

                if (screen is Popup popupHandler)
                    _PopupHandler = popupHandler;
            }

            foreach (var screen in _Screens)
                screen.Init();
        }

        protected virtual void Start()
        {
            if (_Screens != null && _Screens.Length > 0) {
                _Screens[0].Show();
                _ActiveScreen = _Screens[0];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show a screen will automaticall disable the current active screen and call animations.
        /// </summary>
        /// <typeparam name="S">Screen type</typeparam>
        /// <param name="last">Last screen (can be null)</param>
        public virtual void Show<S>(MenuUIScreen last = null) where S : MenuUIScreen
        {
            if (_ScreenLookup.TryGetValue(typeof(S), out var result)) {
                _LastScreen = last;

                if (!result.IsModal && _ActiveScreen != result && _ActiveScreen)
                    _ActiveScreen.Hide();
                if (_ActiveScreen != result)
                    result.Show();
                if (!result.IsModal)
                    _ActiveScreen = result;
            } else
                Debug.LogError($"Show() - Screen type '{typeof(S).Name}' not found.");
        }

        /// <summary>
        /// Show a screen will automaticall disable the current active screen and call animations.
        /// </summary>
        /// <param name="screenToShow">Screen to show</param>
        public virtual void Show(MenuUIScreen screenToShow)
        {
            if (_ScreenLookup.TryGetValue(screenToShow.GetType(), out var result)) {
                if (!result.IsModal && _ActiveScreen != result && _ActiveScreen)
                    _ActiveScreen.Hide();
                if (_ActiveScreen != result)
                    result.Show();
                if (!result.IsModal)
                    _ActiveScreen = result;
                _LastScreen = null;
            } else
                Debug.LogError($"Show() - Screen type '{screenToShow.GetType().Name}' not found.");
        }

        /// <summary>
        /// Show the last screen that was shown.
        /// </summary>
        public virtual void ShowLast()
        {
            if (_LastScreen == null) {
                Debug.LogError($"ShowLast() - No last screen found. Return to the main menu.");

                Show<MenuUIMain>();
            } else {
                var lastScreenType = _LastScreen.GetType();

                Show(_ScreenLookup[lastScreenType]);
            }
        }

        /// <summary>
        /// Get a screen based on type.
        /// </summary>
        /// <typeparam name="S">Screen type</typeparam>
        /// <returns>Screen object</returns>
        public virtual S Get<S>() where S : MenuUIScreen
        {
            if (_ScreenLookup.TryGetValue(typeof(S), out var result))
                return result as S;
            Debug.LogError($"Show() - Screen type '{typeof(S).Name}' not found.");

            return null;
        }

        public virtual bool GetLastScreen(out MenuUIScreen lastScreen)
        {
            lastScreen = _LastScreen;

            return lastScreen != null;
        }

        /// <summary>
        /// Show the popup/notification.
        /// </summary>
        /// <param name="message">Popup message</param>
        /// <param name="header">Popup header</param>
        public void Popup(string message, string header = default)
        {
            if (_PopupHandler == null)
                Debug.LogError("Popup() - no popup handler found");
            else
                _PopupHandler.OpenPopup(message, header);
        }

        /// <summary>
        /// Show the popup but wait until it hides.
        /// </summary>
        /// <param name="message">Popup message</param>
        /// <param name="header">Popup header</param>
        /// <returns>When the user clicked okay.</returns>
        public Task PopupAsync(string message, string header = default)
        {
            if (_PopupHandler == null) {
                Debug.LogError("Popup() - no popup handler found");

                return Task.CompletedTask;
            }
            return _PopupHandler.OpenPopupAsync(message, header);
        }

        /// <summary>
        /// Default connection error handling is reused in a couple places.
        /// </summary>
        /// <param name="result">Connect result</param>
        /// <param name="controller">UI Controller</param>
        /// <returns>When handling is completed</returns>
        public virtual async Task HandleConnectionResult(ConnectResult result, MenuUIController controller)
        {
            return;
        }

        public virtual async Task HandleSceneChangeResult(ConnectResult result, MenuUIController controller, MenuUIScreen screenToShow)
        {
            return;
        }

        #endregion
    }
}
