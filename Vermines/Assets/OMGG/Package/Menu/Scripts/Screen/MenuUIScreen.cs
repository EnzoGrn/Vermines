using System.Collections.Generic;
using System.Collections;
using Fusion;
using UnityEngine;

namespace OMGG.Menu.Screen {
    using OMGG.Menu.Configuration;
    using OMGG.Menu.Connection.Data;
    using OMGG.Menu.Connection.Element;
    using OMGG.Menu.Controller;

    /// <summary>
    /// The screen base class contains a lot of accessors (e.g. Config, Connection, ConnectArgs) for convenient access.
    /// </summary>
    public abstract class MenuUIScreen : FusionMonoBehaviour {

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
        private List<MenuScreenPlugin> _Plugins;

        /// <summary>
        /// The list of screen plugins.
        /// </summary>
        public List<MenuScreenPlugin> Plugins => _Plugins;

        /// <summary>
        /// Is the screen currently showing.
        /// </summary>
        public bool IsShowing { get; private set; }

        #endregion

        #region Configuration & Connection

        /// <summary>
        /// The menu config, assigned by the <see cref="MenuUIController" />
        /// </summary>
        public ServerConfig Config { get; set; }

        /// <summary>
        /// The menu connection object, The menu config, assigned by the <see cref="MenuUIController"/>.
        /// </summary>
        public MenuConnectionBehaviour Connection { get; set; }

        /// <summary>
        /// The menu connection args.
        /// </summary>
        public ConnectionArgs ConnectionArgs { get; set; }

        #endregion

        /// <summary>
        /// The menu UI controller that owns this screen.
        /// </summary>
        public MenuUIController Controller { get; set; }

        #region Override Methods

        public virtual void Awake() { }

        public virtual void Start()
        {
            TryGetComponent(out _Animator);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The screen init method is called during <see cref="MenuUIController{T}.Awake()"/> after all screen have been assigned and configured.
        /// </summary>
        public virtual void Init()
        {
            foreach (MenuScreenPlugin plugin in _Plugins)
                plugin.Init(this);
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public virtual void Hide()
        {
            if (_Animator) {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(HideAnimCoroutine());

                return;
            }

            IsShowing = false;

            foreach (MenuScreenPlugin plugin in _Plugins)
                plugin.Hide(this);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public virtual void Show()
        {
            if (_HideCoroutine != null) {
                StopCoroutine(_HideCoroutine);

                if (_Animator.gameObject.activeInHierarchy && _Animator.HasState(0, ShowAnimHash))
                    _Animator.Play(ShowAnimHash, 0, 0);
            }

            gameObject.SetActive(true);

            IsShowing = true;

            foreach (MenuScreenPlugin plugin in _Plugins)
                plugin.Show(this);
        }

        /// <summary>
        /// Play the hide animation wrapped in a coroutine.
        /// Forces the target framerate to 60 during the transition animations.
        /// </summary>
        /// <returns>When done</returns>
        private IEnumerator HideAnimCoroutine()
        {
            #if UNITY_IOS || UNITY_ANDROID
                var changedFramerate = false;
                
                if (Config.AdaptFramerateForMobilePlatform) {
                    if (Application.targetFrameRate < 60) {
                        Application.targetFrameRate = 60;
                        changedFramerate            = true;
                    }
                }
            #endif

            _Animator.Play(HideAnimHash);

            yield return null;

            while (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;
            #if UNITY_IOS || UNITY_ANDROID
                if (changedFramerate)
                new FusionMenuGraphicsSettings().Apply();
            #endif

            gameObject.SetActive(false);
        }

        #endregion
    }
}
