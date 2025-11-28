using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Extension {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    using Vermines.Utils;

    public static class SceneExtensions {

        public static T GetComponent<T>(this UnityScene scene, bool includeInactive = false) where T : class
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return default;
            List<GameObject> roots = ListPool<GameObject>.Shared.Get(16);

            scene.GetRootGameObjects(roots);

            T component = default;

            int count = roots.Count;

            for (int i = 0; i < count; i++) {
                component = roots[i].GetComponentInChildren<T>(includeInactive);

                if (component != null)
                    break;
            }

            ListPool<GameObject>.Shared.Return(roots);

            return component;
        }

        public static List<T> GetComponents<T>(this UnityScene scene, bool includeInactive = false) where T : class
        {
            List<T> allComponents    = new();
            List<T> objectComponents = ListPool<T>.Shared.Get(16);
            List<GameObject>   roots = ListPool<GameObject>.Shared.Get(16);

            if (!scene.IsValid() || !scene.isLoaded)
                return allComponents;
            scene.GetRootGameObjects(roots);

            foreach (GameObject root in roots) {
                root.GetComponentsInChildren(includeInactive, objectComponents);

                if (objectComponents.Count > 0)
                    allComponents.AddRange(objectComponents);
                objectComponents.Clear();
            }

            ListPool<GameObject>.Shared.Return(roots);
            ListPool<T>.Shared.Return(objectComponents);

            return allComponents;
        }
    }
}
