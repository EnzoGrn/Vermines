using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

namespace Vermines.Menu {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Core.Scene;

    public class Menu : Scene {

        #region Attributes

        [SerializeField]
        private string _SceneToLoad;

        #endregion

        #region Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            StartCoroutine(LoadScene(_SceneToLoad));
        }

        protected override void OnDeinitialize()
        {
            base.OnDeinitialize();

            if (_SceneToLoad != null) {
                UnityScene scene = SceneManager.GetSceneByName(_SceneToLoad);

                if (scene.IsValid())
                    SceneManager.UnloadSceneAsync(_SceneToLoad);
            }
        }

        private IEnumerator LoadScene(string scene)
        {
            UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

            yield return new WaitUntil(() => asyncLoad.isDone);
        }

        #endregion
    }
}
