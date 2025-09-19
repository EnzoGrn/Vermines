using Fusion;
using System;

namespace Vermines.Gameplay.Errors {

    [Serializable]
    public struct GameActionError : INetworkStruct {

        /// <summary>
        /// Error gravity (Minor, Major, Critical).
        /// </summary>
        public ErrorSeverity Severity;

        /// <summary>
        /// Scope of the error: Local (single player) or Global (all players).
        /// </summary>
        public ErrorScope Scope;

        /// <summary>
        /// Location of the error for the UI (i.g. Shop, Sacrifice Phase...)
        /// </summary>
        public ErrorLocation Location;

        /// <summary>
        /// Readable message to display to the player or as a log.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The message should be clear and concise, avoiding technical jargon.
        /// </para>
        /// 
        /// <para>
        /// The message must take 128 characters maximum.
        /// If a message exceeds this limit, it will be truncated.
        /// So update this variable to allow more characters if needed.
        /// </para>
        /// </remarks>
        public NetworkString<_128> Message;

        /// <summary>
        /// Concerned player (if applicable).
        /// </summary>
        public PlayerRef Target;
    }
}
