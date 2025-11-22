using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Core.Network {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    public class SceneChangeController : ContextBehaviour {

        #region Attributes

        private int _Acknoledgements = 0;

        #endregion

        #region Methods

        private IEnumerator HostPerformSceneChangeCoroutine(string scenePath, bool isCustom, bool isGameSession, GameplayType gameplay, string oldScene, int playerConnected, string data)
        {
            int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (sceneIndex < 0) {
                Debug.LogError($"Scene '{scenePath}' not in build settings.");

                yield break;
            }

            var sceneRef = SceneRef.FromIndex(sceneIndex);
            var loadTask = Runner.LoadScene(sceneRef, LoadSceneMode.Additive);

            while (!loadTask.IsDone)
                yield return null;
            if (!loadTask.IsValid) {
                Debug.LogError($"Runner.LoadScene failed for {scenePath}");

                yield break;
            }

            RPC_ApplySceneChange(scenePath, isCustom, isGameSession, gameplay, oldScene, playerConnected, data);
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_RequestSceneChange(string scenePath, bool isCustom, bool isGameSession, GameplayType gameplay, string oldScene, int playerConnected, string data = "")
        {
            StartCoroutine(HostPerformSceneChangeCoroutine(scenePath, isCustom, isGameSession, gameplay, oldScene, playerConnected, data));
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void RPC_ApplySceneChange(string scenePath, bool isCustom, bool isGameSession, GameplayType gameplay, string oldScene, int playerConnected, string data)
        {
            StartCoroutine(Global.Networking.ApplySceneChangeLocalCoroutine(Context.PeerUserID, scenePath, oldScene, isCustom, isGameSession, gameplay, data,
                () => RPC_SceneLoadedAck(oldScene, playerConnected)
            ));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_SceneLoadedAck(string scenePath, int playerActive)
        {
            _Acknoledgements++;

            if (_Acknoledgements >= playerActive) {
                var op = Runner.UnloadScene(scenePath);

                StartCoroutine(new WaitUntil(() => op.IsDone));

                _Acknoledgements = 0;
            }
        }

        #endregion
    }
}
