using Fusion;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Vermines.UI
{
    using Vermines.UI.Screen;

    public class GameplayUIController : MonoBehaviour
    {
        #region Attributes

        /// <summary>
        /// The list of screens. The first one is the default screen shown on start.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameplayUIScreen[] _Screens;

        /// <summary>
        /// A type to screen lookup to support <see cref="Get{S}()"/>
        /// </summary>
        protected Dictionary<Type, GameplayUIScreen> _ScreenLookup;

        /// <summary>
        /// The last screen that was shown.
        /// </summary>
        protected GameplayUIScreen _LastScreen;

        /// <summary>
        /// The popup handler is automatically set if present based on the interface <see cref="IFusionMenuPopup"/>.
        /// </summary>
        protected Popup.GameplayUIPopup _PopupHandler;

        /// <summary>
        /// The dual button popup handler, if present.
        /// </summary>
        [SerializeField]
        protected Popup.GameplayUIDualButtonPopup _DualPopupHandler;

        /// <summary>
        /// The current active screen.
        /// </summary>
        protected GameplayUIScreen _ActiveScreen;

        #endregion

        #region Override Methods

        protected virtual void Awake()
        {
            _ScreenLookup = new Dictionary<Type, GameplayUIScreen>();

            foreach (var screen in _Screens)
            {
                var screenType = screen.GetType();

                screen.Controller = this;

                while (true)
                {
                    _ScreenLookup.Add(screenType, screen);

                    if (screenType.BaseType == null || typeof(GameplayUIScreen).IsAssignableFrom(screenType) == false || screenType.BaseType == typeof(GameplayUIScreen))
                        break;
                    screenType = screenType.BaseType;
                }

                if (screen is Popup.GameplayUIPopup popupHandler)
                    _PopupHandler = popupHandler;

                if (screen is Popup.GameplayUIDualButtonPopup dualPopup)
                    _DualPopupHandler = dualPopup;
            }

            foreach (var screen in _Screens)
            {
                screen.Init();
            }
        }

        protected virtual void Start()
        {
            if (_Screens != null && _Screens.Length > 0)
            {
                // Display the first screen by default
                _Screens[0].Show();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show a screen will automaticall disable the current active screen and call animations.
        /// </summary>
        /// <typeparam name="S">Screen type</typeparam>
        /// <param name="last">Last screen (can be null)</param>
        public virtual void Show<S>(GameplayUIScreen last = null) where S : GameplayUIScreen
        {
            if (_ScreenLookup.TryGetValue(typeof(S), out var result))
            {
                _LastScreen = last;

                if (!result.IsModal && _ActiveScreen != result && _ActiveScreen)
                    _ActiveScreen.Hide();
                if (_ActiveScreen != result)
                    result.Show();
                if (!result.IsModal)
                    _ActiveScreen = result;
            }
            else
            {
                Debug.LogError($"Show() - Screen type '{typeof(S).Name}' not found.");
            }
        }

        /// <summary>
        /// Show a screen of type <typeparamref name="S"/> with a parameter of type <typeparamref name="T"/>.
        /// Automatically hides the current active screen if the new one is not modal.
        /// </summary>
        /// <typeparam name="S">Screen type to show, must implement <see cref="IParamReceiver{T}"/>.</typeparam>
        /// <typeparam name="T">Parameter type to pass to the screen.</typeparam>
        /// <param name="param">Parameter value passed to the screen.</param>
        /// <param name="last">Optional last screen (can be null).</param>
        public virtual void ShowWithParams<S, T>(T param, GameplayUIScreen last = null) where S : GameplayUIScreen, IParamReceiver<T>
        {
            if (_ScreenLookup.TryGetValue(typeof(S), out var result))
            {
                _LastScreen = last;

                // Debug last screen type and current screen type
                Debug.Log($"Last screen type: {_LastScreen?.GetType().Name}, Current screen type: {result.GetType().Name}");

                if (!result.IsModal && _ActiveScreen != result && _ActiveScreen)
                    _ActiveScreen.Hide();

                if (result is IParamReceiver<T> receiver)
                {
                    receiver.SetParam(param);
                }

                result.Show();

                if (!result.IsModal)
                    _ActiveScreen = result;
            }
            else
            {
                Debug.LogError($"ShowWithParams() - Screen type '{typeof(S).Name}' not found.");
            }
        }

        /// <summary>
        /// Show a screen will automaticall disable the current active screen and call animations.
        /// </summary>
        /// <param name="screenToShow">Screen to show</param>
        public virtual void Show(GameplayUIScreen screenToShow)
        {
            if (_ScreenLookup.TryGetValue(screenToShow.GetType(), out var result))
            {
                if (!result.IsModal && _ActiveScreen != result && _ActiveScreen)
                    _ActiveScreen.Hide();
                if (_ActiveScreen != result)
                    result.Show();
                if (!result.IsModal)
                    _ActiveScreen = result;
                _LastScreen = null;
            }
            else
            {
                Debug.LogError($"Show() - Screen type '{screenToShow.GetType().Name}' not found.");
            }
        }

        /// <summary>
        /// Show the last screen that was shown.
        /// </summary>
        public virtual void ShowLast()
        {
            if (_LastScreen == null)
            {
                //Show<GameplayUIMain>();
            }
            else
            {
                var lastScreenType = _LastScreen.GetType();

                Show(_ScreenLookup[lastScreenType]);
            }
        }

        public virtual void Hide()
        {
            if (_ActiveScreen != null)
            {
                Debug.Log($"Hiding screen: {_ActiveScreen.GetType().Name}");
                _ActiveScreen.Hide();
                _ActiveScreen = null;
            }
            ShowLast();
        }

        /// <summary>
        /// Get a screen based on type.
        /// </summary>
        /// <typeparam name="S">Screen type</typeparam>
        /// <returns>Screen object</returns>
        public virtual S Get<S>() where S : GameplayUIScreen
        {
            if (_ScreenLookup.TryGetValue(typeof(S), out var result))
                return result as S;
            Debug.LogError($"Show() - Screen type '{typeof(S).Name}' not found.");

            return null;
        }

        public virtual bool GetLastScreen(out GameplayUIScreen lastScreen)
        {
            lastScreen = _LastScreen;

            return lastScreen != null;
        }

        public virtual bool GetActiveScreen(out GameplayUIScreen activeScreen)
        {
            activeScreen = _ActiveScreen;
            return activeScreen != null;
        }

        public virtual async void ShowDualPopup(IDiscardPopupStrategy strategy)
        {
            bool? result = await PopupDualAsync(
                strategy.GetMessage(),
                strategy.GetTitle(),
                strategy.GetCancelText(),
                strategy.GetConfirmText()
            );

            if (result == true)
            {
                strategy.OnConfirm();
            }
            else
            {
                strategy.OnCancel();
            }
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
            if (_PopupHandler == null)
            {
                Debug.LogError("Popup() - no popup handler found");

                return Task.CompletedTask;
            }

            return _PopupHandler.OpenPopupAsync(message, header);
        }

        public void PopupDual(string message, string header, string leftButtonLabel, string rightButtonLabel, Action<bool?> callback)
        {
            if (_DualPopupHandler == null)
            {
                Debug.LogError("PopupDual() - no dual popup handler found");
                callback?.Invoke(null);
                return;
            }

            _DualPopupHandler.OpenDualButtonPopupAsync(message, header, leftButtonLabel, rightButtonLabel)
                             .ContinueWith(t => callback?.Invoke(t.Result));
        }

        public Task<bool> PopupDualAsync(string message, string header, string leftButtonLabel, string rightButtonLabel)
        {
            if (_DualPopupHandler == null)
            {
                Debug.LogError("PopupDualAsync() - no dual popup handler found");
                return Task.FromResult<bool>(false);
            }

            return _DualPopupHandler.OpenDualButtonPopupAsync(message, header, leftButtonLabel, rightButtonLabel);
        }

        #endregion
    }
}
