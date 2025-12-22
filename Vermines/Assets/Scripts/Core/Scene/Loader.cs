using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Core.Scene {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Core.UI;
    using Vermines.Extension;
    using Vermines.Core.Services;

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

        #endregion

        #region MonoBehaviour Methods

        protected IEnumerator Start()
        {
            _FadeInObject.SetActive(true);
            _FadeOutObject.SetActive(false);

            UnityScene loaderScene = SceneManager.GetActiveScene();

            PersistentSceneService.Instance.RegisterPersistentScene(_MainScene);

            if (_SceneToLoads != null && _SceneToLoads.Length > 0) {
                for (int i = 0; i < _SceneToLoads.Length; i++)
                    StartCoroutine(PersistentSceneService.Instance.LoadSceneAdditive(_SceneToLoads[i]));
            }

            yield return PersistentSceneService.Instance.InitializePersistentScenes();

            PersistentSceneService.Instance.SwitchToScene(_MainScene);

            _FadeInObject.SetActive(false);
            _FadeOutObject.SetActive(true);

            yield return new WaitUntil(() => _FadeOutObject.IsFinished);

            SceneManager.UnloadSceneAsync(loaderScene);
        }

        #endregion
    }
}
