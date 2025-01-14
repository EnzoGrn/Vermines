using Fusion;

namespace Vermines.Network.Utilities {

    static public class NetworkNameTools {

        /// <summary>
        /// Give to a networking object a name depending on its authority.
        /// </summary>
        /// <example>
        /// Name will be displayed like:
        /// "[Player:1] (Input Authority)"
        /// "[Player:2] (State Authority)"
        /// "[Player:3] (Proxy)"
        /// </example>
        /// <note>
        /// Input Authority is a client that has authority over the object's input
        /// State Authority is a client that has authority over the object's state
        /// Proxy is a client that doesn't have authority over the object
        /// </note>
        /// <param name="player">The reference to player (in network)</param>
        /// <param name="hasInputAuthority">If he has a input authority</param>
        /// <param name="hasStateAuthority">If he has a state authority</param>
        /// <param name="extra">Extra string, for more details on name</param>
        /// <returns>The name of the object</returns>
        static public string GiveNetworkingObjectName(PlayerRef player, bool hasInputAuthority, bool hasStateAuthority, string extra = null)
        {
            string name = $"{player} ({(hasInputAuthority ? "Input Authority" : (hasStateAuthority ? "State Authority" : "Proxy"))})";

            if (extra != null)
                name += $" ({extra})";
            return name;
        }
    }
}
