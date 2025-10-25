using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Vermines.UI;
using System.Threading.Tasks;

namespace Vermines.Menu.Screen {

    /// <summary>
    /// Vermines Settings Menu UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_Settings : MenuUIScreen {

        #region Navigation

        [Header("Navigation")]

        /// <summary>
        /// The back button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _BackButton;

        [Header("Book Navigation")]

        [SerializeField]
        private List<BookCategories> _Categories;

        private BookCategories _CurrentSelectedCategory;

        #endregion

        #region Content

        [Header("Content")]

        [SerializeField]
        private GameObject _Content;

        #endregion

        #region Partial Methods

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #endregion

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

        #endregion

        #region Methods

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();

            _Content?.SetActive(gameObject.activeSelf);

            foreach (var category in _Categories) {
                // category.Init(this);
                category.SetActiveCategorie(false);
            }
            SelectCategory(_Categories[0]);
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// </summary>
        public override async void Show()
        {
            if (Controller.GetLastScreen(out MenuUIScreen screen) && screen.GetType() == typeof(VMUI_MainMenu))
                (screen as VMUI_MainMenu)?.DeactiveButton();
            base.Show();

            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;

            await Task.Delay((int)(duration * 1000));

            _Content?.SetActive(true);

            SelectCategory(_Categories[0]);

            ShowUser();
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            if (Controller.GetLastScreen(out MenuUIScreen screen) && screen.GetType() == typeof(VMUI_MainMenu))
                (screen as VMUI_MainMenu)?.ActiveButton();
            base.Hide();

            HideUser();
        }

        public void SelectCategory(BookCategories category)
        {
            if (_CurrentSelectedCategory == category)
                return;
            if (_CurrentSelectedCategory != null)
                _CurrentSelectedCategory.SetActiveCategorie(false);
            _CurrentSelectedCategory = category;

            _CurrentSelectedCategory.SetActiveCategorie(true);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_BackButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            _Content?.SetActive(false);
            Controller.HideModal(this);
        }

        #endregion
    }
}
