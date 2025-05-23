using UnityEngine;

namespace Vermines.GameDesign.Initializer {

    /// <summary>
    /// In-game game design initializer.
    /// This script call a init function to all <see cref="IGGD_Behaviour"/> scripts in the scene.
    /// </summary>
    public class IGGD_Initializer : MonoBehaviour {

        public void Start()
        {
            IGGD_Behaviour[] behaviours = FindObjectsByType<IGGD_Behaviour>(FindObjectsSortMode.None);

            foreach (IGGD_Behaviour behaviour in behaviours)
                behaviour.Init();
        }
    }
}
