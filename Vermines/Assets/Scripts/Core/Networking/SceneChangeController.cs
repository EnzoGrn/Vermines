
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Core.Network
{

    using UnityScene = UnityEngine.SceneManagement.Scene;

    public class SceneChangeController : ContextBehaviour
    {

        #region Attributes

        private readonly Dictionary<string, HashSet<PlayerRef>> _Acks = new();
        private readonly Dictionary<string, Coroutine> _AckTimeouts = new();

        private const float ACK_TIMEOUT = 15f;

        #endregion

        #region Methods

        private IEnumerator HostPerformSceneChangeCoroutine(string scenePath, bool isCustom, bool isGameSession, GameplayType gameplay, string oldScene, int playerConnected, string data)
        {
            int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (sceneIndex < 0)
            {
                Debug.LogError($"Scene '{scenePath}' not in build settings.");

                yield break;
            }

            var sceneRef = SceneRef.FromIndex(sceneIndex);
            var loadTask = Runner.LoadScene(sceneRef, LoadSceneMode.Additive);

            while (!loadTask.IsDone)
                yield return null;
            if (!loadTask.IsValid)
            {
                Debug.LogError($"Runner.LoadScene failed for {scenePath}");

                yield break;
            }

            if (!_Acks.ContainsKey(oldScene))
                _Acks[oldScene] = new HashSet<PlayerRef>();

            if (_AckTimeouts.TryGetValue(oldScene, out Coroutine running) && running != null)
                StopCoroutine(running);
            _AckTimeouts[oldScene] = StartCoroutine(AckTimeoutCoroutine(oldScene));

            RPC_ApplySceneChange(scenePath, isCustom, isGameSession, gameplay, oldScene, playerConnected, data);
        }

        private IEnumerator AckTimeoutCoroutine(string scenePath)
        {
            float elapsed = 0f;

            while (elapsed < ACK_TIMEOUT && _Acks.ContainsKey(scenePath))
            {
                elapsed += Time.unscaledDeltaTime;

                yield return null;
            }

            if (_Acks.ContainsKey(scenePath))
            {
                Debug.LogWarning($"[SceneChange] Ack timeout pour '{scenePath}' ({_Acks[scenePath].Count} acks reçus) — déchargement forcé.");

                FinalizeSceneUnload(scenePath);
            }
        }

        private void FinalizeSceneUnload(string scenePath)
        {
            if (!HasStateAuthority)
                return;
            if (!_Acks.ContainsKey(scenePath))
                return;

            _Acks.Remove(scenePath);

            if (_AckTimeouts.TryGetValue(scenePath, out Coroutine running))
            {
                if (running != null)
                    StopCoroutine(running);
                _AckTimeouts.Remove(scenePath);
            }

            Runner.UnloadScene(scenePath);
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
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
        public void RPC_SceneLoadedAck(string scenePath, int playerActive, RpcInfo info = default)
        {
            PlayerRef source = info.Source == PlayerRef.None ? Runner.LocalPlayer : info.Source;

            if (!_Acks.TryGetValue(scenePath, out HashSet<PlayerRef> acks))
            {
                acks = new HashSet<PlayerRef>();
                _Acks[scenePath] = acks;
            }

            acks.Add(source);
            int expected = Runner.ActivePlayers.Count();

            if (acks.Count >= expected)
                FinalizeSceneUnload(scenePath);
        }

        #endregion
    }
}
