using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Core.Network {

    using Vermines.Core.Scene;
    using Vermines.Extension;

    public class NetworkObjectPool : INetworkObjectProvider {

        public SceneContext Context { get; set; }

        private Dictionary<NetworkPrefabId, Stack<NetworkObject>> _Cached = new(32);
        private Dictionary<NetworkObject, NetworkPrefabId>      _Borrowed = new();

        NetworkObjectAcquireResult INetworkObjectProvider.AcquirePrefabInstance(Fusion.NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
        {
            if (!_Cached.TryGetValue(context.PrefabId, out Stack<NetworkObject> objects))
                objects = _Cached[context.PrefabId] = new Stack<NetworkObject>();
            if (objects.Count > 0) {
                var oldInstance = objects.Pop();

                _Borrowed[oldInstance] = context.PrefabId;

                oldInstance.SetActive(true);

                result = oldInstance;

                runner.MoveToRunnerScene(result);

                return NetworkObjectAcquireResult.Success;
            }
            NetworkObject original = runner.Config.PrefabTable.Load(context.PrefabId, true);

            if (original == null) {
                result = default;

                return NetworkObjectAcquireResult.Failed;
            }
            var instance = Object.Instantiate(original);

            _Borrowed[instance] = context.PrefabId;

            AssignContext(instance);

            for (int i = 0; i < instance.NestedObjects.Length; i++)
                AssignContext(instance.NestedObjects[i]);
            result = instance;

            runner.MoveToRunnerScene(result);

            return NetworkObjectAcquireResult.Success;
        }

        void INetworkObjectProvider.ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
        {
            if (context.IsNestedObject)
                return;
            NetworkObject instance = context.Object;

            if (instance == null)
                return;
            // Not Compatible with Network Scene changement.
            /*if (!instance.NetworkTypeId.IsSceneObject && !runner.IsShutdown) {
                if (_Borrowed.TryGetValue(instance, out NetworkPrefabId prefabId)) {
                    _Borrowed.Remove(instance);

                    _Cached[prefabId].Push(instance);

                    instance.SetActive(false);

                    instance.transform.parent = null;
                    instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                } else {
                    Object.Destroy(instance.gameObject);
                }
            } else {
                Object.Destroy(instance.gameObject);
            }*/

            Object.Destroy(instance.gameObject);
        }

        public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
        {
            return runner.Prefabs.GetId(prefabGuid);
        }

        private void AssignContext(NetworkObject instance)
        {
            int count = instance.NetworkedBehaviours.Length;

            for (int i = 0; i < count; i++) {
                if (instance.NetworkedBehaviours[i] is IContextBehaviour cachedBehaviour)
                    cachedBehaviour.Context = Context;
            }
        }
    }
}
