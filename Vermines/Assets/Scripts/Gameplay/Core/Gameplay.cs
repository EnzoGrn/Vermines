using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

namespace Vermines.Gameplay.Core {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Core.Scene;
    using Vermines.Extension;
    using Vermines.Core;
    using Vermines.Core.Services;
    using Vermines.Core.UI;

    public class Gameplay : Scene {

        [SerializeField]
        private string UI_SCENE_NAME = "UI";

        #region Attributes

        [SerializeField]
        private string _SceneToLoad;

        private UnityScene _UIScene;

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            StartCoroutine(PersistentSceneService.Instance.LoadSceneAdditive(_SceneToLoad));

            var contextBehaviours = Context.Runner.SimulationUnityScene.GetComponents<IContextBehaviour>(true);

            foreach (IContextBehaviour behaviour in contextBehaviours)
                behaviour.Context = Context;
        }

        protected override IEnumerator OnActivate()
        {
            yield return base.OnActivate();
            yield return PersistentSceneService.Instance.LoadSceneAdditive(UI_SCENE_NAME);

            _UIScene = SceneManager.GetSceneByName(UI_SCENE_NAME);

            if (_UIScene != null) {
                SceneUI uiService = _UIScene.GetComponent<SceneUI>(true);

                Context.UI = uiService;

                AddService(uiService);

                uiService.Activate();
            }
        }

        #endregion
    }
}
