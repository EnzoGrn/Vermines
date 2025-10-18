namespace Vermines.Core.Network {

    public class Session {

        public bool ConnectionRequested;
        public GamePeer[] GamePeers;

        public bool IsConnected
        {
            get
            {
                if (GamePeers == null && GamePeers.Length == 0)
                    return false;
                for (int i = 0; i < GamePeers.Length; i++) {
                    if (!GamePeers[i].IsConnected)
                        return false;
                }

                return true;
            }
        }
    }
}
