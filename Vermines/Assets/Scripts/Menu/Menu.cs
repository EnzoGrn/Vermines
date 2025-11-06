using UnityEngine;

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
        }

        #endregion
    }
}
