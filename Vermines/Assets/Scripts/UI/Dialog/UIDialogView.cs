using TMPro;

namespace Vermines.UI.Dialog {

    using Vermines.UI.Core;
    using Vermines.Extension;

    public class UIDialogView : UICloseView {

        #region Attributes

        public TextMeshProUGUI Title;
        public TextMeshProUGUI Description;

        private string _DefaultTitleText;
        private string _DefaultDescriptionText;

        #endregion

        #region Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _DefaultTitleText       = Title.GetTextSafe();
            _DefaultDescriptionText = Title.GetTextSafe();
        }

        protected override void OnClose()
        {
            base.OnClose();

            Clear();
        }

        public virtual void Clear()
        {
            Title.SetTextSafe(_DefaultTitleText);
            Title.SetTextSafe(_DefaultDescriptionText);
        }

        #endregion
    }
}
