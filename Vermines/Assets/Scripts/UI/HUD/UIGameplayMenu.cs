using System;
using UnityEngine;

namespace Vermines.UI.View
{

    using Vermines.Core;
    using Vermines.Extension;
    using Vermines.UI.Core;
    using Vermines.UI.Dialog;

    public class UIGameplayMenu : UICloseView
    {

        #region Static state (read by HUD widgets)

        /// <summary>Raised when the pause flow (pause, settings, dialogs) starts or ends.</summary>
        public static event Action<bool> EngagedChanged;

        public static bool IsEngaged { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsEngaged = false;
            EngagedChanged = null;
        }

        private static void SetEngaged(bool value)
        {
            if (IsEngaged == value)
                return;
            IsEngaged = value;
            EngagedChanged?.Invoke(value);
        }

        #endregion

        #region Attributes

        private bool _MenuVisible;

        [SerializeField]
        private PausePanelAnimator _Anim;

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

        /// <summary>Enters the pause flow. Called by the HUD book button and by the back action.</summary>
        public void Open()
        {
            if (_MenuVisible)
                return;
            SetEngaged(true);
            Show(true);
        }

        /// <summary>Leaves the pause flow and gives the HUD back.</summary>
        public void Resume()
        {
            Show(false);
            SetEngaged(false);
        }

        // aside = true when settings open on top: the scroll leaves but the dim and the HUD tuck stay.
        public void Show(bool value, bool force = false, bool aside = false)
        {
            if (_MenuVisible == value && !force)
                return;
            _MenuVisible = value;
            CanvasGroup.interactable = value;

            if (value)
                _Anim.Show();
            else
                _Anim.Hide(aside);
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
            _ResumeButton.onClick.RemoveListener(OnResumeButton);
            _LeaveButton.onClick.RemoveListener(OnLeaveButton);
            _SettingsButton.onClick.RemoveListener(OnSettingsButton);

            SetEngaged(false); // the static state must not outlive this scene

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            _Anim.Snap(false);
            _MenuVisible = false;
            CanvasGroup.interactable = false;
        }

        protected override void OnCloseButton()
        {
            Resume();
        }

        protected override bool OnBackAction()
        {
            if (_MenuVisible)
                Resume();
            else
                Open();

            return true;
        }

        private void OnResumeButton()
        {
            Resume();
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
            Show(false, aside: true); // scroll leaves, HUD stays tucked

            UISettingsView settings = Open<UISettingsView>();

            void Back()
            {
                settings.HasClosed -= Back;
                Show(true); // back to the pause scroll, not to the game
            }

            settings.HasClosed += Back;
        }

        #endregion
    }
}
