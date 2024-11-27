using System;

namespace OMGG.Network {

    /*
     * @brief LogLevel represents the log level.
     */
    public enum LogLevel {
        Debug,
        Info,
        Warning,
        Error
    }

    /*
     * @brief INetworkLogger is an interface for network logging.
     * It provides the log for all the network processes.
     */
    public interface INetworkLogger {

        /*
         * @brief Log logs the message with the specified log level.
         */
        void Log(string message, LogLevel level = LogLevel.Info);

        /*
         * @brief OnLog is triggered when a log is received.
         */
        event Action<string, LogLevel> OnLog;
    }
}
