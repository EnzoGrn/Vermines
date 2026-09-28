using UnityEngine;

namespace Vermines.UI.Core {

    using Vermines.Core.UI;

    public class UICloseView : UIView {

        #region Attributes

        public UIView BackView { get; set; }

        public UIButton CloseButton => _CloseButton;

        [SerializeField]
        private UIButton _CloseButton;

        #endregion

        #region Methods

        public void CloseWithBack()
        {
            OnCloseButton();
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_CloseButton != null)
                _CloseButton.onClick.AddListener(OnCloseButton);
        }

        protected override void OnDeinitialize()
        {
            base.OnDeinitialize();

            if (_CloseButton)
                _CloseButton.onClick.RemoveListener(OnCloseButton);
        }

        protected override bool OnBackAction()
        {
            if (!IsInteractable)
                return false;
            OnCloseButton();

            if (_CloseButton != null)
                _CloseButton.PlayClickSound();
            return true;
        }

        protected virtual void OnCloseButton()
        {
            Close();

            if (BackView != null) {
                Open(BackView);

                BackView = null;
            }
        }

        #endregion
    }
}
