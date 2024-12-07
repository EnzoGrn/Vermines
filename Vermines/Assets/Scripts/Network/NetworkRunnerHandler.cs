using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Sockets;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour {

    public  NetworkRunner NetworkRunnerPrefab; // !< The prefab of the network runner.
    private NetworkRunner _NetworkRunner;     // !< The network runner instance.

    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindFirstObjectByType<NetworkRunner>();

        if (networkRunnerInScene != null)
            _NetworkRunner = networkRunnerInScene;
    }

    private void Start()
    {
        if (_NetworkRunner == null) {
            _NetworkRunner = Instantiate(NetworkRunnerPrefab);
            _NetworkRunner.name = "Network Runner";

            if (SceneManager.GetActiveScene().name != "Main Menu") {
                Task clientTask = InitializeNetworkRunner(_NetworkRunner, GameMode.AutoHostOrClient, "Test Session", GameManager.Instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex);
            }

            Debug.Log($"Server NetworkRunner started.");
        }
    }

    private INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        INetworkSceneManager sceneManager = runner.GetComponents<MonoBehaviour>().OfType<INetworkSceneManager>().FirstOrDefault();

        sceneManager ??= runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode mode, string sessionName, byte[] token, NetAddress address, int scene)
    {
        SceneRef sceneRef = SceneRef.FromIndex(scene);

        if (sceneRef.IsValid == false)
            Debug.LogError($"Invalid scene index: {scene}");
        INetworkSceneManager sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs {
            GameMode = mode,
            Address = address,
            Scene = sceneRef,
            SessionName = sessionName,
            CustomLobbyName = "Our Lobby ID",
            SceneManager = sceneManager,
            ConnectionToken = token
        });
    }

    protected virtual Task InitializeNetworkRunnerHostMigration(NetworkRunner runner, HostMigrationToken token)
    {
        INetworkSceneManager sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs {
            SceneManager = sceneManager,
            HostMigrationToken = token,              // Contains all information for restart the runner.
            HostMigrationResume = HostMigrationResume // This will be called when the host migration is completed.
        });
    }

    public void StartHostMigration(HostMigrationToken token)
    {
        // Create a new network runner, old one is being shutdown.
        _NetworkRunner = Instantiate(NetworkRunnerPrefab);
        _NetworkRunner.name = "Network Runner - Migrated";

        var clientTask = InitializeNetworkRunnerHostMigration(_NetworkRunner, token);

        Debug.Log("Host migration started.");
    }

    private void HostMigrationResume(NetworkRunner runner)
    {
        Debug.Log($"Host migration resumed started.");

        // Get a reference to each Network Object from the old runner.
        foreach (NetworkObject resumeNetworkObject in runner.GetResumeSnapshotNetworkObjects()) {
            // https://youtu.be/0JiODxetZoY?si=p5ytkvB3GH6rsh_z to understand the loop with Player.
            // 13:45
        }

        Debug.Log($"Host migration resumed completed.");
    }

    public void OnJoinLobby()
    {
        Task clientTask = JoinLobby();
    }

    private async Task JoinLobby()
    {
        Debug.Log("Joining lobby...");

        string lobbyID = "Our Lobby ID";

        var result = await _NetworkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if (!result.Ok)
            Debug.LogError($"Unable to join lobby: {lobbyID}");
        else
            Debug.Log($"Joined lobby: {lobbyID}");
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log($"Create session {sessionName} on scene {sceneName} (build index: {SceneUtility.GetBuildIndexByScenePath($"Resources/Scenes/{sceneName}")})");

        Task clientTask = InitializeNetworkRunner(_NetworkRunner, GameMode.Host, sessionName, GameManager.Instance.GetConnectionToken(), NetAddress.Any(), SceneUtility.GetBuildIndexByScenePath($"Resources/Scenes/{sceneName}"));
    }

    public void JoinGame(string sessionName)
    {
        Debug.Log($"Join session {sessionName}");

        Task clientTask = InitializeNetworkRunner(_NetworkRunner, GameMode.Client, sessionName, GameManager.Instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex);
    }
}
