using OMGG.Menu.Connection;
using OMGG.Menu.Screen;
using UnityEngine;

namespace Vermines.Menu.Screen {

    using Text   = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;

    /// <summary>
    /// Vermines Loading Menu UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_Loading : MenuUIScreen {

        #region Fields

        /// <summary>
        /// The cancel button.
        /// </summary>
        [SerializeField]
        protected Button _CancelButton;

        /// <summary>
        /// The loading screen status text.
        /// </summary>
        [SerializeField]
        protected Text _Text;

        #endregion

        #region Partial Methods

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #endregion

        #region Overrides Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update the text of the loading screen.
        /// </summary>
        /// <param name="text">Text</param>
        public void SetStatusText(string text)
        {
            if (_Text != null)
                _Text.text = text;
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CancelButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual async void OnDisconnectPressed()
        {
            await Connection.DisconnectAsync(ConnectFailReason.UserRequest);
        }

        #endregion
    }
}
