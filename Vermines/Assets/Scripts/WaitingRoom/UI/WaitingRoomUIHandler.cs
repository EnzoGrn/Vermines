using Fusion;
using UnityEngine;
using Vermines.Player;

public class WaitingRoomUiHandler : MonoBehaviour
{
    [Header("Ui")]
    [SerializeField] private PlayerListUiHandler _PlayerList;

    public void AddPlayerList(WaitingRoomPlayerData playerData, bool isMe)
    {
        _PlayerList.AddToList(playerData, isMe);
    }

    public void ClearList()
    {
        _PlayerList.ClearList();
    }
}
