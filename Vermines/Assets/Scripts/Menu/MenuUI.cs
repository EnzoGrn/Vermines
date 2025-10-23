using WebSocketSharp;

namespace Vermines.Menu {

    using Vermines.Core.UI;
    using Vermines.Core;

    public class MenuUI : SceneUI {

        protected override void OnActivate()
        {
            base.OnActivate();

            if (!Global.Networking.ErrorStatus.IsNullOrEmpty()) {
                // TODO: Force go to the tavern view.
                // TODO: Open an error dialog.

                Global.Networking.ClearErrorStatus();
            }
        }

    }
}
