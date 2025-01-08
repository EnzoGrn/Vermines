using System;
using UnityEngine;

namespace Utils {

    public static class ConnectionToken {

        /// <summary>
        /// Create new random token.
        /// </summary>
        public static byte[] NewToken() => Guid.NewGuid().ToByteArray();

        /// <summary>
        /// Converts a token into a Hash format.
        /// </summary>
        /// <param name="token">Token to be hashed</param>
        /// <returns>Token hash</returns>
        public static int HashToken(byte[] token) => new Guid(token).GetHashCode();

        /// <summary>
        /// Converts a token into a string format.
        /// </summary>
        /// <param name="token">Token to be parsed</param>
        /// <returns>Token as a string</returns>
        public static string TokenToString(byte[] token) => new Guid(token).ToString();
    }
}
