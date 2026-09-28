using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.UI
{
    using System.Collections;
    using Vermines.Core.UI;
    using Vermines.UI.Plugin;

    public abstract class GameplayUIScreen : MonoBehaviour, IBackHandler
    {
        #region Animation

        /// <summary>
        /// Cached 'Hide' animation hash.
        /// </summary>
        protected static readonly int HideAnimHash = Animator.StringToHash("Hide");

        /// <summary>
        /// Cached 'Show' animation hash.
        /// </summary>
        protected static readonly int ShowAnimHash = Animator.StringToHash("Show");

        /// <summary>
        /// The animator component.
        /// </summary>
        private Animator _Animator;

        /// <summary>
        /// The hide animation coroutine.
        /// </summary>
        private Coroutine _HideCoroutine;

        #endregion

        #region Properties

        /// <summary>
        /// Is modal is a flag, that must be set for overlay screens.
        /// </summary>
        [InlineHelp, SerializeField]
        private bool _IsModal;

        /// <summary>
        /// Is model property.
        /// </summary>
        public bool IsModal => _IsModal;

        /// <summary>
        /// The list of screen plugins for the screen.
        /// The actual plugin scripts can be distributed inside the UI hierarchy.
        /// </summary>
        [InlineHelp, SerializeField]
        private List<GameplayScreenPlugin> _Plugins;

        /// <summary>
        /// The list of screen plugins.
        /// </summary>
        public List<GameplayScreenPlugin> Plugins => _Plugins;

        /// <summary>
        /// Should show plugins is a flag that can be used to hide the plugin UI elements.
        /// </summary>
        protected virtual bool ShouldShowPlugins => true;

        protected virtual bool ShouldHidePlugins => true;

        /// <summary>
        /// Is the screen currently showing.
        /// </summary>
        public bool IsShowing { get; private set; }

        // Fired once, then cleared - mirrors UIView's HasOpened/HasClosed.
        // A generic, reusable "wait for the next open/close" hook, ported
        // from UIView. NOT wired into SceneInput's IBackHandler dispatch:
        // the gameplay screen system stays fully independent from the menu
        // UIView/SceneUI system by design (not merged, only sharing the
        // interface shape). If Escape should ever close a gameplay screen,
        // that belongs to GameplayUIController's own input handling, not a
        // shared SceneInput registration.
        public event Action HasOpened;
        public event Action HasClosed;

        // Whether this screen currently reacts to a back action, if
        // something in the gameplay UI ever calls OnBackAction() directly
        // (e.g. a future dedicated Escape handler owned by
        // GameplayUIController). Off by default: most screens shouldn't
        // silently swallow a back action just by being shown.
        [InlineHelp, SerializeField]
        private bool _CanHandleBackAction;

        [InlineHelp, SerializeField]
        private int _Priority;

        #endregion

        /// <summary>
        /// The menu UI controller that owns this screen.
        /// </summary>
        public GameplayUIController Controller { get; set; }

        #region Override Methods

        public virtual void Awake()
        {
            TryGetComponent(out _Animator);
        }

        public virtual void Start() { }

        #endregion

        #region Methods

        /// <summary>
        /// The screen init method is called during <see cref="GameplayUIController{T}.Awake()"/> after all screen have been assigned and configured.
        /// </summary>
        public virtual void Init() { }

        public void SetPriority(int priority)
        {
            _Priority = priority;
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public virtual void Hide()
        {
            if (_Animator != null && _Animator.gameObject.activeInHierarchy && _Animator.HasState(0, HideAnimHash))
            {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(HideAnimCoroutine());

                return;
            }

            HideImmediate();
        }

        private void HideImmediate()
        {
            IsShowing = false;

            if (ShouldHidePlugins)
            {
                foreach (GameplayScreenPlugin plugin in _Plugins)
                    plugin.Hide();
            }

            gameObject.SetActive(false);

            if (HasClosed != null)
            {
                HasClosed.Invoke();
                HasClosed = null;
            }
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public virtual void Show()
        {
            if (_HideCoroutine != null)
            {
                StopCoroutine(_HideCoroutine);

                if (_Animator.gameObject.activeInHierarchy && _Animator.HasState(0, ShowAnimHash))
                    _Animator.Play(ShowAnimHash, 0, 0);
            }

            gameObject.SetActive(true);

            IsShowing = true;

            if (ShouldShowPlugins)
            {
                foreach (GameplayScreenPlugin plugin in _Plugins)
                    plugin.Show(this);
            }

            if (HasOpened != null)
            {
                HasOpened.Invoke();
                HasOpened = null;
            }
        }

        public IEnumerator ShowRoutine()
        {
            if (_HideCoroutine != null)
            {
                StopCoroutine(_HideCoroutine);

                if (_Animator.gameObject.activeInHierarchy && _Animator.HasState(0, ShowAnimHash))
                    _Animator.Play(ShowAnimHash, 0, 0);
            }

            gameObject.SetActive(true);

            _Animator.Play(ShowAnimHash);

            yield return null;

            while (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;

            IsShowing = true;

            if (ShouldShowPlugins)
            {
                foreach (GameplayScreenPlugin plugin in _Plugins)
                    plugin.Show(this);
            }

            if (HasOpened != null)
            {
                HasOpened.Invoke();
                HasOpened = null;
            }
        }


        /// <summary>
        /// Get a screen plugin based on its type.
        /// </summary>
        /// <typeparam name="S">The type of the plugin to retrieve.</typeparam>
        /// <returns>The plugin of type S if found, otherwise null.</returns>
        public virtual S Get<S>() where S : GameplayScreenPlugin
        {
            foreach (var plugin in _Plugins)
            {
                if (plugin is S typedPlugin)
                    return typedPlugin;
            }

            Debug.LogError($"[GameplayScreen] Get<{typeof(S).Name}> - Plugin not found.");
            return null;
        }

        /// <summary>
        /// Play the hide animation wrapped in a coroutine.
        /// Forces the target framerate to 60 during the transition animations.
        /// </summary>
        /// <returns>When done</returns>
        private IEnumerator HideAnimCoroutine()
        {
            yield return AnimatedTransition.PlayAndWait(_Animator, HideAnimHash);
            HideImmediate();
        }

        #endregion

        #region IBackHandler

        int IBackHandler.Priority => _Priority;
        bool IBackHandler.IsActive => IsShowing && _CanHandleBackAction;

        bool IBackHandler.OnBackAction()
        {
            return OnBackAction();
        }

        protected virtual bool OnBackAction()
        {
            Hide();

            return true;
        }

        #endregion
    }
}
