using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Plugin;
using Fusion;
using Vermines.CardSystem.Enumerations;
using Vermines.UI.Card;
using System.Collections.Generic;
using Vermines.Player;

namespace Vermines.UI.Screen
{
    using Button = UnityEngine.UI.Button;

    public partial class GameplayUIBook : GameplayUIScreen
    {
        #region Attributes

        protected override bool ShouldShowPlugins => false;

        public enum BookTabType { Profile, Map, Inventory, Quests, Settings, None }

        [SerializeField] private BookTabType _DefaultPage = BookTabType.Profile;

        private Dictionary<BookTabType, BookPagePlugin> _BookPages;
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

            _BookPages = new Dictionary<BookTabType, BookPagePlugin>();
            _CurrentPage = BookTabType.None;

            foreach (var plugin in Plugins)
            {
                if (plugin is BookPagePlugin bookPage)
                {
                    _BookPages[bookPage.PageType] = bookPage;
                    bookPage.Hide(); // toutes les pages sont masquées au début
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
            base.Show();
            
            ShowUser();
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
            Debug.Log($"Switching to default page: {_DefaultPage}");
            SwitchToPage(_DefaultPage);
        }

        public void SwitchToPage(BookTabType newPage)
        {
            if (_CurrentPage == newPage)
                return;

            if (_BookPages.TryGetValue(_CurrentPage, out var oldPage))
                oldPage.Hide();

            _CurrentPage = newPage;

            if (_BookPages.TryGetValue(_CurrentPage, out var newPageObj))
                newPageObj.Show(this);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            Controller.Hide();
        }

        #endregion

    }
}