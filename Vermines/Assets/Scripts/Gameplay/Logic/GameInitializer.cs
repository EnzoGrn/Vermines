using UnityEngine.SceneManagement;
using Fusion;

namespace Vermines {

    public class GameInitializer : NetworkBehaviour {

        #region Overrides Methods

        public override void Spawned()
        {
            if (HasStateAuthority) {
                Runner.LoadScene("GameplayCameraTravelling" , LoadSceneMode.Additive); // Scene that active the camera travelling with spline on the map.
                Runner.LoadScene("UI"                       , LoadSceneMode.Additive); // Scene that contains the UI elements for the game.
                Runner.LoadScene("FinalAnimation"           , LoadSceneMode.Additive); // Scene that active the final animation when the game is over.
            }
        }

        #endregion
    }
}
