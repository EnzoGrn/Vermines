using Fusion;

namespace Vermines.Core.Network {

    public struct SessionRequest {

        public string UserID;

        public GameMode GameMode;
        public GameplayType GameplayType;

        public string SessionName;
        public string ScenePath;
        public string CustomLobby;

        public bool IsCustom;
        public bool IsGameSession;

        public int MaxPlayers;
        public int ExtraPeers;

        public string IPAddress;
        public ushort Port;
    }
}
