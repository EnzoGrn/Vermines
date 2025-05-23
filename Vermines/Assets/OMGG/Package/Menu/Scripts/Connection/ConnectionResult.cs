
using System.Threading.Tasks;

namespace OMGG.Menu.Connection {

    /// <summary>
    /// Connection result info object.
    /// </summary>
    public partial class ConnectResult {

        /// <summary>
        /// Is successful
        /// </summary>
        public bool Success;

        /// <summary>
        /// The fail reason code <see cref="ConnectFailReason"/>
        /// </summary>
        public int FailReason;

        /// <summary>
        /// Another custom code that can be filled by out by RealtimeClient.DisconnectCause for example.
        /// </summary>
        public int DisconnectCause;

        /// <summary>
        /// A debug message.
        /// </summary>
        public string DebugMessage;

        /// <summary>
        /// Data send with the connection result.
        /// </summary>
        public string Data;

        /// <summary>
        /// Set to true to disable all error handling by the menu.
        /// </summary>
        public bool CustomResultHandling;

        /// <summary>
        /// An optional task to signal the menu to wait until cleanup operation have completed (e.g. level unloading).
        /// </summary>
        public Task WaitForCleanup;

        public static ConnectResult Ok()
        {
            return new ConnectResult {
                Success = true
            };
        }

        public static ConnectResult Fail(int failReason, string debugMessage = null, Task waitForCleanup = null)
        {
            return new ConnectResult {
                Success        = false,
                FailReason     = failReason,
                DebugMessage   = debugMessage,
                WaitForCleanup = waitForCleanup
            };
        }
    }

    /// <summary>
    /// Is used to convey some information about a connection error back to the caller.
    /// Is not an enum to allow SDK implementation to add errors.
    /// </summary>
    public partial class ConnectFailReason {

        /// <summary>
        /// No reason code available.
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// User requested cancellation or disconnect.
        /// </summary>
        public const int UserRequest = 1;

        /// <summary>
        /// App or Editor closed
        /// </summary>
        public const int ApplicationQuit = 2;

        /// <summary>
        /// Connection disconnected.
        /// </summary>
        public const int Disconnect = 3;

        /// <summary>
        /// Argument error.
        /// </summary>
        public const int ArgumentError = 4;
    }
}
