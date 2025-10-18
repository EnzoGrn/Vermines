using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Core.Scene {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Core.UI;
    using Vermines.Extension;

    public class Loader : MonoBehaviour {

        #region Attributes

        [SerializeField]
        private string _MainScene = "Menu";

        [SerializeField]
        private string[] _SceneToLoads;

        [SerializeField]
        private UIFader _FadeInObject;

        [SerializeField]
        private UIFader _FadeOutObject;

        private bool _Initialized = false;

        #endregion

        #region MonoBehaviour Methods

        protected IEnumerator Start()
        {
            _FadeInObject.SetActive(true);
            _FadeOutObject.SetActive(false);

            UnityScene loaderScene = SceneManager.GetActiveScene();

            if (_SceneToLoads != null && _SceneToLoads.Length > 0) {
                for (int i = 0; i < _SceneToLoads.Length; i++)
                    yield return LoadSceneAsync(_SceneToLoads[i]);
            }

            yield return InitializeMainScene();

            _FadeInObject.SetActive(false);
            _FadeOutObject.SetActive(true);

            yield return new WaitUntil(() => _FadeOutObject.IsFinished);

            SceneManager.UnloadSceneAsync(loaderScene);
        }

        #endregion

        #region Coroutine

        private IEnumerator InitializeMainScene()
        {
            if (!_Initialized) {
                Debug.Log("[Loader] Initializing main scene...");

                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_MainScene, LoadSceneMode.Additive);

                yield return new WaitUntil(() => asyncLoad.isDone);

                UnityScene mainScene = SceneManager.GetSceneByName(_MainScene);

                if (mainScene.IsValid())
                    SceneManager.SetActiveScene(mainScene);
                _Initialized = true;
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            Debug.Log($"[Loader] Loading {sceneName} scene...");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            yield return new WaitUntil(() => asyncLoad.isDone);
        }

        #endregion
    }
}
