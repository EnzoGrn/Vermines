using OMGG.Menu.Connection.Data;
using OMGG.Menu.Connection;
using OMGG.Menu.Screen;
using System.Threading.Tasks;

namespace Vermines.Menu.Controller {

    using Vermines.Menu.Connection.Data;
    using Vermines.Menu.Screen;

    using MenuUIController = OMGG.Menu.Controller.MenuUIController;

    public partial class VMUI_Controller : MenuUIController {

        protected override ConnectionArgs CreateConnectArgs()
        {
            IConnectionDataProvider provider = new PlayerPrefsConnectionDataProvider();

            return new ConnectionArgs(provider);
        }

        /// <summary>
        /// Default connection error handling is reused in a couple places.
        /// </summary>
        /// <param name="result">Connect result</param>
        /// <param name="controller">UI Controller</param>
        /// <returns>When handling is completed</returns>
        public override async Task HandleConnectionResult(ConnectResult result, MenuUIController controller)
        {
            if (result.CustomResultHandling)
                return;
            if (result.Success)
                controller.Show(_ScreenToShowOnConnect);
            else if (result.FailReason != ConnectFailReason.ApplicationQuit) {
                var popup = controller.PopupAsync(result.DebugMessage, "Connection Failed");

                if (result.WaitForCleanup != null)
                    await Task.WhenAll(result.WaitForCleanup, popup);
                else
                    await popup;
                controller.Show<VMUI_Tavern>();
            }
        }

        public override async Task HandleSceneChangeResult(ConnectResult result, MenuUIController controller, MenuUIScreen screenToShow)
        {
            if (result.CustomResultHandling)
                return;
            if (result.Success)
                controller.Show(screenToShow);
            else {
                var popup = controller.PopupAsync(result.DebugMessage, "Problem during game launch.");

                if (result.WaitForCleanup != null)
                    await Task.WhenAll(result.WaitForCleanup, popup);
                else
                    await popup;
                controller.Show<VMUI_MainMenu>();
            }
        }
    }
}
