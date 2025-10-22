using UnityEngine;
using Fusion;

namespace Vermines.Core.Scene {

    using Vermines.Core.Player;
    using Vermines.Core.Settings;
    using Vermines.Core.UI;

    [System.Serializable]
    public class SceneContext {

        #region Player

        [HideInInspector]
        public string PeerUserID;

        [HideInInspector]
        public PlayerData PlayerData;

        #endregion

        #region General

        public SceneUI UI;

        #endregion

        #region Settings

        public GlobalSettings Settings;
        public RuntimeSettings RuntimeSettings;

        #endregion

        #region Gameplay

        [HideInInspector]
        public bool IsVisible;

        [HideInInspector]
        public bool HasInput;

        [HideInInspector]
        public NetworkRunner Runner;

        [HideInInspector]
        public PlayerRef LocalPlayerRef;

        #endregion

        #region Menu

        public Matchmaking Matchmaking;

        #endregion
    }
}
