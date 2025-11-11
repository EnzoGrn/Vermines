using UnityEngine;

namespace Vermines.Menu.Matchmaking {

    using Vermines.Core.UI;
    using Vermines.UI;
    using Vermines.UI.Dialog;
    using Vermines.Extension;
    using Vermines.Core;

    public class MatchmakingView : UIView {

        #region Attributes

        [SerializeField]
        private UIButton _LeaveButton;

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _LeaveButton.onClick.AddListener(OnLeaveButton);
        }

        protected override void OnDeinitialize()
        {
            _LeaveButton.onClick.RemoveListener(OnLeaveButton);

            base.OnDeinitialize();
        }

        private void OnLeaveButton()
        {
            var dialog = Open<UIYesNoDialog>();

            dialog.Title.SetTextSafe("LEAVE CUSTOM");
            dialog.Description.SetTextSafe("Are you sure you want to leave the lobby?");

            dialog.HasClosed += (result) => {
                if (result == true) {
                    if (Context != null && Context.Lobby != null)
                        Context.NetworkMatchmaking.LeaveGame();
                    else
                        Global.Networking.StopGame();
                }
            };
        }

        #endregion
    }
}
