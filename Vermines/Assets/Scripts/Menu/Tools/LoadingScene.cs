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
            AsyncOperation op = SceneManager.LoadSceneAsync(_SceneName, _Mode);

            while (!op.isDone)
                yield return null;
        }
    }
}
