using Fusion;
using OMGG.Menu.Screen;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Vermines.Menu.Tools;

namespace Vermines.Menu.Screen {

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
