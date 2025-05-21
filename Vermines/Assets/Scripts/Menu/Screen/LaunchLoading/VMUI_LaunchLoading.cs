using OMGG.Menu.Screen;
using Vermines.Menu.Tools;

namespace Vermines.Menu.Screen {

    public partial class VMUI_LaunchLoading : MenuUIScreen {

        #region Fields

        public LoadingManager LoadingManager;

        #endregion

        #region Overrides Methods

        public override void Show()
        {
            base.Show();

            LoadingManager.AddStep(new LoadSceneStep("EnvironmentDay", UnityEngine.SceneManagement.LoadSceneMode.Additive));

            LoadingManager.OnLoadingDone += OnLoadingDone;

            LoadingManager.StartLoading();
        }

        #endregion

        #region Events

        public void OnLoadingDone()
        {
            LoadingManager.OnLoadingDone -= OnLoadingDone;

            Controller.Show<VMUI_MainMenu>(this);
        }

        #endregion
    }
}
