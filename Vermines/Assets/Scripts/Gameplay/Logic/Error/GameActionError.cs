using UnityEngine;
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
        /// The message key for localization systems.
        /// </summary>
        public NetworkString<_32> MessageKey;

        /// <summary>
        /// The dynamic arguments for the message (if applicable).
        /// </summary>
        /// <remarks>
        /// <para>
        /// It can contain up to 4 arguments, depending on the message.
        /// </para>
        /// </remarks>
        public GameActionErrorArgs MessageArgs;

        /// <summary>
        /// Concerned player (if applicable).
        /// </summary>
        public PlayerRef Target;
    }

    /// <summary>
    /// Network structure used to pass dynamic arguments for error messages.
    /// This structure can hold up to 4 string arguments.
    /// </summary>
    [Serializable]
    public struct GameActionErrorArgs : INetworkStruct {

        public NetworkString<_16> Arg0;
        public NetworkString<_16> Arg1;
        public NetworkString<_16> Arg2;
        public NetworkString<_16> Arg3;

        public GameActionErrorArgs(string[] args)
        {
            if (args.Length > 4) {
                Debug.LogWarning(
                    $"GameActionErrorArgs only supports up to 4 arguments, but {args.Length} were provided." +
                     " Extra arguments will be ignored."
                );
            }

            Arg0 = args.Length > 0 ? args[0] : "";
            Arg1 = args.Length > 1 ? args[1] : "";
            Arg2 = args.Length > 2 ? args[2] : "";
            Arg3 = args.Length > 3 ? args[3] : "";
        }

        public GameActionErrorArgs(string arg0 = "", string arg1 = "", string arg2 = "", string arg3 = "")
        {
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public string[] ToArray()
        {
            return new string[] {
                Arg0.ToString(),
                Arg1.ToString(),
                Arg2.ToString(),
                Arg3.ToString()
            };
        }

        public void Set(int index, string value)
        {
            switch (index) {
                case 0:
                    Arg0 = value;
                    break;
                case 1:
                    Arg1 = value;
                    break;
                case 2:
                    Arg2 = value;
                    break;
                case 3:
                    Arg3 = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("GameActionErrorArgs only supports indices 0 to 3.");
            }
        }
    }
}
