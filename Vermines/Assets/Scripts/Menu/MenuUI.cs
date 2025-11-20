using WebSocketSharp;

namespace Vermines.Menu {

    using Vermines.Core.UI;
    using Vermines.Core;
    using Vermines.UI.Dialog;
    using Vermines.Extension;
    using Vermines.Core.Network;

    public class MenuUI : SceneUI {

        protected override void OnActivate()
        {
            base.OnActivate();

            if (!Global.Networking.ErrorStatus.IsNullOrEmpty()) {
                var dialog = Open<UIErrorDialogView>();

                dialog.Title.SetTextSafe("Connection Issue");

                if (Global.Networking.ErrorStatus == Networking.STATUS_SERVER_CLOSED)
                    dialog.Description.SetTextSafe($"Server was closed.");
                else
                    dialog.Description.SetTextSafe($"Failed to start network game\n\nReason:\n{Global.Networking.ErrorStatus}");
                Global.Networking.ClearErrorStatus();
            }
        }
    }
}
