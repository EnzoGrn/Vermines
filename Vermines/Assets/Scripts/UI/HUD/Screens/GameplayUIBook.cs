using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Plugin;
using Fusion;
using Vermines.CardSystem.Enumerations;
using Vermines.UI.Card;
using System.Collections.Generic;
using Vermines.Player;
using System.Collections;
using System.Linq;


namespace Vermines.UI.Screen
{
    using Button = UnityEngine.UI.Button;

    public partial class GameplayUIBook : GameplayUIScreen
    {
        #region Attributes

        protected override bool ShouldShowPlugins => false;

        public enum BookTabType { Profile, Rules, None }

        [SerializeField] private BookTabType _DefaultPage = BookTabType.Profile;

        private Dictionary<BookTabType, GameplayScreenPlugin> _BookPages;
        private BookTabType _CurrentPage;

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            _BookPages = new Dictionary<BookTabType, GameplayScreenPlugin>();
            _CurrentPage = BookTabType.None;

            foreach (var plugin in Plugins)
            {
                if (plugin is BookPagePlugin bookPage)
                {
                    _BookPages[bookPage.PageType] = bookPage;
                    bookPage.Hide();
                }

                if (plugin is RulesBookPlugin ruleBookTab)
                {
                    Debug.Log($"Adding RulesBookPlugin for page type: {ruleBookTab.PageType}");
                    _BookPages[ruleBookTab.PageType] = ruleBookTab;
                    ruleBookTab.Hide();
                }

                if (plugin is PlayerBookTabPlugin playerTabsPlugin)
                {
                    BookPagePlugin bookPagePlugin = Plugins
                        .OfType<BookPagePlugin>()
                        .FirstOrDefault();

                    if (bookPagePlugin != null)
                    {
                        playerTabsPlugin.OnTabClicked += bookPagePlugin.ShowPlayerInfo;
                    }
                }
            }

        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowScreenCoroutine());

            ShowUser();
        }

        private IEnumerator ShowScreenCoroutine()
        {
            yield return base.ShowRoutine();

            foreach (var plugin in Plugins)
            {
                if (plugin is PlayerBookTabPlugin playerBookTab)
                {
                    playerBookTab.Show(this);
                    yield return playerBookTab.ShowPlayerTabsSequentially();
                }
                else if (plugin is RuleBookTabPlugin bookPagePlugin)
                {
                    bookPagePlugin.Show(this);
                }
                else if (plugin is BookPagePlugin bookPage)
                {
                    bookPage.ShowPlayerInfo(GameDataStorage.Instance.PlayerData[PlayerController.Local.PlayerRef]);
                }
                else if (plugin is PlayerBookTabPlugin bookTab)
                {
                    bookTab.SetPlayerTabActive(GameDataStorage.Instance.PlayerData[PlayerController.Local.PlayerRef]);
                }
            }
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            // Hide the current page if it exists
            if (_BookPages.TryGetValue(_CurrentPage, out var currentPage))
                currentPage.Hide();
            _CurrentPage = BookTabType.None; // Reset current page
            base.Hide();

            HideUser();
        }

        #endregion

        #region Methods

        public void SwitchToPage()
        {
            SwitchToPage(_DefaultPage);
        }

        public void SwitchToPage(BookTabType newPage)
        {
            if (_CurrentPage == newPage)
                return;

            Debug.Log($"Switching to page: {newPage}, Current Page: {_CurrentPage}");

            if (_BookPages.TryGetValue(_CurrentPage, out var oldPage))
                oldPage.Hide();

            _CurrentPage = newPage;

            if (_CurrentPage != BookTabType.Profile)
            {
                PlayerBookTabPlugin playerTabPlugin = Plugins
                    .OfType<PlayerBookTabPlugin>()
                    .FirstOrDefault();
                if (playerTabPlugin != null)
                {
                    playerTabPlugin.SetPlayerTabActive(false);
                }
            }

            if (_BookPages.TryGetValue(_CurrentPage, out var newPageObj))
            {
                Debug.Log($"Showing new page: {newPageObj.name}");
                newPageObj.Show(this);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            StartCoroutine(CloseScreen());
        }

        private IEnumerator CloseScreen()
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is PlayerBookTabPlugin bookPage)
                {
                    yield return bookPage.HidePlayerTabsSequentially();
                }
                else if (plugin is RuleBookTabPlugin bookPagePlugin)
                {
                    bookPagePlugin.Hide();
                }
            }

            // Hide the current page if it exists
            if (_BookPages.TryGetValue(_CurrentPage, out var currentPage))
                currentPage.Hide();

            _CurrentPage = BookTabType.None;
            Controller.Hide();
        }

        #endregion

    }
}