using System;

namespace OMGG.Network.Fusion {

    public class FusionSessionManager : ISessionManager {

        public string SessionId => throw new NotImplementedException();

        public event Action OnSessionCreated;
        public event Action OnSessionJoined;
        public event Action OnSessionLeft;

        public void CreateSession(string sessionName, RoomOptions options)
        {
            throw new NotImplementedException();
        }

        public void JoinSession(string sessionId)
        {
            throw new NotImplementedException();
        }

        public void LeaveSession()
        {
            throw new NotImplementedException();
        }
    }
}
