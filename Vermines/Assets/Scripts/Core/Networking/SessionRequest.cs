using Fusion;

namespace Vermines.Core.Network {

    public struct SessionRequest {

        public string UserID;

        public GameMode GameMode;

        public string DisplayName;
        public string SessionName;
        public string ScenePath;
        public string CustomLobby;

        public int MaxPlayers;
        public int ExtraPeers;

        public string IPAddress;
        public ushort Port;
    }
}
