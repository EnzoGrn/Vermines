using UnityEngine;
using System;
using TMPro;

#pragma warning disable CS0414

namespace Vermines.UI.Dialog {

    using Vermines.Extension;

    public class UIYesNoDialog : UIDialogView {

        #region Attributes

        public bool Result { get; private set; }

        public new event Action<bool> HasClosed;

        public UIButton YesButton;
        public TextMeshProUGUI YesButtonText;

        [SerializeField]
        private string _DefaultYesButtonText = "confirm";

        public UIButton NoButton;
        public TextMeshProUGUI NoButtonText;

        [SerializeField]
        private string _DefaultNoButtonText = "cancel";

        #endregion

        #region Methods

        public override void Clear()
        {
            base.Clear();

            YesButtonText.SetTextSafe(_DefaultYesButtonText);
            NoButtonText.SetTextSafe(_DefaultYesButtonText);
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            YesButton.onClick.AddListener(OnYesButton);
            NoButton.onClick.AddListener(OnNoButton);
        }

        protected override void OnDeinitialize()
        {
            YesButton.onClick.RemoveListener(OnYesButton);
            NoButton.onClick.RemoveListener(OnNoButton);


            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            Result = false;
        }

        protected override void OnClose()
        {
            base.OnClose();

            if (HasClosed != null) {
                HasClosed.Invoke(Result);

                HasClosed = null;
            }
        }

        private void OnYesButton()
        {
            Result = true;

            Close();
        }

        private void OnNoButton()
        {
            Result = false;

            Close();
        }

        #endregion
    }
}
