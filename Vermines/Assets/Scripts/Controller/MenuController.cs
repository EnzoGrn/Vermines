using OMGG.Network.Fusion;
using OMGG.Optimizer;
using OMGG.Network;
using UnityEngine;
using Fusion;
using TMPro;

public class MenuController : MonoBehaviour, INetworkManagerView, IFixedUpdateObserver {

    /*
     * @brief Network manager for handle logic of the network.
     */
    private INetworkManager _NetworkManager;

    /*
     * @brief Presenter for the NetworkManager.
     */
    private NetworkManagerPresenter _NetworkPresenter;

    /*
     * @brief Initialize the network manager and the presenter.
     */
    private void Awake()
    {
        // Create the NetworkRunner object.
        GameObject  runnerGO = new("NetworkRunner");
        NetworkRunner runner = runnerGO.AddComponent<NetworkRunner>();

        // Initialize the network manager and the presenter.
        _NetworkManager = new FusionNetworkManager(runner, "Vermines");

        // Binding the events for the network callbacks.
        _NetworkManager.OnConnected    += () => ShowMessage("Connected to the server.");
        _NetworkManager.OnDisconnected += () => ShowMessage("Disconnected from the server.");
        _NetworkManager.OnError        += ShowError;

        // Link the presenter with the network manager.
        _NetworkPresenter = new(_NetworkManager, this);

        // Register the observer for the fixed update. (Optimization)
        FixedUpdateManager.Instance.RegisterObserver(this);
    }

    /*
     * @brief Start the connection to the server.
     */
    private void Start()
    {
        _NetworkPresenter.StartConnection();
    }

    /*
     * @brief Show a message on the screen.
     */
    public void ShowMessage(string message)
    {
        _StatusMessage = message;
    }

    /*
     * @brief Show an error message on the screen.
     */
    public void ShowError(string message)
    {
        _StatusMessage = message;
    }

    #region UI

    /*
     * @brief Text component for display the status of the connection.
     */
    [SerializeField]
    private TMP_Text _StatusText;

    [SerializeField]
    private string _StatusMessage;

    public void ObservedFixedUpdate()
    {
        if (_StatusText != null && _StatusText.text != _StatusMessage)
            _StatusText.SetText(_StatusMessage);
    }

    #endregion
}
