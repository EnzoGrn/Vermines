using TMPro;

namespace Vermines.UI.Dialog {

    using Vermines.UI.Core;
    using Vermines.Extension;

    public class UIButtonDialogView : UIDialogView {

        #region Attributes

        public UIButton ConfirmButton;

        public TextMeshProUGUI ConfirmButtonText;

        private string _DefaultOkButtonText;

        #endregion

        #region Methods

        public override void Clear()
        {
            base.Clear();

            ConfirmButtonText.SetTextSafe(_DefaultOkButtonText);
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ConfirmButton.onClick.AddListener(OnConfirmButton);

            _DefaultOkButtonText = ConfirmButtonText.GetTextSafe();
        }

        protected override void OnDeinitialize()
        {
            ConfirmButton.onClick.RemoveListener(OnConfirmButton);

            base.OnDeinitialize();
        }

        private void OnConfirmButton()
        {
            Close();
        }

        #endregion
    }
}
