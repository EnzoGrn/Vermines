using System.Collections.Generic;
using OMGG.Menu.Screen;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {

    using Vermines.Menu.Tools;

    public partial class VMUI_LaunchLoading : MenuUIScreen {

        #region Scene to load

        [InlineHelp, SerializeField]
        private List<string> ScenesToLoad;

        #endregion

        #region Fields

        [InlineHelp, SerializeField]
        [Tooltip("The camera for the loading screen.")]
        private Camera _Camera;

        public LoadingManager LoadingManager;

        #endregion

        #region Overrides Methods

        public override void Init()
        {
            base.Init();
        }

        public override void Show()
        {
            if (_Camera)
                _Camera.gameObject.SetActive(true);
            base.Show();

            for (int i = 0; i < ScenesToLoad.Count; i++)
                LoadingManager.AddStep(new LoadSceneStep(ScenesToLoad[i], UnityEngine.SceneManagement.LoadSceneMode.Additive));
            LoadingManager.OnLoadingDone += OnLoadingDone;

            LoadingManager.StartLoading();
        }

        public override void Hide()
        {
            base.Hide();

            LoadingManager.ClearSteps();
            LoadingManager.OnLoadingDone -= OnLoadingDone;

            if (_Camera)
                _Camera.gameObject.SetActive(false);
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
