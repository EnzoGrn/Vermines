using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Core.Services {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    public class PersistentSceneService : MonoBehaviour {

        #region Singleton

        public static PersistentSceneService Instance => _Instance;

        private static PersistentSceneService _Instance;

        #endregion

        #region Attributes

        public readonly List<string> PersistentScenes = new();
        private readonly HashSet<string> _LoadedScenes = new();

        #endregion

        #region Getters & Setters

        public bool IsSceneLoaded(string sceneName)
        {
            return _LoadedScenes.Contains(sceneName);
        }

        public bool IsScenePersistent(string sceneName)
        {
            return PersistentScenes.Contains(sceneName);
        }

        public UnityScene GetFirstPersistentScene()
        {
            if (PersistentScenes.Count == 0)
                return default;
            return SceneManager.GetSceneByName(PersistentScenes[0]);
        }

        #endregion

        #region Methods

        private void Awake()
        {
            if (_Instance != null) {
                Destroy(gameObject);

                return;
            }

            _Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        public void RegisterPersistentScene(string sceneName)
        {
            if (!PersistentScenes.Contains(sceneName))
                PersistentScenes.Add(sceneName);
        }

        public IEnumerator InitializePersistentScenes()
        {
            foreach (string sceneName in PersistentScenes)
                yield return LoadSceneAdditive(sceneName);
        }

        public IEnumerator LoadSceneAdditive(string sceneName)
        {
            if (IsSceneLoaded(sceneName)) {
                RefreshActiveScene();

                if (IsSceneLoaded(sceneName))
                    yield break;
            }
            UnityEngine.AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            yield return new WaitUntil(() => op.isDone);

            _LoadedScenes.Add(sceneName);
        }

        private void RefreshActiveScene()
        {
            _LoadedScenes.Clear();

            int count = SceneManager.sceneCount;

            for (int i = 0; i < count; i++) {
                UnityScene scene = SceneManager.GetSceneAt(i);

                if (PersistentScenes.Contains(scene.name))
                    continue;
                _LoadedScenes.Add(scene.name);
            }
        }

        public IEnumerator UnloadScene(string sceneName)
        {
            if (PersistentScenes.Contains(sceneName))
                yield break;
            if (!IsSceneLoaded(sceneName))
                yield break;
            UnityEngine.AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);

            yield return new WaitUntil(() => op.isDone);

            _LoadedScenes.Remove(sceneName);
        }

        public void SwitchToScene(string sceneName)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public void ApplyPersistentSceneRenderSettings()
        {
            foreach (string sceneName in PersistentScenes) {
                UnityScene scene = SceneManager.GetSceneByName(sceneName);

                if (!scene.isLoaded)
                    continue;
                foreach (var root in scene.GetRootGameObjects()) {
                    var skybox = root.GetComponentInChildren<SkyboxEvolution>(true);

                    if (skybox != null)
                        skybox.InitSkyboxSettings();
                    var volume = root.GetComponentInChildren<UnityEngine.Rendering.Volume>(true);

                    if (volume != null)
                        volume.enabled = true;
                }
            }
        }

        #endregion
    }
}
