using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {

    /*
     * @brief Class containing the profile of player in the server side.
     */
    [System.Serializable]
    public class PlayerProfile {

        /*
         * @brief Nickname of the player.
         * It's the nickname created by the server for keep the anonymity of the player.
         */
        public string Nickname = string.Empty;

        /*
         * @brief ID of the player in the server.
         * It's the ID of the player in the server, for recognize the player in the party.
         */
        public int PlayerID = -1;
    }
}
