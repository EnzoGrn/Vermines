using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

namespace Vermines.Menu {

    using UnityScene = UnityEngine.SceneManagement.Scene;

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

        protected override void OnDeinitialize()
        {
            base.OnDeinitialize();
        }

        #endregion
    }
}
