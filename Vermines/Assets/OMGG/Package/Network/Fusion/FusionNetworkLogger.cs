using System;

namespace OMGG.Network.Fusion {

    public class FusionNetworkLogger : INetworkLogger {

        public event Action<string, LogLevel> OnLog;

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            throw new NotImplementedException();
        }
    }
}
