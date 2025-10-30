using UnityEngine;
using Fusion;

namespace Vermines.Core.Scene {
    using Vermines.Characters;
    using Vermines.Core.Player;
    using Vermines.Core.Services;
    using Vermines.Core.Settings;
    using Vermines.Core.UI;
    using Vermines.Menu.CustomLobby;

    [System.Serializable]
    public class SceneContext {

        #region Player

        [HideInInspector]
        public string PeerUserID;

        [HideInInspector]
        public PlayerData PlayerData;

        [HideInInspector]
        public Cultist PlayerCultist = default;

        #endregion

        #region General

        public SceneAudio Audio;
        public SceneUI UI;
        public ObjectCache ObjectCache;
        public SceneInput SceneInput;

        #endregion

        #region Settings

        [HideInInspector]
        public GlobalSettings Settings;

        [HideInInspector]
        public RuntimeSettings RuntimeSettings;

        #endregion

        #region Scene

        public string CustomGameScenePath;
        public string GameScenePath;

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

        public NetworkGame NetworkGame;
        [HideInInspector]
        public GameplayMode GameplayMode;

        public NetworkLobby NetworkLobby;
        [HideInInspector]
        public LobbyManager Lobby;

        #endregion

        #region Menu

        public Matchmaking Matchmaking;

        #endregion
    }
}
