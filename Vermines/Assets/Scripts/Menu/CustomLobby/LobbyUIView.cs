using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Core.UI;
    using Vermines.UI.Dialog;
    using Vermines.UI;
    using Vermines.Extension;
    using Vermines.Core;
    using Fusion;
    using static PlasticGui.PlasticTableColumn;

    public class LobbyUIView : UIView {

        #region Attributes

        [SerializeField]
        private PlayerCard[] _PlayerCards;

        [SerializeField]
        private UIButton _BookButton;

        [SerializeField]
        private UIButton _QuitButton;

        [SerializeField]
        private UIButton _SettingsButton;

        [SerializeField]
        private UIButton _CopySessionButton;

        [SerializeField]
        private TextMeshProUGUI _SessionCode;

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _BookButton.onClick.AddListener(OnBookButton);
            _SettingsButton.onClick.AddListener(OnSettingsButton);
            _QuitButton.onClick.AddListener(OnQuitButton);
            _CopySessionButton.onClick.AddListener(OnCopySessionButton);

            _SessionCode.SetTextSafe(Context.Runner.SessionInfo.Name);

            OnPlayerStatesChanged();
        }

        protected override void OnDeinitialize()
        {
            _BookButton.onClick.RemoveListener(OnBookButton);
            _SettingsButton.onClick.RemoveListener(OnSettingsButton);
            _QuitButton.onClick.RemoveListener(OnQuitButton);
            _CopySessionButton.onClick.RemoveListener(OnCopySessionButton);

            base.OnDeinitialize();
        }

        public void OnPlayerStatesChanged()
        {
            if (_PlayerCards == null || _PlayerCards.Length == 0 || Context == null || Context.Runner == null)
                return;
            List<LobbyPlayerController> players = Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            for (int i = 0; i < _PlayerCards.Length; i++) {
                if (i < players.Count && players[i] != null) {
                    CultistSelectState state = players[i].State;

                    if (state.ClientID != default)
                        _PlayerCards[i].UpdateDisplay(state, players[i].Nickname, state.ClientID == Context.Runner.LocalPlayer);
                    else
                        _PlayerCards[i].DisableDisplay();
                } else
                    _PlayerCards[i].DisableDisplay();
            }

            foreach (NetworkCultistSelectDisplay display in FindObjectsByType<NetworkCultistSelectDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                display.HandleStatesChanged();
        }

        private void OnBookButton()
        {
            Debug.Log("TODO: OnBookButton UIView, open the book of rules");
        }

        private void OnSettingsButton()
        {
            Open<UISettingsView>();
        }

        private void OnQuitButton()
        {
            var dialog = Open<UIYesNoDialog>();

            dialog.Title.SetTextSafe("LEAVE CUSTOM");
            dialog.Description.SetTextSafe("Are you sure you want to leave the lobby?");

            dialog.HasClosed += (result) => {
                if (result == true) {
                    if (Context != null && Context.Lobby != null)
                        Context.Lobby.StopGame();
                    else
                        Global.Networking.StopGame();
                }
            };
        }

        private void OnCopySessionButton()
        {
            GUIUtility.systemCopyBuffer = _SessionCode.GetTextSafe();
        }

        #endregion
    }
}
