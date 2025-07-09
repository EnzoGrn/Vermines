using UnityEngine;

namespace Vermines.GameDesign.Initializer {

    /// <summary>
    /// In-game game design behaviour.
    /// This is an interface for all game design scripts that need to be initialized at the start of the game by the <see cref="IGGD_Initializer"/>.
    /// </summary>
    public abstract class IGGD_Behaviour : MonoBehaviour {

        /// <summary>
        /// Initialize the game design script.
        /// </summary>
        public virtual void Init() { }
    }
}
