using UnityEngine.UI;
using UnityEngine;
using TMPro;
using OMGG.Menu.Screen;
using Vermines.Menu.Screen;

namespace Vermines.UI {

    public class BookCategories : MenuScreenPlugin {

        #region Fields

        [Header("Button")]

        [SerializeField]
        private Button _Button;

        [Header("Properties")]

        [SerializeField]
        private string _CategoryName;

        public string CategoryName => _CategoryName;

        [SerializeField]
        private TMP_Text _Label;

        [SerializeField]
        private GameObject _ButtonBorder;

        [SerializeField]
        private GameObject _Cursor;

        [SerializeField]
        private GameObject _Toggle;

        [Header("Linked Page")]

        [SerializeField]
        private MenuScreenPlugin _LinkedPage;

        public MenuScreenPlugin LinkedPage => _LinkedPage;

        #endregion

        private MenuUIScreen _SettingsScreen;

        #region Override Methods

        public override void Init(MenuUIScreen screen)
        {
            base.Init(screen);

            _SettingsScreen = screen;

            if (_Label != null)
                _Label.text = _CategoryName;
            if (_Button != null)
                _Button.onClick.AddListener(OnCategoryClicked);
            if (LinkedPage != null)
                LinkedPage.Init(screen);
        }

        #endregion

        #region Methods

        public void SetActiveCategorie(bool isActive)
        {
            _ButtonBorder?.SetActive(isActive);
            _Cursor?.SetActive(isActive);
            _Toggle?.SetActive(isActive);
            _LinkedPage?.gameObject.SetActive(isActive);

            if (isActive)
                _LinkedPage?.Show(_SettingsScreen);
        }

        #endregion

        #region Events

        private void OnCategoryClicked()
        {
            (_SettingsScreen as VMUI_Settings)?.SelectCategory(this);
        }

        #endregion
    }
}
