using UnityEngine;
using TMPro;

namespace Vermines.UI {

    using Vermines.Extension;
    using Vermines.Core.UI;

    public class BookCategories : UIView {

        #region Fields

        [Header("Button")]

        [SerializeField]
        private UIButton _Button;

        [Header("Properties")]

        [SerializeField]
        private string _CategoryName;

        public string CategoryName => _CategoryName;

        [SerializeField]
        private TextMeshProUGUI _Label;

        [SerializeField]
        private GameObject _ButtonBorder;

        [SerializeField]
        private GameObject _Cursor;

        [SerializeField]
        private GameObject _Toggle;

        [Header("Linked Page")]

        [SerializeField]
        private MonoBehaviour _LinkedPage;

        public MonoBehaviour LinkedPage => _LinkedPage;

        #endregion

        #region Override Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _Label.SetTextSafe(_CategoryName);

            if (_Button != null)
                _Button.onClick.AddListener(OnCategoryClicked);
        }

        protected override void OnDeinitialize()
        {
            if (_Button != null)
                _Button.onClick.RemoveListener(OnCategoryClicked);
            base.OnDeinitialize();
        }

        #endregion

        #region Methods

        public void SetActiveCategorie(bool isActive)
        {
            _ButtonBorder?.SetActive(isActive);
            _Cursor?.SetActive(isActive);
            _Toggle?.SetActive(isActive);
            _LinkedPage?.gameObject.SetActive(isActive);
        }

        #endregion

        #region Events

        private void OnCategoryClicked()
        {
            
            UISettingsView settings = SceneUI.Get<UISettingsView>();

            settings?.SelectCategory(this);
        }

        #endregion
    }
}
