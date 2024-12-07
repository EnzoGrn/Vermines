using OMGG.Network.Fusion;
using OMGG.Network;
using UnityEngine;
using System;
using OMGG.Optimizer;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour, INetworkRoomView {

    /*
     * @brief Network manager for handle logic of the network.
     */
    private INetworkRoom _NetworkRoom;

    /*
     * @brief Presenter for the NetworkManager.
     */
    private NetworkRoomPresenter _NetworkPresenter;

    /*
     * @brief Initialize the network manager and the presenter.
     */
    private void Awake()
    {
        // Initialize the network manager and the presenter.
        _NetworkRoom = new FusionNetworkRoom();

        // Binding the events for the network callbacks.
        _NetworkRoom.OnPlayerJoined += ShowPlayerJoined;
        _NetworkRoom.OnPlayerLeft   += ShowPlayerLeft;

        // Link the presenter with the network manager.
        _NetworkPresenter = new(_NetworkRoom, this);
    }

    public void CreateRoom()
    {
        _NetworkPresenter.CreateRoom("Room #5555", new RoomOptions() {
            RoomName = "Room #5555"
        });

        SceneManager.LoadScene("Game");
    }

    public void JoinRoom()
    {
        _NetworkPresenter.JoinRoom("Room #5555");
    }

    /*
     * @brief Show a message in the console or window.
     */
    public void ShowMessage(string message)
    {
        Debug.Log("[RoomController]: " + message);
    }

    /*
     * @brief Show player's data information, when it's connected
     */
    public void ShowPlayerJoined(IPlayer player)
    {
        Debug.Log("[RoomController]: Player joined: " + player.PlayerName + " #" + player.PlayerId);
    }

    /*
     * @brief Show player's data information, when he leave.
     */
    public void ShowPlayerLeft(IPlayer player)
    {
        Debug.Log("[RoomController]: Player left: " + player.PlayerName + " #" + player.PlayerId);
    }
}
