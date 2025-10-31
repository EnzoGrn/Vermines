using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using Fusion;

namespace Vermines.Core.Network {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Core.Scene;
    using Vermines.Extension;
    using Vermines.Core.Services;

    public class NetworkSceneManager : NetworkSceneManagerDefault {

        public Scene GameplayScene => _GameplayScene;
        private Scene _GameplayScene;

        public override bool IsBusy => _IsBusy | base.IsBusy;
        private bool _IsBusy;

        protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, UnityScene scene, NetworkLoadSceneParameters sceneParams)
        {
            _IsBusy        = true;
            _GameplayScene = scene.GetComponent<Scene>(true);

            float contextTimeout = 20.0f;

            while (!_GameplayScene.ContextReady && contextTimeout > 0.0f) {
                yield return null;

                contextTimeout -= Time.unscaledDeltaTime;
            }
            var contextBehaviours = scene.GetComponents<IContextBehaviour>(true);

            foreach (var behaviour in contextBehaviours)
                behaviour.Context = _GameplayScene.Context;
            yield return base.OnSceneLoaded(sceneRef, scene, sceneParams);

            PersistentSceneService.Instance.SwitchToScene(PersistentSceneService.Instance.PersistentScenes[0]);
            PersistentSceneService.Instance.ApplyPersistentSceneRenderSettings();

            _IsBusy = false;
        }
    }
}
