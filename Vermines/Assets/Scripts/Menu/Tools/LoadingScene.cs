using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

namespace Vermines.Menu.Tools {

    public class LoadSceneStep : ILoadingSteps {

        private string _SceneName; // Name of the scene to load.

        private LoadSceneMode _Mode; // Scene loading mode (single or additive).

        private bool _SetToActive; // Flag to set the loaded scene as active.

        public LoadSceneStep(string sceneName, LoadSceneMode mode, bool setToActive = false)
        {
            _SceneName   = sceneName;
            _Mode        = mode;
            _SetToActive = setToActive;
        }

        public string StepName => $"Loading the scene...";

        public IEnumerator Execute()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(_SceneName, _Mode);

            while (!op.isDone)
                yield return null;
            // Get the loaded scene by name
            Scene loadedScene = SceneManager.GetSceneByName(_SceneName);

            if (loadedScene.IsValid() && loadedScene.isLoaded) {
                if (_SetToActive) {
                    SceneManager.SetActiveScene(loadedScene);

                    DynamicGI.UpdateEnvironment();
                }
            } else {
                Debug.LogError($"[LoadSceneStep]: Failed to load {_SceneName}.");
            }
        }
    }
}
