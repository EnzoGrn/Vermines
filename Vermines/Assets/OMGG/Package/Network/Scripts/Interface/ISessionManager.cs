using System;
using System.Collections.Generic;

namespace OMGG.Network {

    /*
     * @brief ISessionManager is an interface for session management.
     * It provides the session management for the network, such as creating, joining, and leaving a session.
     */
    public interface ISessionManager {

        /*
         * @brief SessionId is the session id.
         */
        string SessionId { get; }

        /*
         * @brief CreateSession creates a session with the specified session name and options.
         */
        void CreateSession(string sessionName, RoomOptions options);

        /*
         * @brief JoinSession joins the session with the specified session id.
         */
        void JoinSession(string sessionId);

        /*
         * @brief LeaveSession leaves the session.
         */
        void LeaveSession();

        /*
         * @brief OnSessionCreated is triggered when a session is created.
         */
        event Action OnSessionCreated;

        /*
         * @brief OnSessionJoined is triggered when a session is joined.
         */
        event Action OnSessionJoined;

        /*
         * @brief OnSessionLeft is triggered when a session is left.
         */
        event Action OnSessionLeft;
    }
}
