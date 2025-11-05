using UnityEngine;
using Fusion;
using TMPro;

namespace Vermines.Menu.View {

    using Vermines.Core.Network;
    using Vermines.UI;
    using Vermines.UI.Core;
    using Vermines.Core;
    using Vermines.UI.Dialog;
    using Vermines.Extension;
    using Vermines.Core.Settings;

    public class UIPartyMenuView : UICloseView {

        #region Attributes

        [SerializeField]
        private UIButton _CreateButton;

        [SerializeField]
        private UIButton _JoinButton;

        [SerializeField]
        private TMP_InputField _SessionCodeField;

        #endregion

        #region Methods

        private void OpenDialog(string title, string description)
        {
            var dialog = Open<UIYesNoDialog>();

            dialog.Title.SetTextSafe(title);
            dialog.Description.SetTextSafe(description);
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _CreateButton.onClick.AddListener(OnCreateButton);
            _JoinButton.onClick.AddListener(OnJoinButton);
        }

        protected override void OnDeinitialize()
        {
            _CreateButton.onClick.RemoveListener(OnCreateButton);
            _JoinButton.onClick.RemoveListener(OnJoinButton);

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }

        private void OnCreateButton()
        {
            SessionRequest session = new() {
                GameMode     = GameMode.Host,
                GameplayType = GameplayType.Standart,
                MaxPlayers   = 4,
                ScenePath    = Context.CustomGameScenePath
            };

            Context.Matchmaking.CreateSession(session, isCustom: true);
        }

        private void OnJoinButton()
        {
            NetworkSettings settings = Context.Settings.Network;

            string lobbyCode = _SessionCodeField.text.ToUpper();

            if (!settings.CodeGenerator.IsValid(lobbyCode)) {
                OpenDialog("INVALID SESSION CODE", $"The session code '{lobbyCode}' is not a valid session code. Please enter {settings.CodeGenerator.Length} characters or digits.");

                return;
            }

            int regionIndex = settings.CodeGenerator.DecodeRegion(lobbyCode);

            if (regionIndex < 0 || regionIndex >= settings.Regions.Length) {
                OpenDialog("INVALID SESSION CODE", $"The session code '{lobbyCode}' is not a valid session code. Please enter {settings.CodeGenerator.Length} characters or digits.");

                return;
            }

            Context.Matchmaking.JoinSession(lobbyCode, isCustom: true);
        }

        #endregion
    }
}
