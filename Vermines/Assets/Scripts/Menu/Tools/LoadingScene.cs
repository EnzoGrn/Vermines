using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

namespace Vermines.Menu.Tools {

    public class LoadSceneStep : ILoadingSteps {

        private string _SceneName; // Name of the scene to load.

        private LoadSceneMode _Mode; // Scene loading mode (single or additive).

        public LoadSceneStep(string sceneName, LoadSceneMode mode)
        {
            _SceneName = sceneName;
            _Mode      = mode;
        }

        public string StepName => $"Loading the scene...";

        public IEnumerator Execute()
        {
            Debug.Log($"[LoadSceneStep]: Loading scene {_SceneName} in mode {_Mode}");
            AsyncOperation op = SceneManager.LoadSceneAsync(_SceneName, _Mode);

            while (!op.isDone)
                yield return null;

            // Get the loaded scene by name
            Scene loadedScene = SceneManager.GetSceneByName(_SceneName);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
                DynamicGI.UpdateEnvironment();
                Debug.Log($"[LoadSceneStep]: Scene {_SceneName} is now active.");
            }
            else
            {
                Debug.LogError($"[LoadSceneStep]: Failed to load or set active scene {_SceneName}.");
            }
        }
    }
}
