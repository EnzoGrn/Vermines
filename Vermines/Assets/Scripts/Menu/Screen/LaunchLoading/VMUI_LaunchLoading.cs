using System.Collections.Generic;
using OMGG.Menu.Screen;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {
    using System.Collections;
    using Vermines.Menu.Tools;
    using Vermines.Sound;

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

        public override IEnumerator ShowCoroutine()
        {
            if (_Camera != null)
                _Camera.gameObject.SetActive(true);
            yield return base.ShowCoroutine();
        }

        public override void Show()
        {
            base.Show();

            // Set has active only the first scene,
            // here: The Vermines main maps, to have the skybox and light system on.
            for (int i = 0; i < ScenesToLoad.Count; i++)
                LoadingManager.AddStep(new LoadSceneStep(ScenesToLoad[i], UnityEngine.SceneManagement.LoadSceneMode.Additive, i == 0));
            LoadingManager.OnLoadingDone += OnLoadingDone;

            LoadingManager.StartLoading();
        }

        public override void Hide()
        {
            LoadingManager.ClearSteps();
            LoadingManager.OnLoadingDone -= OnLoadingDone;

            if (_Camera != null)
                _Camera.gameObject.SetActive(false);
            base.Hide();
        }

        #endregion

        #region Events

        public void OnLoadingDone()
        {
            LoadingManager.OnLoadingDone -= OnLoadingDone;

            MusicManager musicManager = FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);

            if (musicManager)
                musicManager.Play();
            Controller.Show<VMUI_MainMenu>(this);
        }

        #endregion
    }
}
