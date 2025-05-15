using OMGG.Menu.Connection;
using OMGG.Menu.Screen;
using System.Collections;
using System.Text;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {

    using Text   = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;

    /// <summary>
    /// Vermines Gameplay Menu UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_Gameplay : MenuUIScreen {

        #region Fields

        /// <summary>
        /// The session code label.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _CodeText;

        /// <summary>
        /// The list of players.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _PlayersText;

        /// <summary>
        /// The current player count.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _PlayersCountText;

        /// <summary>
        /// The max player count.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _PlayersMaxCountText;
        
        /// <summary>
        /// The menu header text.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _HeaderText;
        
        /// <summary>
        /// The GameObject of the session part to be toggled off.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _SessionGameObject;
        
        /// <summary>
        /// The GameObject of the player part to be toggled off.
        /// </summary>
        [InlineHelp, SerializeField]
        protected GameObject _PlayersGameObject;

        /// <summary>
        /// The copy session button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _CopySessionButton;

        /// <summary>
        /// The disconnect button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _DisconnectButton;

        /// <summary>
        /// In what frequencey are the usernames refreshed.
        /// </summary>
        [InlineHelp]
        public float UpdateUsernameRateInSeconds = 2;

        /// <summary>
        /// The coroutine is started during Show() and updates the Usernames using this interval <see cref="UpdateUsernameRateInSeconds"/>.
        /// </summary>
        protected Coroutine _UpdateUsernamesCoroutine;

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
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();

            // Only show the session UI if it is a party code
            if (Config.CodeGenerator != null && Config.CodeGenerator.IsValid(Connection.SessionName)) {
                _CodeText.text = Connection.SessionName;

                _SessionGameObject.SetActive(true);
            } else {
                _CodeText.text = string.Empty;

                _SessionGameObject.SetActive(false);
            }

            UpdateUsernames();

            if (UpdateUsernameRateInSeconds > 0)
                _UpdateUsernamesCoroutine = StartCoroutine(UpdateUsernamesCoroutine());
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            if (_UpdateUsernamesCoroutine != null) {
                StopCoroutine(_UpdateUsernamesCoroutine);

                _UpdateUsernamesCoroutine = null;
            }
        }

        /// <summary>
        /// Update the usernames list. Will cancel itself if UpdateUsernameRateInSeconds <= 0.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator UpdateUsernamesCoroutine()
        {
            while (UpdateUsernameRateInSeconds > 0) {
                yield return new WaitForSeconds(UpdateUsernameRateInSeconds);

                UpdateUsernames();
            }
        }

        /// <summary>
        /// Update the usernames and toggle the UI part on/off depending on the <see cref="IFusionMenuConnection.Usernames"/>
        /// </summary>
        protected virtual void UpdateUsernames()
        {
            if (Connection.Usernames != null && Connection.Usernames.Count > 0) {
                StringBuilder sBuilder    = new();
                int           playerCount = 0;

                _PlayersGameObject.SetActive(true);

                foreach (var username in Connection.Usernames) {
                    sBuilder.AppendLine(username);
                    playerCount += string.IsNullOrEmpty(username) ? 0 : 1;
                }

                _PlayersText.text = sBuilder.ToString();
                _PlayersCountText.text = $"{playerCount}";
                _PlayersMaxCountText.text = $"/{Connection.MaxPlayerCount}";
            } else
                _PlayersGameObject.SetActive(false);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_disconnectButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual async void OnDisconnectPressed()
        {
            await Connection.DisconnectAsync(ConnectFailReason.UserRequest);

            Controller.Show<VMUI_MainMenu>();
        }

        /// <summary>
        /// Is called when the <see cref="_copySessionButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnCopySessionPressed()
        {
            GUIUtility.systemCopyBuffer = _CodeText.text;
        }

        #endregion
    }
}
