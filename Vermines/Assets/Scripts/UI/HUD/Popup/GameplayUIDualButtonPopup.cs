using Fusion;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI.Popup
{
    using Text = TMPro.TMP_Text;

    public partial class GameplayUIDualButtonPopup : GameplayUIScreen
    {
        #region Fields

        /// <summary>
        /// The text field for the message.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _Text;

        [InlineHelp, SerializeField]
        protected Button _ButtonLeft;

        [InlineHelp, SerializeField]
        protected Button _ButtonRight;

        [InlineHelp, SerializeField]
        protected Text _ButtonLeftText;

        [InlineHelp, SerializeField]
        protected Text _ButtonRightText;

        #endregion

        /// <summary>
        /// The completion source will be triggered when the screen has been hidden.
        /// </summary>
        protected TaskCompletionSource<bool> _buttonCompletionSource;

        #region Override Methods

        public override void Awake()
        {
            base.Awake();

            _ButtonLeft.onClick.AddListener(() => OnButtonClicked(false));
            _ButtonRight.onClick.AddListener(() => OnButtonClicked(true));
        }

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        /// <summary>
        /// The screen init method. Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();
        }

        /// <summary>
        /// The screen show method. Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();
        }

        /// <summary>
        /// The screen hide method. Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            // Free the _TaskCompletionSource before releasing the old one.
            var completionSource = _buttonCompletionSource;

            _buttonCompletionSource = null;

            completionSource?.TrySetResult(true);
        }

        #endregion

        #region Methods

        private void OnButtonClicked(bool rightButton)
        {
            _buttonCompletionSource?.TrySetResult(rightButton);
            Hide();
        }

        /// <summary>
        /// Opens the popup with two buttons and waits for user input.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="header">Header</param>
        /// <param name="leftButtonLabel">Label for the left button</param>
        /// <param name="rightButtonLabel">Label for the right button</param>
        /// <returns>True if right button was clicked, False if left, null if cancelled.</returns>
        public Task<bool> OpenDualButtonPopupAsync(string message, string header, string leftButtonLabel, string rightButtonLabel)
        {
            _buttonCompletionSource = new TaskCompletionSource<bool>();

            _Text.text = message;
            _ButtonLeftText.text = leftButtonLabel;
            _ButtonRightText.text = rightButtonLabel;

            Show();

            return _buttonCompletionSource.Task;
        }

        #endregion
    }
}
