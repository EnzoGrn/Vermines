using UnityEngine;

/*
 * @brief This abstract class is used to manage the lobby, it inherits from MonoBehaviour.
 */
public abstract class ALobbyManager : MonoBehaviour
{
    #region [SerializeField] Private Attributes
    [SerializeField] protected NetworkLobbyManager _networkLobbyManagerPrefab;
    #endregion

    /*
     * @brief This method is used to start observing events, when the scirpt istance is being loaded.
     * The base is mandatory to use Observable pattern.
     * 
     * @param void
     * 
     * @return void
     */
    protected virtual void Awake()
    {
        if (!TryGetComponent<NetworkLobbyManager>(out _networkLobbyManagerPrefab))
        {
            Debug.LogError("NetworkLobbyManager is not set");
            return;
        }

        _networkLobbyManagerPrefab.OnJoinedLobbyAction += OnClientConnectedToServer;
        _networkLobbyManagerPrefab.OnDisconnectedAction += OnClientDisconnectedFromServer;
    }

    #region Callbacks
    /*
     * @brief This method is an event handler for when the client is connected to the master server and has joined the lobby.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnClientConnectedToServer()
    {
        Debug.Log("Client connected to server");
    }
    /*
     * @brief This method is an event handler for when the client has disconnected from the server.
     * 
     * @param void
     * 
     * @return void
     */
    public virtual void OnClientDisconnectedFromServer()
    {
        Debug.Log("Client disconnected to server");
    }
    #endregion

    /*
     * @brief This method is used to disconnect client from the server, when the scirpt istance is being destroyed.
     * The base is mandatory to use Observable pattern.
     * 
     * @param void
     * 
     * @return void
     */
    protected virtual void OnDestroy()
    {
        _networkLobbyManagerPrefab.OnJoinedLobbyAction -= OnClientConnectedToServer;
        _networkLobbyManagerPrefab.OnDisconnectedAction -= OnClientDisconnectedFromServer;
    }
}
