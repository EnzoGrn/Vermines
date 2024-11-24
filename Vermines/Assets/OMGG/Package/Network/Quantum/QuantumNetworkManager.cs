using System;

namespace OMGG.Network.Quantum {

    public class QuantumNetworkManager : INetworkManager {

        public bool IsConnected => throw new NotImplementedException();

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        public void Connect(string address, string auth)
        {
            throw new NotImplementedException();
        }

        public void Connect(string address, int port)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
