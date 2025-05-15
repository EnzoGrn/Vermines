using System.Threading.Tasks;
using UnityEngine;
using Fusion;

namespace OMGG.Menu.Screen {

    using Text   = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;

    /// <summary>
    /// The popup screen handles notificaction.Popup
    /// The screen has be <see cref="MenuUIScreen.IsModal"/> true.
    /// </summary>
    public partial class Popup : MenuUIScreen {

        #region Fields

        /// <summary>
        /// The text field for the message.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _Text;

        /// <summary>
        /// The text field for the header.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _Header;

        /// <summary>
        /// The okay button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _Button;

        #endregion

        /// <summary>
        /// The completion source will be triggered when the screen has been hidden.
        /// </summary>
        protected TaskCompletionSource<bool> _TaskCompletionSource;

        #region Override Methods

        /// <summary>
        /// The Unity awake method. Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();
        }

        #endregion

        #region Methods

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
            var completionSource = _TaskCompletionSource;

            _TaskCompletionSource = null;

            completionSource?.TrySetResult(true);
        }

        /// <summary>
        /// Open the screen in overlay mode
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="header">Header, can be null</param>
        public virtual void OpenPopup(string message, string header)
        {
            _Header.text = header;
            _Text.text   = message;

            Show();
        }

        /// <summary>
        /// Open the screen and wait for it being hidden
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="header">Header, can be null</param>
        /// <returns>When the screen is hidden.</returns>
        public virtual Task OpenPopupAsync(string message, string header)
        {
            _TaskCompletionSource?.TrySetResult(true);

            _TaskCompletionSource = new TaskCompletionSource<bool>();

            OpenPopup(message, header);

            return _TaskCompletionSource.Task;
        }

        #endregion
    }
}
