using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using Vermines.Core;
    using Vermines.Core.UI;
    using Vermines.Menu.View;
    using Vermines.Player;
    using Vermines.UI.View;

    /// <summary>
    /// Entry point of the tutorial in the game (Tutorial button of the main menu).
    /// Creates once, and keeps between scenes: the TutorialManager and its UI,
    /// the bridge (VerminesTutorialBridge), the target binder and the lesson menu.
    /// </summary>
    public class VerminesTutorialDirector : MonoBehaviour {

        #region Attributes

        public static VerminesTutorialDirector Instance { get; private set; }

        [SerializeField]
        [Tooltip("Logs every tutorial event and every target ID put on the game UI.")]
        private bool _VerboseLogs;

        [SerializeField]
        [Tooltip("How long to wait for the main menu after a lesson, to reopen the lesson list.")]
        private float _ReopenTimeout = 30f;

        private TutorialManager        _Manager;
        private VerminesTutorialBridge _Bridge;
        private TutorialTargetBinder   _Binder;
        private TutorialLessonMenu     _Menu;
        private Canvas                 _TutorialCanvas;
        private UIView                 _MainView;
        private string                 _GameScenePath;
        private float                  _ReopenUntil;

        public TutorialManager Manager => _Manager;

        #endregion

        #region Entry points

        public static void OpenLessonMenu(UIView mainView, string gameScenePath)
        {
            VerminesTutorialDirector director = Ensure();

            director._MainView      = mainView;
            director._GameScenePath = gameScenePath;
            director._ReopenUntil   = 0f;

            director.ShowMenu();
        }

        public static VerminesTutorialDirector Ensure()
        {
            if (Instance != null)
                return Instance;
            GameObject root = new("VerminesTutorialDirector");

            DontDestroyOnLoad(root);

            return root.AddComponent<VerminesTutorialDirector>();
        }

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;

            _Bridge = gameObject.AddComponent<VerminesTutorialBridge>();
            _Binder = gameObject.AddComponent<TutorialTargetBinder>();
            _Menu   = TutorialLessonMenu.Create(transform);

            _Bridge.MatchLost  += OnMatchLost;
            _Menu.LessonChosen += OnLessonChosen;
            _Menu.Closed       += OnMenuClosed;

            _Manager = TutorialUIBuilder.BuildSystem(VerminesTutorialContent.Build());

            _Manager.SetBridge(_Bridge);
            _Manager.Initialize(new PlayerPrefsTutorialProgressStore(), ProfileId());

            _Manager.StepStarted     += OnStepStarted;
            _Manager.LessonCompleted += OnLessonCompleted;
            _Manager.LessonAborted   += OnLessonAborted;

            _TutorialCanvas = _Manager.UI != null ? _Manager.UI.GetComponentInParent<Canvas>(true) : null;

            SetVerbose(_VerboseLogs);
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;
            Instance = null;

            if (_Manager != null)
                Destroy(_Manager.gameObject);
        }

        private void Update()
        {
            UpdateTutorialVisibility();
            TryReopenMenu();
        }

        #endregion

        #region Methods

        public void SetVerbose(bool value)
        {
            _VerboseLogs = value;

            if (_Manager != null)
                _Manager.VerboseLogs = value;
            if (_Binder != null)
                _Binder.LogNewIds = value;
        }

        private static string ProfileId()
        {
            string userID = Global.PlayerService?.PlayerData?.UserID;

            return string.IsNullOrEmpty(userID) ? "default" : userID;
        }

        private void ShowMenu()
        {
            SetMainViewActive(false);

            _Menu.Show(_Manager);
        }

        private void SetMainViewActive(bool active)
        {
            if (_MainView == null)
                return;
            _MainView.IsInteractable = active;

            // The signs of the main menu are 3D buttons: their listeners are removed while the list is open.
            if (_MainView is UIMainMenuView mainMenu) {
                if (active)
                    mainMenu.ActiveButton();
                else
                    mainMenu.DeactiveButton();
            }
        }

        /// <summary>The tutorial UI is hidden while the in-game menu (pause) is open.</summary>
        private void UpdateTutorialVisibility()
        {
            if (_TutorialCanvas == null || _Manager == null)
                return;
            bool show = !_Manager.IsRunning || !IsGameMenuOpen();

            if (_TutorialCanvas.enabled != show)
                _TutorialCanvas.enabled = show;
        }

        private UIGameplayMenu _GameMenu;

        private bool IsGameMenuOpen()
        {
            if (_GameMenu == null) {
                if (PlayerController.Local == null)
                    return false;
                _GameMenu = FindFirstObjectByType<UIGameplayMenu>(FindObjectsInactive.Exclude);
            }

            return _GameMenu != null && _GameMenu.NeedsCursor;
        }

        /// <summary>
        /// After a lesson, the list comes back as soon as the main menu is shown again.
        /// </summary>
        private void TryReopenMenu()
        {
            if (_ReopenUntil <= 0f)
                return;
            if (Time.unscaledTime > _ReopenUntil) {
                _ReopenUntil = 0f;

                return;
            }

            if (PlayerController.Local != null || _Manager.IsRunning)
                return;
            UIMainMenuView mainMenu = FindFirstObjectByType<UIMainMenuView>(FindObjectsInactive.Exclude);

            if (mainMenu == null || !mainMenu.IsOpen)
                return;
            _ReopenUntil = 0f;
            _MainView    = mainMenu;

            ShowMenu();
        }

        #endregion

        #region Events

        private void OnLessonChosen(TutorialLessonData lesson)
        {
            SetMainViewActive(true);

            _Bridge.GameScenePath = _GameScenePath;
            _MainView             = null;
            _GameMenu             = null;

            _Manager.StartLesson(lesson);
        }

        private void OnMenuClosed()
        {
            SetMainViewActive(true);

            _MainView = null;
        }

        private void OnStepStarted(TutorialStepData step, int index)
        {
            _Binder.Scan();
            _Binder.ResolveNamedTargets(step.targetIds);

            if (!string.IsNullOrEmpty(step.pointerDropTargetId))
                _Binder.ResolveNamedTargets(new[] { step.pointerDropTargetId });
        }

        private void OnLessonCompleted(TutorialLessonData lesson, bool skipped)
        {
            _ReopenUntil = Time.unscaledTime + _ReopenTimeout;
        }

        private void OnLessonAborted(TutorialLessonData lesson)
        {
            _ReopenUntil = Time.unscaledTime + _ReopenTimeout;
        }

        private void OnMatchLost()
        {
            if (_Manager.IsRunning)
                _Manager.AbortLesson();
        }

        #endregion
    }
}
