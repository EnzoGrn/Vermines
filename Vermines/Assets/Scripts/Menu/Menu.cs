using UnityEngine;
using Fusion;

namespace Vermines.Menu {

    using Vermines.Core.Scene;
    using Vermines.Core.Services;

    public class Menu : Scene {

        #region Attributes

        [SerializeField]
        private string _SceneToLoad;

        #endregion

        #region Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            StartCoroutine(PersistentSceneService.Instance.LoadSceneAdditive(_SceneToLoad));

            NetworkObject[] noArray = FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (noArray == null || noArray.Length == 0)
                return;
            for (int i = 0; i < noArray.Length; i++)
            {
                if (noArray[i] != null)
                    Destroy(noArray[i]);
            }
        }

        #endregion
    }
}
