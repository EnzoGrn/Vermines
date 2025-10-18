using Fusion;
using Vermines.Core.Player;

namespace Vermines.Core.Scene {

    [System.Serializable]
    public class SceneContext {

        #region Player

        public string PeerUserID;

        public PlayerData PlayerData;

        #endregion

        public NetworkRunner Runner;

        public bool IsVisible;
        public bool HasInput;
    }
}
