using System;
using UnityEngine;

namespace Vermines.UI.View {

    using Vermines.Core;
    using Vermines.Core.UI;
    using Vermines.Extension;
    using Vermines.UI.Core;
    using Vermines.UI.Dialog;

    public class UIGameplayMenu : UICloseView {

        #region Static state (read by HUD widgets)

        /// <summary>
        /// Raised when the pause flow (pause, settings, dialogs) starts or ends.
        /// </summary>
        public static event Action<bool> EngagedChanged;

        public static bool IsEngaged { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsEngaged      = false;
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

        // Settings or leave dialog opened from the pause, null otherwise.
        private UIView _SubView;

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

        // True for the whole pause flow, so the tutorial also stays hidden while the settings are open.
        public override bool NeedsCursor => IsEngaged;

        #endregion

        #region Methods

        /// <summary>
        /// Enters the pause flow. Called by the HUD book button and by the back action.
        /// Hides UIView.Open() on purpose: the view itself is always open (default view), only the scroll shows up.
        /// </summary>
        public new void Open()
        {
            if (_MenuVisible)
                return;
            SetEngaged(true);
            Show(true);
        }

        /// <summary>
        /// Leaves the pause flow and gives the HUD back.
        /// </summary>
        public void Resume()
        {
            Show(false);
            SetEngaged(false);
        }

        /// <param name="aside">True when a sub view opens on top: the scroll leaves but the dim and the HUD tuck stay.</param>
        public void Show(bool value, bool force = false, bool aside = false)
        {
            if (_MenuVisible == value && !force)
                return;
            _MenuVisible = value;

            CanvasGroup.interactable = value;

            if (_Anim == null)
                return;
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

            if (_Anim == null && !TryGetComponent(out _Anim))
                Debug.LogError("[UIGameplayMenu] No PausePanelAnimator assigned.", this);

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

            if (_Anim != null)
                _Anim.Snap(false);
            _MenuVisible = false;
            _SubView     = null;

            CanvasGroup.interactable = false;
        }

        protected override void OnClose()
        {
            // The view itself is closed (scene UI deactivated): the pause flow ends with it.
            _MenuVisible = false;
            _SubView     = null;

            SetEngaged(false);

            base.OnClose();
        }

        protected override void OnCloseButton()
        {
            Resume();
        }

        protected override bool OnBackAction()
        {
            // A sub view that did not handle the back action itself (or is busy closing).
            if (_SubView != null && _SubView.IsOpen) {
                if (_SubView.IsInteractable && _SubView is UICloseView closeView)
                    closeView.CloseWithBack();
                return true;
            }

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
            UIYesNoDialog dialog = Open<UIYesNoDialog>();

            if (dialog == null)
                return;
            _SubView = dialog;

            dialog.Title.SetTextSafe("LEAVE MATCH");
            dialog.Description.SetTextSafe("Are you sure you want to leave current match?");

            dialog.HasClosed += (result) => {
                _SubView = null;

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
            UIView settings = Open<UISettingsScrollView>();

            if (settings == null) {
                Debug.LogWarning("[UIGameplayMenu] UISettingsScrollView not found, falling back to UISettingsView.", this);

                settings = Open<UISettingsView>();
            }

            if (settings == null)
                return;
            _SubView = settings;

            Show(false, aside: true); // scroll leaves, dim and HUD tuck stay

            void Back()
            {
                settings.HasClosed -= Back;

                _SubView = null;

                if (IsEngaged)
                    Show(true); // back to the pause scroll, not to the game
            }

            settings.HasClosed += Back;
        }

        #endregion
    }
}
