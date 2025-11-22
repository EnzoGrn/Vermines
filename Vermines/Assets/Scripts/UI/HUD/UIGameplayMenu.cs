using UnityEngine;

namespace Vermines.UI.View {

    using Vermines.Core;
    using Vermines.Extension;
    using Vermines.UI.Core;
    using Vermines.UI.Dialog;

    public class UIGameplayMenu : UICloseView {

        private const string OPEN_STATE  = "Open";
        private const string CLOSE_STATE = "Close";

        #region Attributes

        private bool _MenuVisible;

        [SerializeField]
        private UIButton _ResumeButton;

        [SerializeField]
        private UIButton _LeaveButton;

        [SerializeField]
        private UIButton _SettingsButton;

        #endregion

        #region Getters & Setters

        public override bool NeedsCursor => _MenuVisible;

        #endregion

        #region Methods

        public void Show(bool value, bool force = false)
        {
            if (_MenuVisible == value && !force)
                return;
            _MenuVisible = value;
            CanvasGroup.interactable = value;

            if (value)
                Animator.PlayForward(OPEN_STATE, true);
            else
                Animator.PlayForward(CLOSE_STATE, true);
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _ResumeButton.onClick.AddListener(OnResumeButton);
            _LeaveButton.onClick.AddListener(OnLeaveButton);
            _SettingsButton.onClick.AddListener(OnSettingsButton);
        }

        protected override void OnDeinitialize()
        {
            _ResumeButton.onClick.AddListener(OnResumeButton);
            _LeaveButton.onClick.AddListener(OnLeaveButton);
            _SettingsButton.onClick.AddListener(OnSettingsButton);

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            Animator.SampleStart(CLOSE_STATE);

            _MenuVisible = false;

            CanvasGroup.interactable = false;
        }

        protected override void OnCloseButton()
        {
            Show(false);
        }

        protected override bool OnBackAction()
        {
            if (_MenuVisible)
                return base.OnBackAction();
            Show(true);

            return true;
        }

        private void OnResumeButton()
        {
            OnCloseButton();
        }

        private void OnLeaveButton()
        {
            var dialog = Open<UIYesNoDialog>();

            dialog.Title.SetTextSafe("LEAVE MATCH");
            dialog.Description.SetTextSafe("Are you sure you want to leave current match?");

            dialog.HasClosed += (result) => {
                if (result)
                    OnLeave();
            };
        }

        public void OnLeave()
        {
            if (Context != null && Context.GameplayMode != null)
                Context.GameplayMode.StopGame();
            else
                Global.Networking.StopGame();
        }

        private void OnSettingsButton()
        {
            UISettingsView settings = Open<UISettingsView>();

            settings.HasClosed += () => {
                Show(false);
            };
        }

        #endregion
    }
}
