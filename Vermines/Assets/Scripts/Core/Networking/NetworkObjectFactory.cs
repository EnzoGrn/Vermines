using UnityEngine;
using Fusion;

namespace Vermines.Core.Network
{

    using Vermines.Core.Scene;
    using Vermines.Extension;

    public class NetworkObjectFactory : INetworkObjectProvider
    {

        public SceneContext Context { get; set; }

        NetworkObjectAcquireResult INetworkObjectProvider.AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
        {
            NetworkObject original = runner.Config.PrefabTable.Load(context.PrefabId, true);

            if (original == null)
            {
                result = default;

                return NetworkObjectAcquireResult.Failed;
            }

            NetworkObject instance = Object.Instantiate(original);

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

            Object.Destroy(instance.gameObject);
        }

        public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
        {
            return runner.Prefabs.GetId(prefabGuid);
        }

        private void AssignContext(NetworkObject instance)
        {
            int count = instance.NetworkedBehaviours.Length;

            for (int i = 0; i < count; i++)
            {
                if (instance.NetworkedBehaviours[i] is IContextBehaviour cachedBehaviour)
                    cachedBehaviour.Context = Context;
            }
        }
    }
}