using UnityEngine;
using Fusion;

namespace Vermines.Core.Scene {
    using Vermines.Core.Network;
    using Vermines.Core.Player;
    using Vermines.Core.Services;
    using Vermines.Core.Settings;
    using Vermines.Core.UI;
    using Vermines.Menu.CustomLobby;
    using Vermines.Menu.Matchmaking;
    using Vermines.UI.Card;

    [System.Serializable]
    public class SceneContext {

        #region Player

        [HideInInspector]
        public string PeerUserID;

        [HideInInspector]
        public PlayerData PlayerData;

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
        public string MatchmakingScenePath;
        public string GameScenePath;

        #endregion

        #region Gameplay

        [HideInInspector]
        public bool IsVisible;

        [HideInInspector]
        public bool HasInput;

        [HideInInspector]
        public NetworkRunner Runner;
        public SceneChangeController SceneChangeController;

        [HideInInspector]
        public PlayerRef LocalPlayerRef;

        public HandManager HandManager;

        public NetworkGame NetworkGame;
        [HideInInspector]
        public GameplayMode GameplayMode;

        public NetworkLobby NetworkLobby;
        [HideInInspector]
        public LobbyManager Lobby;

        public NetworkMatchmaking NetworkMatchmaking;

        #endregion

        #region Menu

        public Matchmaking Matchmaking;

        #endregion
    }
}
